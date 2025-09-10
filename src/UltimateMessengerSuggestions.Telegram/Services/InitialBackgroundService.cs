
using Telegram.Bot;
using Telegram.Bot.Polling;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class InitialBackgroundService : BackgroundService
{
	private readonly IBotService _botService;
	private readonly ITelegramBotClient _botClient;
	private readonly ILogger<InitialBackgroundService> _logger;

	public InitialBackgroundService(IBotService botService, ITelegramBotClient botClient, ILogger<InitialBackgroundService> logger)
	{
		_botService = botService;
		_botClient = botClient;
		_logger = logger;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var receiverOptions = new ReceiverOptions
		{
			AllowedUpdates = { }
		};

		_botClient.StartReceiving(
			_botService.HandleUpdateAsync,
			_botService.HandleErrorAsync,
			receiverOptions,
			stoppingToken
		);

		_logger.LogInformation("Bot service started.");
		return Task.CompletedTask;
	}
}
