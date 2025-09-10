namespace UltimateMessengerSuggestions.Telegram.Models.Db;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
	/// <summary>
	/// User identifier.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Hash of the user, used for authentication and identification.
	/// </summary>
	public string UserHash { get; set; } = null!;

	/// <summary>
	/// Name of the user.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Unique identifiers for the user from external client.
	/// </summary>
	public ICollection<MessengerAccount> MessengerAccounts { get; set; } = [];
}
