using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UltimateMessengerSuggestions.Telegram.Models.Internal.Enums;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class BotMessageHandler : IBotMessageHandler
{
	private readonly ILogger<BotMessageHandler> _logger;
	private readonly ITelegramBotClient _botClient;
	private readonly IJwtStore _jwtStore;
	private readonly IServiceProvider _serviceProvider;
	private readonly IMediaProcessorService _mediaProcessorService;

	private readonly ConcurrentDictionary<long, string> _editState = new();

	public BotMessageHandler(ILogger<BotMessageHandler> logger, ITelegramBotClient botClient, IJwtStore jwtStore, IServiceProvider serviceProvider, IMediaProcessorService mediaProcessorService)
	{
		_logger = logger;
		_botClient = botClient;
		_jwtStore = jwtStore;
		_serviceProvider = serviceProvider;
		_mediaProcessorService = mediaProcessorService;
	}

	public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
	{
#pragma warning disable CS4014
		Task.Run(async () =>
		{
			if (message.Text!.StartsWith("/register"))
			{
				return await HandleRegistrationAsync(message, cancellationToken);
			}
			if (message.Text!.StartsWith("/get"))
			{
				return await HandleGetAsync(message, cancellationToken);
			}
			return await SendMessageAsync(message.Chat.Id, "Not supported request", cancellationToken);
		});
#pragma warning restore CS4014
	}

	private async Task<bool> HandleRegistrationAsync(Message message, CancellationToken cancellationToken)
	{
		var parts = message.Text!.Split(' ', 2);
		if (parts.Length == 2)
		{
			using var scope = _serviceProvider.CreateScope();
			var api = scope.ServiceProvider.GetRequiredService<IApiService>();
			var userHash = parts[1];
			var jwt = await api.RegisterAsync(userHash, message.From!.Id.ToString());
			if (jwt != null)
			{
				_logger.LogInformation("User {UserId} registered successfully.", message.From.Id);
				_jwtStore.SaveToken(message.From.Id, jwt);
				return await SendMessageAsync(message.Chat.Id, "Registration successful ✅", cancellationToken);
			}
			else
			{
				_logger.LogInformation("User {UserId} had some issues during registration process.", message.From.Id);
				return await SendMessageAsync(message.Chat.Id, "Registration failed ❌", cancellationToken);
			}
		}
		else
		{
			return await SendMessageAsync(message.Chat.Id, "Usage: /register <hash>", cancellationToken);
		}
	}

	private async Task<bool> HandleGetAsync(Message message, CancellationToken cancellationToken)
	{
		var parts = message.Text!.Split(' ', 2);
		if (parts.Length == 2)
		{
			using var scope = _serviceProvider.CreateScope();
			var api = scope.ServiceProvider.GetRequiredService<IApiService>();
			var userId = message.From!.Id;
			var jwt = _jwtStore.GetToken(userId);

			if (jwt == null)
			{
				jwt = await api.LoginAsync(userId.ToString());
				if (jwt == null)
				{
					return await SendMessageAsync(message.Chat.Id, "Registration required", cancellationToken);
				}
				_jwtStore.SaveToken(userId, jwt);
			}

			var userFiltersString = parts[1];
			var result = await api.GetAsync(jwt, userFiltersString);

			if (result.Count == 0)
			{
				return await SendMessageAsync(message.Chat.Id, "No results found", cancellationToken);
			}

			foreach (var item in result)
			{
				var keyboard = new InlineKeyboardMarkup(new[]
				{
					InlineKeyboardButton.WithCallbackData("✏ Edit", $"edit:{item.Id}")
				});

				var caption = $"🆔 {item.Id}\n" +
							  $"📄 {item.Description}\n" +
							  $"🏷️ {string.Join(", ", item.Tags)}";

				if (item.MediaType == "picture" && !string.IsNullOrEmpty(item.MediaUrl))
				{
					await _botClient.SendPhoto(
						chatId: message.Chat.Id,
						photo: _mediaProcessorService.ProcessPictureLink(item.MediaUrl, PictureSize.Small),
						caption: caption,
						replyMarkup: keyboard,
						cancellationToken: cancellationToken
					);
				}
				else
				{
					await _botClient.SendMessage(
						chatId: message.Chat.Id,
						text: caption,
						replyMarkup: keyboard,
						cancellationToken: cancellationToken
					);
				}
			}
			return true;
		}
		else
		{
			return await SendMessageAsync(message.Chat.Id, "Usage: /get d:\"desc text\" t:tag_name i:a1b2c3\r\n", cancellationToken);
		}
	}

	public async Task HandleCallbackQueryAsync(CallbackQuery query, CancellationToken cancellationToken)
	{
		if (query.Data != null && query.Data.StartsWith("edit:"))
		{
			var mediaId = query.Data.Substring("edit:".Length);

			_editState.TryAdd(query.From.Id, mediaId);

			await _botClient.SendMessage(
				chatId: query.Message.Chat.Id,
				text: "Enter new data using format:\nDescription\ntag1, tag2, tag3",
				cancellationToken: cancellationToken
			);
		}
	}

	private async Task<bool> HandleEditMessageAsync(Message message, CancellationToken cancellationToken)
	{
		_editState.Remove(message.From.Id, out string mediaId);

		var text = message.Text ?? string.Empty;
		var parts = text.Split('\n', 2, StringSplitOptions.TrimEntries);

		if (parts.Length < 2)
		{
			await _botClient.SendMessage(
				chatId: message.Chat.Id,
				text: "❌ Unsupported format. Use: \nDescription\ntag1, tag2, tag3",
				cancellationToken: cancellationToken
			);
			return false;
		}

		var newDescription = parts[0];
		var newTags = parts[1]
			.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(t => t.Trim())
			.Where(t => !string.IsNullOrEmpty(t))
			.ToList();

		using var scope = _serviceProvider.CreateScope();
		var api = scope.ServiceProvider.GetRequiredService<IApiService>();
		var userId = message.From.Id;
		var jwt = _jwtStore.GetToken(userId);

		if (jwt == null)
		{
			jwt = await api.LoginAsync(userId.ToString());
			if (jwt == null)
			{
				await _botClient.SendMessage(message.Chat.Id, "❌ Registration required", cancellationToken: cancellationToken);
				return false;
			}
			_jwtStore.SaveToken(userId, jwt);
		}

		var success = await api.UpdateAsync(jwt, mediaId, newDescription, newTags);

		await _botClient.SendMessage(
			chatId: message.Chat.Id,
			text: success ? "✅ Media updated!" : "❌ Update error",
			cancellationToken: cancellationToken
		);
		return true;
	}
	private async Task<bool> SendMessageAsync(long id, string message, CancellationToken cancellationToken)
	{
		try
		{
			await _botClient.SendMessage(id, message, cancellationToken: cancellationToken);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
