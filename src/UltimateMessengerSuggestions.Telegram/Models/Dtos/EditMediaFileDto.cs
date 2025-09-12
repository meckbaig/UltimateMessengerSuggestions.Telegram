namespace UltimateMessengerSuggestions.Telegram.Models.Dtos;

internal class EditMediaFileDto
{
	/// <summary>
	/// Media file type.
	/// </summary>
	public string MediaType { get; init; } = null!;

	/// <summary>
	/// Media file URL.
	/// </summary>
	public string MediaUrl { get; init; } = null!;

	/// <summary>
	/// Indicates whether the media file is free to use.
	/// </summary>
	public bool IsPublic { get; init; } = false;

	/// <summary>
	/// Description of the media file content.
	/// </summary>
	public string Description { get; init; } = null!;

	/// <summary>
	/// List of tags associated with the file.
	/// </summary>
	public List<string> Tags { get; init; } = [];
}
