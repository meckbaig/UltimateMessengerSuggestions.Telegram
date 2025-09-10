using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using System.Reflection;
using UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;

namespace UltimateMessengerSuggestions.Telegram.Extensions;

internal static class LoggerConfigurationExtensions
{
	/// <summary>
	/// Creates a bootstrap logger for application initialization.
	/// </summary>
	/// <param name="builder">The application host builder.</param>
	/// <returns>A logger with the required parameters.</returns>
	public static Serilog.Core.Logger CreateBootstrapLogger(this IHostApplicationBuilder builder)
	{
		return new LoggerConfiguration()
			.Enrich.FromLogContext()
			.ReadFrom.Configuration(builder.Configuration)
			.WriteTo.Console()
			.CreateLogger();
	}

	/// <summary>
	/// Creates a full logger with settings from the configuration.
	/// </summary>
	/// <param name="builder">The application host builder.</param>
	/// <returns>A logger with the required parameters.</returns>
	public static Serilog.Core.Logger CreateCompleteLogger(this IHostApplicationBuilder builder)
	{
		return new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Configuration)
			.Enrich.FromLogContext()
			.Filter.ByExcluding(e => e.Exception is OptionsValidationException)
			.WithSerilogSinks(builder.Services.BuildServiceProvider())
			.CreateLogger();
	}

	/// <summary>
	/// Adds Serilog sinks and sets specific logging levels for them.
	/// </summary>
	public static LoggerConfiguration WithSerilogSinks
	(
		this LoggerConfiguration loggerConfiguration,
		IServiceProvider serviceProvider
	)
	{
		ArgumentNullException.ThrowIfNull(loggerConfiguration);
		ArgumentNullException.ThrowIfNull(serviceProvider);

		loggerConfiguration.OverrideSpecificNamespaces(serviceProvider);
		loggerConfiguration.ConfigureSeqFromOptions(serviceProvider);
		loggerConfiguration.ConfigureConsoleFromOptions(serviceProvider);
		loggerConfiguration.ConfigureDebugFromOptions(serviceProvider);
		loggerConfiguration.ConfigureFileLogFromOptions(serviceProvider);

		return loggerConfiguration;
	}

	/// <summary>
	/// Sets specific logging levels for different sources from configuration.
	/// </summary>
	private static void OverrideSpecificNamespaces(this LoggerConfiguration loggerConfiguration, IServiceProvider serviceProvider)
	{
		var loggingSection = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Logging:LogLevel");
		foreach (var (source, level) in loggingSection.GetChildren()
							 .Where(s => !string.Equals(s.Key, "Default", StringComparison.OrdinalIgnoreCase))
							 .ToDictionary(s => s.Key, s => s.Value))
		{
			if (Enum.TryParse<LogLevel>(level, true, out var parsedMsLogLevel))
			{
				var serilogLevel = parsedMsLogLevel.ToSerilogLevel();
				if (serilogLevel == null)
				{
					loggerConfiguration.Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", c => c.Contains(source)));
				}
				else
				{
					loggerConfiguration.MinimumLevel.Override(source, (LogEventLevel)serilogLevel);
				}
			}
		}
		loggerConfiguration.Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", c => c.Contains("Microsoft.Extensions.Options")));
	}

	/// <summary>
	/// Converts <see cref="LogLevel"/> from <see cref="Microsoft"/> to <see cref="LogEventLevel"/> from <see cref="Serilog"/>.
	/// </summary>
	/// <param name="logLevel"><see cref="LogLevel"/> from <see cref="Microsoft"/>.</param>
	/// <returns><see cref="LogEventLevel"/> from <see cref="Serilog"/>.</returns>
	private static LogEventLevel? ToSerilogLevel(this LogLevel logLevel)
	{
		switch (logLevel)
		{
			case LogLevel.Trace:
				return LogEventLevel.Verbose;
			case LogLevel.Debug:
				return LogEventLevel.Debug;
			case LogLevel.Information:
				return LogEventLevel.Information;
			case LogLevel.Warning:
				return LogEventLevel.Warning;
			case LogLevel.Error:
				return LogEventLevel.Error;
			case LogLevel.Critical:
				return LogEventLevel.Fatal;
			case LogLevel.None:
			default:
				return null;
		}
	}

	/// <summary>
	/// Adds a Seq sink.
	/// </summary>
	private static void ConfigureSeqFromOptions(this LoggerConfiguration loggerConfiguration, IServiceProvider serviceProvider)
	{
		var seqOptions = serviceProvider.GetRequiredService<IOptions<SeqOptions>>().Value;
		var serilogLevel = Enum.Parse<LogLevel>(seqOptions.MinimumLevel!, true).ToSerilogLevel();
		if (seqOptions?.Enabled == true && serilogLevel != null)
		{
			var seqConfiguration = loggerConfiguration.WriteTo.Seq(seqOptions.ServerUrl!, apiKey: seqOptions.ApiKey)
				.MinimumLevel.Is((LogEventLevel)serilogLevel);

			if (seqOptions.LevelOverride != null)
			{
				foreach (var overrideLevel in seqOptions.LevelOverride)
				{
					var serilogOverrideLevel = Enum.Parse<LogLevel>(overrideLevel.Value, true).ToSerilogLevel();
					if (serilogOverrideLevel == null)
					{
						seqConfiguration.Filter.ByExcluding(Matching.WithProperty<string>("SourceContext", c => c.Contains(overrideLevel.Key)));
					}
					else
					{
						seqConfiguration.MinimumLevel.Override(overrideLevel.Key, (LogEventLevel)serilogOverrideLevel);
					}
				}
			}
		}
	}

	/// <summary>
	/// Adds a Console sink.
	/// </summary>
	private static void ConfigureConsoleFromOptions(this LoggerConfiguration loggerConfiguration, IServiceProvider serviceProvider)
	{
		var consoleOptions = serviceProvider.GetRequiredService<IOptions<ConsoleLogOptions>>().Value;
		var serilogLevel = Enum.Parse<LogLevel>(consoleOptions.MinimumLevel!, true).ToSerilogLevel();
		if (consoleOptions?.Enabled == true && serilogLevel != null)
		{
			loggerConfiguration.WriteTo.Console()
				.MinimumLevel.Is((LogEventLevel)serilogLevel);
		}
	}

	/// <summary>
	/// Adds a Debug sink.
	/// </summary>
	private static void ConfigureDebugFromOptions(this LoggerConfiguration loggerConfiguration, IServiceProvider serviceProvider)
	{
		var debugOptions = serviceProvider.GetRequiredService<IOptions<DebugLogOptions>>().Value;
		var serilogLevel = Enum.Parse<LogLevel>(debugOptions.MinimumLevel!, true).ToSerilogLevel();
		if (debugOptions?.Enabled == true && serilogLevel != null)
		{
			loggerConfiguration.WriteTo.Debug()
				.MinimumLevel.Is((LogEventLevel)serilogLevel);
		}
	}

	/// <summary>
	/// Adds a FileLog sink.
	/// </summary>
	private static void ConfigureFileLogFromOptions(this LoggerConfiguration loggerConfiguration, IServiceProvider serviceProvider)
	{
		var fileLogOptions = serviceProvider.GetRequiredService<IOptions<FileLogOptions>>().Value;
		var serilogLevel = Enum.Parse<LogLevel>(fileLogOptions.MinimumLevel!, true).ToSerilogLevel();
		if (fileLogOptions?.Enabled == true && serilogLevel != null)
		{
			var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileLogs");
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}

			loggerConfiguration.WriteTo.File
			(
				Path.Combine(logDirectory, "log-.log"),
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: fileLogOptions.RetainedDaysCountLimit,
				rollOnFileSizeLimit: false,
				outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
			)
			.MinimumLevel.Is(Enum.Parse<LogEventLevel>(fileLogOptions.MinimumLevel!, true));
		}
	}
}
