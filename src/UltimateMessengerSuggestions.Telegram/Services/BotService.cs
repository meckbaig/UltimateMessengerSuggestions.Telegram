using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class BotService : IBotService
{
	private readonly ILogger<BotService> _logger;
	private readonly IBotInlineQueryHandler _inlineQueryHandler;
	private readonly IBotMessageHandler _messageHandler;

	public BotService(
		ILogger<BotService> logger,
		IBotInlineQueryHandler inlineQueryHandler,
		IBotMessageHandler botMessageHandler)
	{
		_logger = logger;
		_inlineQueryHandler = inlineQueryHandler;
		_messageHandler = botMessageHandler;
	}

	public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
	{
		if (update.Type == UpdateType.Message && update.Message?.Text != null)
		{
			_logger.LogDebug("Received message from {UserId}: {MessageText}", update.Message.From?.Id, update.Message.Text);
			await _messageHandler.HandleMessageAsync(update.Message, cancellationToken);
		}
		else if (update.Type == UpdateType.InlineQuery)
		{
			_logger.LogDebug("Received inline query from {UserId}: {QueryText}", update.InlineQuery?.From.Id, update.InlineQuery?.Query);
			await _inlineQueryHandler.HandleInlineQueryAsync(update.InlineQuery!, cancellationToken);
		}
		else if (update.Type == UpdateType.CallbackQuery)
		{
			_logger.LogDebug("Received callback query from {UserId}: {Data}", update.CallbackQuery?.From.Id, update.CallbackQuery?.Data);
			await _messageHandler.HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken);
		}
		else
		{
			_logger.LogWarning("Unhandled update type: {UpdateType}", update.Type);
		}
	}

	public Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken cancellationToken)
	{
		_logger.LogError(ex, "Unhandled exception.");
		return Task.CompletedTask;
	}
}
