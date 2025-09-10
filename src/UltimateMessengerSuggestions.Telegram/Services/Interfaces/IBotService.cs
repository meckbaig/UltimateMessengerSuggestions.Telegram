using Telegram.Bot;
using Telegram.Bot.Types;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

public interface IBotService
{
	Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct);
	Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct);
}
