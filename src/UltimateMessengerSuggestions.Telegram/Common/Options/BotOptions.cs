namespace UltimateMessengerSuggestions.Telegram.Common.Options;

sealed class BotOptions
{
	public const string ConfigurationSectionName = "Bot";

	/// <summary>
	/// Token for connection with Telegram bot.
	/// </summary>
	public required string Token { get; set; }

	/// <summary>
	/// Time in seconds for caching inline query results on Telegram side.
	/// </summary>
	public required int CacheTimeInSeconds { get; set; }
}
