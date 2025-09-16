using Telegram.Bot.Types;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

internal interface IBotMessageHandler
{
	Task HandleCallbackQueryAsync(CallbackQuery query, CancellationToken cancellationToken);
	Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
}
