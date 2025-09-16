namespace UltimateMessengerSuggestions.Telegram.Models.Dtos;

/// <summary>
/// Information about a media file.
/// </summary>
public record MediaFileDto
{
	/// <summary>
	/// Public identifier of the media file.
	/// </summary>
	public string Id { get; init; } = null!;

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
	/// Location of the message associated with the media file, if applicable.
	/// </summary>
	public MessageLocationDto? MessageLocation { get; init; }

	/// <summary>
	/// List of tags associated with the file.
	/// </summary>
	public List<string> Tags { get; init; } = [];

	/// <summary>
	/// Constructor for creating a media file DTO with required fields and a list of tags.
	/// </summary>
	/// <param name="id">File identifier.</param>
	/// <param name="description">Media file description.</param>
	/// <param name="mediaUrl">Media file URL.</param>
	/// <param name="mediaType">Media file type.</param>
	/// <param name="tags">List of tags associated with the file.</param>
	/// <param name="messageLocation">Location of the message associated with the media file, if applicable.</param>
	/// <param name="isPublic">Indicates whether the media file is free to use.</param>
	public MediaFileDto(string id, string description, string mediaUrl, string mediaType, List<string> tags, MessageLocationDto? messageLocation = null, bool isPublic = false)
	{
		Id = id;
		Description = description;
		MediaUrl = mediaUrl;
		MediaType = mediaType;
		Tags = tags;
		MessageLocation = messageLocation;
		IsPublic = isPublic;
	}
}
