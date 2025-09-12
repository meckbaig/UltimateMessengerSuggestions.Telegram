using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators;

sealed class BotOptionsValidator : IValidateOptions<BotOptions>
{
	public ValidateOptionsResult Validate(string? name, BotOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{BotOptions.ConfigurationSectionName}' must not be null.");
		}

		if (string.IsNullOrWhiteSpace(options.Token))
		{
			failures.AppendLine($"'{BotOptions.ConfigurationSectionName}:" +
				$"{nameof(BotOptions.Token)}' cannot be null or empty.");
		}
		if (options.CacheTimeInSeconds <= 0)
		{
			failures.AppendLine($"'{BotOptions.ConfigurationSectionName}:" +
				$"{nameof(BotOptions.CacheTimeInSeconds)}' must be greater than 0.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
