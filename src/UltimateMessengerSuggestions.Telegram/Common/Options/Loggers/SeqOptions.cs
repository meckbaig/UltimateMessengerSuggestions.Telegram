namespace UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

sealed class SeqOptions
{
	public const string ConfigurationSectionName = "Seq";

	/// <summary>
	/// Flag to enable or disable console logging.
	/// </summary>
	public bool Enabled { get; init; }

	/// <summary>
	/// Seq server URL to send logs to.
	/// </summary>
	public string? ServerUrl { get; init; }

	/// <summary>
	/// Seq access key.
	/// </summary>
	public string? ApiKey { get; init; }

	/// <summary>
	/// Minimum log level for console output.
	/// </summary>
	public string? MinimumLevel { get; init; }

	/// <summary>
	/// Override logging levels for specific sources.
	/// </summary>
	public Dictionary<string, string>? LevelOverride { get; init; }
}
