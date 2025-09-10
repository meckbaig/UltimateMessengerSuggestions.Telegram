using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class BotService : IBotService
{
	private readonly ITelegramBotClient _botClient;
	private readonly IJwtStore _jwtStore;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<BotService> _logger;

	public BotService(
		ITelegramBotClient botClient,
		IJwtStore jwtStore,
		IServiceProvider serviceProvider,
		ILogger<BotService> logger)
	{
		_botClient = botClient;
		_jwtStore = jwtStore;
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
	{
		if (update.Type == UpdateType.Message && update.Message!.Text != null)
		{
			_logger.LogDebug("Received message from {UserId}: {MessageText}", update.Message.From?.Id, update.Message.Text);
			await HandleMessage(update.Message, ct);
		}
		else if (update.Type == UpdateType.InlineQuery)
		{
			_logger.LogDebug("Received inline query from {UserId}: {QueryText}", update.InlineQuery?.From.Id, update.InlineQuery?.Query);
			await HandleInlineQuery(update.InlineQuery!, ct);
		}
	}

	private async Task HandleMessage(Message message, CancellationToken ct)
	{
		if (message.Text!.StartsWith("/start"))
		{
			var parts = message.Text.Split(' ', 2);
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
					await _botClient.SendMessage(
						message.Chat.Id,
						"Registration successful ✅",
						cancellationToken: ct);
				}
				else
				{
					_logger.LogInformation("User {UserId} had some issues during registration process.", message.From.Id);
					await _botClient.SendMessage(
						message.Chat.Id,
						"Registration failed ❌",
						cancellationToken: ct);
				}
			}
			else
			{
				await _botClient.SendMessage(
					message.Chat.Id,
					"Usage: /start <hash>",
					cancellationToken: ct);
			}
		}
	}

	private async Task HandleInlineQuery(InlineQuery query, CancellationToken ct)
	{
		using var scope = _serviceProvider.CreateScope();
		var api = scope.ServiceProvider.GetRequiredService<IApiService>();
		var userId = query.From.Id;
		var jwt = _jwtStore.GetToken(userId);

		if (jwt == null)
		{
			jwt = await api.LoginAsync(userId.ToString());
			if (jwt == null)
			{
				_logger.LogInformation("User {UserId} isn't registrated.", userId);
				await _botClient.AnswerInlineQuery(query.Id, new[]
				{
					new InlineQueryResultArticle("noauth", "Not authorized",
					new InputTextMessageContent("Use /start <hash> to register"))
				}, cancellationToken: ct);
				return;
			}

			_jwtStore.SaveToken(userId, jwt);
		}

		var suggestions = await api.GetSuggestionsAsync(jwt, query.Query);

		var results = suggestions.Select((item, i) =>
		{
			return item.MediaType switch
			{
				"picture" => new InlineQueryResultPhoto(
					id: i.ToString(),
					photoUrl: item.MediaUrl!,
					thumbnailUrl: item.MediaUrl!)
				{
					Title = item.Description,
					Description = string.Join(", ", item.Tags)
				},
				_ => throw new NotImplementedException($"Media type {item.MediaType} is not implemented")
			};
		}).ToArray();

		_logger.LogInformation("User {UserId} received {Count} suggestions.", userId, results.Length);
		await _botClient.AnswerInlineQuery(query.Id, results, cancellationToken: ct);
	}

	public Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
	{
		_logger.LogError(ex, "Unhandled exception.");
		return Task.CompletedTask;
	}
}
