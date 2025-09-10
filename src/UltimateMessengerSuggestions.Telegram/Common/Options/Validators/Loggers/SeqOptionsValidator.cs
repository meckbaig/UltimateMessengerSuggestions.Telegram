using Microsoft.Extensions.Options;
using System.Text;
using UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators.Loggers;

sealed class SeqOptionsValidator : IValidateOptions<SeqOptions>
{
	public ValidateOptionsResult Validate(string? name, SeqOptions options)
	{
		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{SeqOptions.ConfigurationSectionName}' must not be null.");
		}

		if (!options.Enabled)
		{
			return ValidateOptionsResult.Success;
		}

		var failures = new StringBuilder();

		if (string.IsNullOrWhiteSpace(options.ServerUrl))
		{
			failures.AppendLine($"'{SeqOptions.ConfigurationSectionName}:" +
				$"{nameof(SeqOptions.ServerUrl)}' cannot be null or empty.");
		}

		if (string.IsNullOrWhiteSpace(options.MinimumLevel))
		{
			failures.AppendLine($"'{SeqOptions.ConfigurationSectionName}:" +
				$"{nameof(SeqOptions.MinimumLevel)}' cannot be null or empty.");
		}
		else if (!Enum.TryParse<LogLevel>(options.MinimumLevel, true, out _))
		{
			failures.AppendLine($"'{SeqOptions.ConfigurationSectionName}:" +
				$"{nameof(SeqOptions.MinimumLevel)}' is not a valid log level.");
		}

		if (options.LevelOverride != null)
		{
			foreach (var (key, value) in options.LevelOverride)
			{
				if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
				{
					failures.AppendLine($"Key and value '{SeqOptions.ConfigurationSectionName}:" +
						$"{nameof(SeqOptions.LevelOverride)}' cannot be empty.");
				}
				else if (!Enum.TryParse<LogLevel>(value, true, out _))
				{
					failures.AppendLine($"'{SeqOptions.ConfigurationSectionName}:" +
						$"{nameof(SeqOptions.LevelOverride)}:{key}' is not a valid log level.");
				}
			}
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
