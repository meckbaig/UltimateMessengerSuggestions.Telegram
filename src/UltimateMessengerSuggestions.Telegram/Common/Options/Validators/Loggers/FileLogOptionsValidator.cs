using Microsoft.Extensions.Options;
using System.Text;
using UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators.Loggers;

sealed class FileLogOptionsValidator : IValidateOptions<FileLogOptions>
{
	public ValidateOptionsResult Validate(string? name, FileLogOptions options)
	{
		if (options == null)
		{
			return ValidateOptionsResult.Fail(
				$"'{FileLogOptions.ConfigurationSectionName}' must not be null.");
		}

		if (!options.Enabled)
		{
			return ValidateOptionsResult.Success;
		}

		var failures = new StringBuilder();

		if (options.RetainedDaysCountLimit <= 0)
		{
			failures.AppendLine($"'{FileLogOptions.ConfigurationSectionName}:" +
				$"{nameof(FileLogOptions.RetainedDaysCountLimit)}' must be a positive number.");
		}
		if (string.IsNullOrWhiteSpace(options.MinimumLevel))
		{
			failures.AppendLine($"'{FileLogOptions.ConfigurationSectionName}:" +
				$"{nameof(FileLogOptions.MinimumLevel)}' cannot be null or empty.");
		}
		else if (!Enum.TryParse<LogLevel>(options.MinimumLevel, true, out _))
		{
			failures.AppendLine($"'{FileLogOptions.ConfigurationSectionName}:" +
				$"{nameof(FileLogOptions.MinimumLevel)}' is not a valid log level.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
