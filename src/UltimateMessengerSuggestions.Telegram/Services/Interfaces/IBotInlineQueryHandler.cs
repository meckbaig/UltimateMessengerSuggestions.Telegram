using Telegram.Bot.Types;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

internal interface IBotInlineQueryHandler
{
	Task HandleInlineQueryAsync(InlineQuery query, CancellationToken cancellationToken);
}
