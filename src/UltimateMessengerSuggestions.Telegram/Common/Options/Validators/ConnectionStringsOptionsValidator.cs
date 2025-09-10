using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators;

sealed class ConnectionStringsOptionsValidator : IValidateOptions<ConnectionStringsOptions>
{
	public ValidateOptionsResult Validate(string? name, ConnectionStringsOptions options)
	{
		var failures = new StringBuilder();

		if (string.IsNullOrWhiteSpace(options.Default))
		{
			failures.AppendLine("The connection line to the database should not be empty");
		}
		if (string.IsNullOrWhiteSpace(options.Api))
		{
			failures.AppendLine($"'{ConnectionStringsOptions.ConfigurationSectionName}:" +
				$"{nameof(ConnectionStringsOptions.Api)}' cannot be null or empty.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
