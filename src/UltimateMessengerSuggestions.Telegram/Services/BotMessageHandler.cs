using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class BotMessageHandler : IBotMessageHandler
{
	private readonly ILogger<BotMessageHandler> _logger;
	private readonly ITelegramBotClient _botClient;
	private readonly IJwtStore _jwtStore;
	private readonly IServiceProvider _serviceProvider;

	public BotMessageHandler(ILogger<BotMessageHandler> logger, ITelegramBotClient botClient, IJwtStore jwtStore, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_botClient = botClient;
		_jwtStore = jwtStore;
		_serviceProvider = serviceProvider;
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

			foreach (var item in result)
			{
				var keyboard = new InlineKeyboardMarkup(new[]
				{
					// TODO: EDIT
					InlineKeyboardButton.WithCallbackData("✏ Редактировать", $"edit:{item.Id}")
				});

				var caption = $"🆔 {item.Id}\n" +
							  $"📄 {item.Description}\n" +
							  $"🏷️ {string.Join(", ", item.Tags)}";

				if (item.MediaType == "picture" && !string.IsNullOrEmpty(item.MediaUrl))
				{
					await _botClient.SendPhoto(
						chatId: message.Chat.Id,
						photo: item.MediaUrl,
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
