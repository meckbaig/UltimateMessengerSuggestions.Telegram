namespace UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

sealed class DebugLogOptions
{
	public const string ConfigurationSectionName = "DebugLog";

	/// <summary>
	/// Flag to enable or disable console logging.
	/// </summary>
	public bool Enabled { get; init; }

	/// <summary>
	/// Minimum log level for console output.
	/// </summary>
	public string? MinimumLevel { get; init; }
}
