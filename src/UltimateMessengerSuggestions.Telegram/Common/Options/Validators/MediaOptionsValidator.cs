using Microsoft.Extensions.Options;
using System.Text;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators;

sealed class MediaOptionsValidator : IValidateOptions<MediaOptions>
{
	public ValidateOptionsResult Validate(string? name, MediaOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaOptions.ConfigurationSectionName}' must not be null.");
		}

		for (int i = 0; i < options.InHouseMediaHosts.Length; i++)
		{
			var host = options.InHouseMediaHosts[i];
			if (!Uri.TryCreate(host, UriKind.Absolute, out var uri) ||
				(uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
			{
				failures.AppendLine($"'{MediaOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaOptions.InHouseMediaHosts)}' at index {i} ('{host}') is not a valid HTTP/HTTPS URL.");
			}
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
