namespace UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

sealed class ConsoleLogOptions
{
	public const string ConfigurationSectionName = "ConsoleLog";

	/// <summary>
	/// Flag to enable or disable console logging.
	/// </summary>
	public bool Enabled { get; init; }

	/// <summary>
	/// Minimum log level for console output.
	/// </summary>
	public string? MinimumLevel { get; init; }
}
