namespace UltimateMessengerSuggestions.Telegram.Models.Dtos;

/// <summary>
/// Represents the location of a message in a specific platform.
/// </summary>
public record MessageLocationDto
{
	/// <summary>
	/// Unique identifier of the platform where the message is located (e.g., "vk", "tg").
	/// </summary>
	public string Platform { get; init; } = null!;

	/// <summary>
	/// Unique identifier of the dialog (conversation) where the message is located.
	/// </summary>
	public string DialogId { get; init; } = null!;

	/// <summary>
	/// Unique identifier of the message within the dialog.
	/// </summary>
	public string MessageId { get; init; } = null!;

	/// <summary>
	/// Constructor for creating a message location DTO with required fields.
	/// </summary>
	/// <param name="platform">Unique identifier of the platform where the message is located (e.g., "vk", "tg").</param>
	/// <param name="dialogId">Unique identifier of the dialog (conversation) where the message is located.</param>
	/// <param name="messageId">Unique identifier of the message within the dialog.</param>
	public MessageLocationDto(string platform, string dialogId, string messageId)
	{
		Platform = platform;
		DialogId = dialogId;
		MessageId = messageId;
	}
}
