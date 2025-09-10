namespace UltimateMessengerSuggestions.Telegram.Common.Options;

sealed class BotOptions
{
	public const string ConfigurationSectionName = "Bot";

	/// <summary>
	/// Token for connection with telegram bot.
	/// </summary>
	public required string Token { get; set; }
}
