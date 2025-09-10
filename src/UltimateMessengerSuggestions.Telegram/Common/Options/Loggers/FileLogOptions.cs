namespace UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

sealed class FileLogOptions
{
	public const string ConfigurationSectionName = "FileLog";

	/// <summary>
	/// The number of days/files saved on the disk.
	/// </summary>
	public int RetainedDaysCountLimit { get; set; }

	/// <summary>
	/// Flag to enable or disable console logging.
	/// </summary>
	public bool Enabled { get; init; }

	/// <summary>
	/// Minimum log level for console output.
	/// </summary>
	public string? MinimumLevel { get; init; }
}
