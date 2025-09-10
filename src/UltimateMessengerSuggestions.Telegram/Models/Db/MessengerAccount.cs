namespace UltimateMessengerSuggestions.Telegram.Models.Db;

/// <summary>
/// Registered messenger account for a user in the system.
/// </summary>
public class MessengerAccount
{
	/// <summary>
	/// Unique identifier for the messenger account.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Identifier of the user to whom this messenger account belongs.
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	/// Unique identifier for the user in the messenger system.
	/// </summary>
	public required string MessengerId { get; set; }

	/// <summary>
	/// Name of the client from which the user is registered.
	/// </summary>
	public required string Client { get; set; }

	/// <summary>
	/// User associated with this messenger account.
	/// </summary>
	public User User { get; set; } = null!;
}
