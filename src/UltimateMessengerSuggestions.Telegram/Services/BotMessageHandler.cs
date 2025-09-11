using Telegram.Bot;
using Telegram.Bot.Types;
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

	public async Task<bool> HandleMessageAsync(Message message, CancellationToken cancellationToken)
	{
		if (message.Text!.StartsWith("/register"))
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
		return await SendMessageAsync(message.Chat.Id, "Not supported request", cancellationToken);
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
