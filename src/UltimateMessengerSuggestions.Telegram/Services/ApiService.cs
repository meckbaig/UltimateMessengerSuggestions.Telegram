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
		var filters = FilterParser.ParseGetFilters(userFiltersString);
		string query = $"{_options.Api.TrimEnd('/')}/v1/media?orderBy=id desc&take=5&";
		if (filters.Any())
			query += "&" + string.Join("&", filters.Select(f => "filters=" + Uri.EscapeDataString(f)));

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

	public async Task<bool> UpdateAsync(string jwt, string mediaId, string newDescription, List<string> newTags, CancellationToken cancellationToken = default)
	{
		var mediaToEdit = (await GetAsync(jwt, $"i:{mediaId}", cancellationToken)).Single();
		var editedMedia = new EditMediaFileDto(mediaToEdit.MediaType, mediaToEdit.MediaUrl, mediaToEdit.IsPublic, newDescription, newTags);

		var content = JsonContent.Create(new EditMediaRequest { MediaFile = editedMedia }, options: _jsonOptions);
		var response = await _http.PutAsync($"{_options.Api.TrimEnd('/')}/v1/media/{mediaId}", content, cancellationToken);
		response.EnsureSuccessStatusCode();

		return response.IsSuccessStatusCode;
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

internal class RegisterResponse
{
	/// <summary>
	/// JWT token for the user.
	/// </summary>
	public required string Token { get; set; }
}
