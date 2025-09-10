using UltimateMessengerSuggestions.Telegram.Models.Dtos;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

/// <summary>
/// Service for user authentication and registration.
/// </summary>
public interface IApiService
{
	/// <summary>
	/// Register a new user using provided hash and telegram messengerId.
	/// </summary>
	/// <param name="userHash">Registration hash.</param>
	/// <param name="messengerId">Telegram user identifier.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <remarks>Returns JWT if successful.</remarks>
	Task<string?> RegisterAsync(string userHash, string messengerId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Login existing user by messengerId.
	/// </summary>
	/// <param name="messengerId">Telegram user identifier.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <remarks>Returns JWT if successful.</remarks>
	Task<string?> LoginAsync(string messengerId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get media file suggestions based on a search string.
	/// </summary>
	/// <param name="jwt">Authentification token.</param>
	/// <param name="searchString">User input string.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Search results.</returns>
	Task<List<MediaFileDto>> GetSuggestionsAsync(string jwt, string searchString, CancellationToken cancellationToken = default);
}
