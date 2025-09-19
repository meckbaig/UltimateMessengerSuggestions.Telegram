using Telegram.Bot.Types;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

internal interface IBotMessageHandler
{
	Task<bool> HandleCallbackQueryAsync(CallbackQuery query, CancellationToken cancellationToken);
	Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
}
