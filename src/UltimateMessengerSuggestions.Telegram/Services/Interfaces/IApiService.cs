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

	/// <summary>
	/// Get media files based on user filters string.
	/// </summary>
	/// <param name="jwt">Authentification token.</param>
	/// <param name="userFiltersString">User data filters.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Search results.</returns>
	Task<List<MediaFileDto>> GetAsync(string jwt, string userFiltersString, CancellationToken cancellationToken = default);

	/// <summary>
	/// Update media file description and tags.
	/// </summary>
	/// <param name="jwt">Authentification token.</param>
	/// <param name="mediaId">Id of media to edit.</param>
	/// <param name="newDescription">New description value.</param>
	/// <param name="newTags">New tags collection.</param>
	/// <returns><see langword="true"/> if update was successfull; otherwise <see langword="false"/>.</returns>
	Task<bool> UpdateAsync(string jwt, string mediaId, string newDescription, List<string> newTags, CancellationToken cancellationToken = default);
}
