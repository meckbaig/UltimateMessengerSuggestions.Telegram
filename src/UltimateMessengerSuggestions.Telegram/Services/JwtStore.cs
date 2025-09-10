using System.Collections.Concurrent;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class JwtStore : IJwtStore
{
	private readonly ConcurrentDictionary<long, string> _tokens = new();

	public void SaveToken(long userId, string jwt) => _tokens[userId] = jwt;
	public string? GetToken(long userId) => _tokens.TryGetValue(userId, out var jwt) ? jwt : null;
}
