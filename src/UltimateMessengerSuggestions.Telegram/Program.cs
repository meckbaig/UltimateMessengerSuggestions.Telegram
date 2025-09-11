using Microsoft.Extensions.Options;
using Serilog;
using UltimateMessengerSuggestions.Telegram.Common;
using UltimateMessengerSuggestions.Telegram.Extensions;
using UltimateMessengerSuggestions.Telegram.Services;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = builder.CreateBootstrapLogger();

try
{
	builder.Services.AddAppOptionsValidators();
	builder.Services.AddAppOptions();
	Log.Logger = builder.CreateCompleteLogger();
	builder.Logging.ClearProviders().AddSerilog(Log.Logger);
	builder.Services.AddDatabaseConnection();
	builder.Services.AddBotClient();
	builder.Services.AddSingleton<IBotMessageHandler, BotMessageHandler>();
	builder.Services.AddSingleton<IBotInlineQueryHandler, BotInlineQueryHandler>();
	builder.Services.AddSingleton<IMediaProcessorService, MediaProcessorService>();
	builder.Services.AddSingleton<IBotService, BotService>();
	builder.Services.AddSingleton<IJwtStore, JwtStore>();
	builder.Services.AddScoped<IApiService, ApiService>();
	builder.Services.AddHostedService<InitialBackgroundService>();

	var app = builder.Build();
	app.Run();

	return ExitCode.Normal;
}
catch (OptionsValidationException ex)
{
	Log.Error(ex.Message);

	return ExitCode.NoConfiguration;
}
catch (Exception ex)
{
	Log.Fatal(ex, "Fatal termination of application.");

	return ExitCode.Fatal;
}
finally
{
	Log.CloseAndFlush();
}

/// <summary>
/// <see langword="partial"/> initialization of the <see cref="Program"/> class for use in tests.
/// </summary>
public partial class Program { }
