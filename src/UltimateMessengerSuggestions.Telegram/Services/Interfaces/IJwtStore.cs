namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

public interface IJwtStore
{
	void SaveToken(long userId, string jwt);
	string? GetToken(long userId);
}
