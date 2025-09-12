using Telegram.Bot.Types;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

internal interface IBotMessageHandler
{
	Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
}
