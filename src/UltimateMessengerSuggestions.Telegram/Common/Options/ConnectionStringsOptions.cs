namespace UltimateMessengerSuggestions.Telegram.Common.Options;

sealed class ConnectionStringsOptions
{
	public const string ConfigurationSectionName = "ConnectionStrings";

	/// <summary>
	/// Database connection string for the application.
	/// </summary>
	public required string Default { get; set; }

	/// <summary>
	/// API connection string for the application.
	/// </summary>
	public required string Api { get; set; }
}
