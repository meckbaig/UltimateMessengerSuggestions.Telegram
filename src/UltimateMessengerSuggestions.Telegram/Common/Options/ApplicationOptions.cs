namespace UltimateMessengerSuggestions.Telegram.Common.Options;

sealed class ApplicationOptions
{
	public const string ConfigurationSectionName = "Application";

	/// <summary>
	/// Maximum number of retries for database connection attempts.
	/// </summary>
	public int CheckDbRetryCount { get; set; }

	/// <summary>
	/// Maximum wait time for a response from the database during retry attempts (in seconds).
	/// </summary>
	public int CheckDbRetryDelay { get; set; }
}
