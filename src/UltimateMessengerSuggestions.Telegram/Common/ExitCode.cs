namespace UltimateMessengerSuggestions.Telegram.Common;

/// <summary>
/// Program exit codes.
/// </summary>
public static class ExitCode
{
	/// <summary>
	/// Normal termination.
	/// </summary>
	public const int Normal = 0;

	/// <summary>
	/// Missing initial configuration data.
	/// </summary>
	public const int NoConfiguration = 1;

	/// <summary>
	/// Fatal error.
	/// </summary>
	public const int Fatal = 255;
}
