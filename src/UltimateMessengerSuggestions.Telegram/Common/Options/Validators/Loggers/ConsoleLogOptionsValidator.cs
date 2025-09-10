using Microsoft.Extensions.Options;
using System.Text;
using UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

namespace UltimateMessengerSuggestions.Telegram.Common.Options.Validators.Loggers;

sealed class ConsoleLogOptionsValidator : IValidateOptions<ConsoleLogOptions>
{
    public ValidateOptionsResult Validate(string? name, ConsoleLogOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"'{ConsoleLogOptions.ConfigurationSectionName}' must not be null.");
        }

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new StringBuilder();

        if (string.IsNullOrWhiteSpace(options.MinimumLevel))
        {
            failures.AppendLine($"'{ConsoleLogOptions.ConfigurationSectionName}:" +
                $"{nameof(ConsoleLogOptions.MinimumLevel)}' cannot be null or empty.");
        }
        else if (!Enum.TryParse<LogLevel>(options.MinimumLevel, true, out _))
        {
            failures.AppendLine($"'{ConsoleLogOptions.ConfigurationSectionName}:" +
                $"{nameof(ConsoleLogOptions.MinimumLevel)}' is not a valid log level.");
        }

        return failures.Length > 0
            ? ValidateOptionsResult.Fail(failures.ToString())
            : ValidateOptionsResult.Success;
    }
}
