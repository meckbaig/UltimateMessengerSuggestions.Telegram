using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using UltimateMessengerSuggestions.Telegram.Common.Options;
using UltimateMessengerSuggestions.Telegram.Models.Dtos;
using UltimateMessengerSuggestions.Telegram.Models.Internal.Enums;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class BotInlineQueryHandler : IBotInlineQueryHandler
{
	private readonly ILogger<BotInlineQueryHandler> _logger;
	private readonly ITelegramBotClient _botClient;
	private readonly IJwtStore _jwtStore;
	private readonly IServiceProvider _serviceProvider;
	private readonly IMediaProcessorService _mediaProcessorService;
	private readonly BotOptions _botOptions;

	private readonly ConcurrentDictionary<long, DateTime> _userLastQueryTime = new();
	private readonly List<MediaQueryState> _userQueries = new();
	private readonly object _userQueriesLock = new();
	private long _operation = 0;

	private const bool LogOperations = false;
	private const bool InvalidateOldResponses = false;

	public BotInlineQueryHandler(
		ILogger<BotInlineQueryHandler> logger,
		ITelegramBotClient botClient,
		IJwtStore jwtStore,
		IServiceProvider serviceProvider,
		IMediaProcessorService mediaProcessorService,
		IOptions<BotOptions> botOptions)
	{
		_logger = logger;
		_botClient = botClient;
		_jwtStore = jwtStore;
		_serviceProvider = serviceProvider;
		_mediaProcessorService = mediaProcessorService;
		_botOptions = botOptions.Value;
	}

	public async Task HandleInlineQueryAsync(InlineQuery query, CancellationToken cancellationToken)
	{
		if (LogOperations)
		{
			long operation = Interlocked.Increment(ref _operation);
			_logger.LogDebug("Operation {Count} started", operation);
			_logger.LogDebug("Operation text: {Text}", query.Query);
			await ProcessInlineQueryAsync(query, cancellationToken);
			_logger.LogDebug("Operation {Count} ended", operation);
		}
		else
		{
			await ProcessInlineQueryAsync(query, cancellationToken);
		}
	}

	private async Task ProcessInlineQueryAsync(InlineQuery query, CancellationToken cancellationToken)
	{
		try
		{
			using var scope = _serviceProvider.CreateScope();
			var api = scope.ServiceProvider.GetRequiredService<IApiService>();
			var userId = query.From.Id;
			var jwt = _jwtStore.GetToken(userId);

			if (jwt == null)
			{
				jwt = await api.LoginAsync(userId.ToString());
				if (jwt == null)
				{
					await _botClient.AnswerInlineQuery(
						query.Id,
						[new InlineQueryResultArticle("noauth", "Not authorized", new InputTextMessageContent("Use /register <hash> to register"))],
						cancellationToken: cancellationToken);
					return;
				}

				_jwtStore.SaveToken(userId, jwt);
			}

			if (string.IsNullOrWhiteSpace(query.Query))
			{
				await _botClient.AnswerInlineQuery(query.Id, null, cacheTime: 0, isPersonal: true, cancellationToken: cancellationToken);
				return;
			}
#pragma warning disable CS4014
			if (InvalidateOldResponses)
				Task.Run(async () => await ReturnLastValidSuggestion(query, api, userId, jwt, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)), cancellationToken);
			else
				Task.Run(async () => await ReturnSuggestion(query, api, userId, jwt, cancellationToken), cancellationToken);
#pragma warning restore CS4014
		}
		catch (OperationCanceledException)
		{
			_logger.LogDebug("Inline query processing cancelled for {UserId}", query.From.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing inline query for {UserId}", query.From.Id);
		}
	}

	private async Task<bool> ReturnLastValidSuggestion(InlineQuery query, IApiService api, long userId, string jwt, CancellationTokenSource cts)
	{
		var mqs = new MediaQueryState(userId, cts);

		lock (_userQueriesLock)
		{
			_userQueries.Add(mqs);
		}

		var suggestions = await api.GetSuggestionsAsync(jwt, query.Query, cts.Token);

		if (cts.IsCancellationRequested)
			return false;

		/// TODO: remove hardcode cringe
		var results = suggestions.Where(x => x.MediaType == "picture").Select(GetInlineQueryResult).ToArray();

		if (_userLastQueryTime.TryGetValue(userId, out var lastRequestTime))
		{
			if (mqs.RequestTime < lastRequestTime)
			{
				_logger.LogDebug("Skipping outdated suggestions for user {UserId}.", userId);
				return false;
			}
		}

		_userLastQueryTime[userId] = mqs.RequestTime;

		lock (_userQueriesLock)
		{
			var outdated = _userQueries
				.Where(x => x.UserId == userId && x.RequestTime < mqs.RequestTime)
				.ToList();

			foreach (var item in outdated)
			{
				item.Cts.Cancel();
				_userQueries.Remove(item);
			}

			_userQueries.Remove(mqs);
		}

		await _botClient.AnswerInlineQuery(query.Id, results, cacheTime: _botOptions.CacheTimeInSeconds, isPersonal: true);

		_logger.LogInformation("User {UserId} received {Count} suggestions.", userId, results.Length);
		return true;
	}

	private async Task<bool> ReturnSuggestion(InlineQuery query, IApiService api, long userId, string jwt, CancellationToken cancellationToken)
	{
		var suggestions = await api.GetSuggestionsAsync(jwt, query.Query, cancellationToken);

		if (cancellationToken.IsCancellationRequested)
			return false;

		/// TODO: remove hardcode cringe
		var results = suggestions.Where(x => x.MediaType == "picture").Select(GetInlineQueryResult).ToArray();

		await _botClient.AnswerInlineQuery(query.Id, results, cacheTime: _botOptions.CacheTimeInSeconds, isPersonal: true, cancellationToken: cancellationToken);

		_logger.LogInformation("User {UserId} received {Count} suggestions.", userId, results.Length);
		return true;
	}

	private InlineQueryResult GetInlineQueryResult(MediaFileDto item)
	{
		return item.MediaType switch
		{
			"picture" => new InlineQueryResultPhoto(
				id: item.Id,
				photoUrl: _mediaProcessorService.ProcessPictureLink(item.MediaUrl, PictureSize.Full),
				thumbnailUrl: _mediaProcessorService.ProcessPictureLink(item.MediaUrl, PictureSize.Preview))
			{
				Title = item.Description,
				Description = string.Join(", ", item.Tags)
			},
			_ => throw new NotImplementedException($"Media type {item.MediaType} is not implemented")
		};
	}

	private struct MediaQueryState
	{
		public long UserId { get; set; }
		public DateTime RequestTime { get; set; }
		public CancellationTokenSource Cts { get; set; }

		public MediaQueryState(long userId, CancellationTokenSource cts)
		{
			UserId = userId;
			RequestTime = DateTime.UtcNow;
			Cts = cts;
		}
	}
}
