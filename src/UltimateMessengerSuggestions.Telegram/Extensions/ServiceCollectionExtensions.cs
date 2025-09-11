using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using UltimateMessengerSuggestions.Telegram.Common.Options;
using UltimateMessengerSuggestions.Telegram.Common.Options.Loggers;
using UltimateMessengerSuggestions.Telegram.Common.Options.Validators;
using UltimateMessengerSuggestions.Telegram.Common.Options.Validators.Loggers;
using UltimateMessengerSuggestions.Telegram.DbContexts;

namespace UltimateMessengerSuggestions.Telegram.Extensions;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddAppOptions(this IServiceCollection services)
	{
		services
			.AddOptionsWithValidateOnStart<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
			.BindConfiguration(ConnectionStringsOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<BotOptions>()
			.BindConfiguration(BotOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<MediaOptions>()
			.BindConfiguration(MediaOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<SeqOptions>()
			.BindConfiguration(SeqOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ConsoleLogOptions>()
			.BindConfiguration(ConsoleLogOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<DebugLogOptions>()
			.BindConfiguration(DebugLogOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<FileLogOptions>()
			.BindConfiguration(FileLogOptions.ConfigurationSectionName);

		return services;
	}

	internal static IServiceCollection AddAppOptionsValidators(this IServiceCollection services)
	{
		services.AddSingleton<IValidateOptions<ApplicationOptions>, ApplicationOptionsValidator>();
		services.AddSingleton<IValidateOptions<ConnectionStringsOptions>, ConnectionStringsOptionsValidator>();
		services.AddSingleton<IValidateOptions<BotOptions>, BotOptionsValidator>();
		services.AddSingleton<IValidateOptions<MediaOptions>, MediaOptionsValidator>();
		services.AddSingleton<IValidateOptions<SeqOptions>, SeqOptionsValidator>();
		services.AddSingleton<IValidateOptions<ConsoleLogOptions>, ConsoleLogOptionsValidator>();
		services.AddSingleton<IValidateOptions<DebugLogOptions>, DebugLogOptionsValidator>();
		services.AddSingleton<IValidateOptions<FileLogOptions>, FileLogOptionsValidator>();

		return services;
	}

	internal static IServiceCollection AddBotClient(this IServiceCollection services)
	{
		var options = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<BotOptions>>()
			.Value;
		return services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(options.Token));
	}

	internal static IServiceCollection AddDatabaseConnection(this IServiceCollection services)
	{
		var connectionOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ConnectionStringsOptions>>()
			.Value;
		var applicationOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ApplicationOptions>>()
			.Value;

		return services.AddDbContext<IAppDbContext, AppDbContext>
		(
			options => options.UseNpgsql
			(
				connectionOptions.Default,
				options => options.EnableRetryOnFailure(
					maxRetryCount: applicationOptions.CheckDbRetryCount,
					maxRetryDelay: TimeSpan.FromSeconds(applicationOptions.CheckDbRetryDelay),
					errorCodesToAdd: null
				)
			)
		);
	}
}
