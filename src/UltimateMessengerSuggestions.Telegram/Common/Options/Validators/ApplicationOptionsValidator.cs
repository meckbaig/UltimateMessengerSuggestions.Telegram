using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators;

sealed class ApplicationOptionsValidator : IValidateOptions<ApplicationOptions>
{
	public ValidateOptionsResult Validate(string? name, ApplicationOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{ApplicationOptions.ConfigurationSectionName}' must not be null.");
		}

		if (options.CheckDbRetryCount <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.CheckDbRetryCount)}' must be greater than 0.");
		}
		if (options.CheckDbRetryDelay <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.CheckDbRetryDelay)}' must be greater than 0.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
