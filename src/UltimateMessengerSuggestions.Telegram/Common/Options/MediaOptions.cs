namespace UltimateMessengerSuggestions.Telegram.Common.Options;

sealed class MediaOptions
{
	public const string ConfigurationSectionName = "Media";

	/// <summary>
	/// Media hosts that can support unified resize query parameters.
	/// </summary>
	public required string[] InHouseMediaHosts { get; set; } = [];
}
