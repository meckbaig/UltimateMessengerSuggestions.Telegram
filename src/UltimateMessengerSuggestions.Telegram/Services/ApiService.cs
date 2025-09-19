using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UltimateMessengerSuggestions.Telegram.Common.Options;
using UltimateMessengerSuggestions.Telegram.DbContexts;
using UltimateMessengerSuggestions.Telegram.Extensions;
using UltimateMessengerSuggestions.Telegram.Models.Dtos;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class ApiService : IApiService
{
	private readonly HttpClient _http = new();
	private readonly ConnectionStringsOptions _options;
	private readonly IAppDbContext _context;
	private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	public ApiService(IOptions<ConnectionStringsOptions> options, IAppDbContext context)
	{
		_options = options.Value;
		_context = context;
	}

	public async Task<List<MediaFileDto>> GetAsync(string jwt, string userFiltersString, CancellationToken cancellationToken = default)
	{
		_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
		var filters = FilterParser.Parse(userFiltersString, FilterParser.CommandType.Get);
		string query = $"{_options.Api.TrimEnd('/')}/v1/media?orderBy=id desc&take=5&";
		if (filters.Any())
			query += "&" + string.Join("&", filters.Select(f => "filters=" + Uri.EscapeDataString($"{f.Key}:{f.Value}")));

		var response = await _http.GetAsync(query, cancellationToken);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<GetResponse>(json, _jsonOptions);
		return data?.Items ?? [];
	}

	public async Task<List<MediaFileDto>> GetSuggestionsAsync(string jwt, string searchString, CancellationToken cancellationToken = default)
	{
		_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

		var response = await _http.GetAsync($"{_options.Api.TrimEnd('/')}/v1/suggestions?searchString={Uri.EscapeDataString(searchString)}&client=tg", cancellationToken);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<GetResponse>(json, _jsonOptions);
		return data?.Items ?? [];
	}

	public async Task<string?> LoginAsync(string messengerId, CancellationToken cancellationToken = default)
	{
		var account = await _context.MessengerAccounts
			.Include(a => a.User)
			.FirstOrDefaultAsync(u =>
					u.MessengerId == messengerId &&
					u.Client == "tg",
				cancellationToken);
		if (account == null)
			return null;
		return await RegisterAsync(account.User.UserHash, messengerId, cancellationToken);
	}

	public async Task<string?> RegisterAsync(string userHash, string messengerId, CancellationToken cancellationToken = default)
	{
		var registerRequest = new RegisterRequest
		{
			UserHash = userHash,
			MessengerId = messengerId
		};
		var content = JsonContent.Create(registerRequest, options: _jsonOptions);
		var response = await _http.PostAsync($"{_options.Api.TrimEnd('/')}/v1/auth", content, cancellationToken);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<RegisterResponse>(json, _jsonOptions);
		return data?.Token;
	}

	public async Task<bool> UpdateAsync(string jwt, string mediaId, string userFiltersString, CancellationToken cancellationToken = default)
	{
		var mediaToEdit = (await GetAsync(jwt, $"i:{mediaId}", cancellationToken)).Single();
		var filters = FilterParser.Parse(userFiltersString, FilterParser.CommandType.Edit);
		var newDescription = mediaToEdit.Description;
		var newTags = mediaToEdit.Tags;
		var newIsPublic = mediaToEdit.IsPublic;
		if (filters.TryGetValue("description", out string desc))
		{
			newDescription = desc;
		}
		if (filters.TryGetValue("tags", out string tagsStr))
		{
			newTags = tagsStr
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.ToList();
		}
		if (filters.TryGetValue("isPublic", out string isPublicStr))
		{
			newIsPublic = bool.TryParse(isPublicStr, out var p) && p;
		}
		var editedMedia = new EditMediaFileDto(mediaToEdit.MediaType, mediaToEdit.MediaUrl, newIsPublic, newDescription, newTags);

		var content = JsonContent.Create(new EditMediaRequest { MediaFile = editedMedia }, options: _jsonOptions);
		var response = await _http.PutAsync($"{_options.Api.TrimEnd('/')}/v1/media/{mediaId}", content, cancellationToken);
		response.EnsureSuccessStatusCode();

		return response.IsSuccessStatusCode;
	}

	public async Task<string?> UploadMediaAsync(string jwt, Stream fileStream, string fileName, string mediaType, CancellationToken cancellationToken = default)
	{
		using var content = new MultipartFormDataContent
		{
			{ new StreamContent(fileStream), "File", fileName },
			{ new StringContent(mediaType), "MediaType" }
		};

		var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.Api.TrimEnd('/')}/v1/media/upload")
		{
			Content = content
		};
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

		var response = await _http.SendAsync(request);
		if (!response.IsSuccessStatusCode) return null;

		using var stream = await response.Content.ReadAsStreamAsync();
		var doc = await JsonDocument.ParseAsync(stream);

		return doc.RootElement.GetProperty("previewUrl").GetString();
	}

	public async Task<MediaFileDto> CreateAsync(string jwt, string userFiltersString, string mediaUrl, string mediaType, CancellationToken cancellationToken = default)
	{
		_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
		var filters = FilterParser.Parse(userFiltersString, FilterParser.CommandType.Add);
		var description = filters.GetValueOrDefault("description") ?? "";
		var tags = (filters.GetValueOrDefault("tags") ?? "")
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.ToList();
		var isPublic = bool.TryParse(filters.GetValueOrDefault("isPublic"), out var p) && p;

		var newMedia = new EditMediaFileDto(mediaType, mediaUrl, isPublic, description, tags);
		var content = JsonContent.Create(new EditMediaRequest { MediaFile = newMedia }, options: _jsonOptions);

		var response = await _http.PostAsync($"{_options.Api.TrimEnd('/')}/v1/media", content, cancellationToken);
		response.EnsureSuccessStatusCode();

		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<AddMediaResponse>(json, _jsonOptions);
		return data?.MediaFile ?? throw new ArgumentNullException("Returned value is null.");
	}
}

internal class GetResponse
{
	/// <summary>
	/// List of media files that match the search criteria.
	/// </summary>
	public required List<MediaFileDto> Items { get; set; }
}

internal record RegisterRequest
{
	/// <summary>
	/// Unique hash identifying the user.
	/// </summary>
	public required string UserHash { get; set; }

	/// <summary>
	/// Unique identifier for the messenger account (e.g., Telegram user ID).
	/// </summary>
	public required string MessengerId { get; set; }

	/// <summary>
	/// Client type, e.g., "tg" for Telegram.
	/// </summary>
	public string Client { get; set; } = "tg";
}

internal record EditMediaRequest
{
	/// <summary>
	/// The media file to be edited.
	/// </summary>
	public required EditMediaFileDto MediaFile { get; init; }
}

public class AddMediaResponse
{
	/// <summary>
	/// The media file that was added to the system.
	/// </summary>
	public required MediaFileDto MediaFile { get; init; }
}


internal class RegisterResponse
{
	/// <summary>
	/// JWT token for the user.
	/// </summary>
	public required string Token { get; set; }
}
