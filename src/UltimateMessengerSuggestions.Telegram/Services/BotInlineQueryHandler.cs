using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
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

	//private readonly ConcurrentDictionary<long, CancellationTokenSource> _debounceCts = new();
	private readonly ConcurrentDictionary<long, DateTime> _userLastQueryTime = new();
	private readonly List<MediaQueryState> _userQueries = new();
	private readonly object _userQueriesLock = new();
	private long _operation = 0;

	public BotInlineQueryHandler(
		ILogger<BotInlineQueryHandler> logger,
		ITelegramBotClient botClient,
		IJwtStore jwtStore,
		IServiceProvider serviceProvider,
		IMediaProcessorService mediaProcessorService)
	{
		_logger = logger;
		_botClient = botClient;
		_jwtStore = jwtStore;
		_serviceProvider = serviceProvider;
		_mediaProcessorService = mediaProcessorService;
	}

	public async Task HandleInlineQueryAsync(InlineQuery query, CancellationToken cancellationToken)
	{
		//var userId = query.From.Id;

		//if (_debounceCts.TryRemove(userId, out var old))
		//	old.Cancel();

		//var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		//_debounceCts[userId] = cts;

		//_ = Task.Delay(500, cts.Token)
		//	.ContinueWith(async t =>
		//	{
		//		if (!t.IsCanceled)
		//		{
		//			long operation = Interlocked.Increment(ref _operation);
		//			_logger.LogDebug("Operation {Count} started", operation);
		//			_logger.LogDebug("Operation text: {Text}", query.Query);
		//			await ProcessInlineQueryAsync(query, cancellationToken);
		//			_logger.LogDebug("Operation {Count} ended", operation);
		//		}
		//	}, TaskScheduler.Default);

		long operation = Interlocked.Increment(ref _operation);
		_logger.LogDebug("Operation {Count} started", operation);
		_logger.LogDebug("Operation text: {Text}", query.Query);
		await ProcessInlineQueryAsync(query, cancellationToken);
		_logger.LogDebug("Operation {Count} ended", operation);
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
			Task.Run(async () => await ReturnSuggestion(query, api, userId, jwt, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)), cancellationToken);
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

	private async Task<bool> ReturnSuggestion(InlineQuery query, IApiService api, long userId, string jwt, CancellationTokenSource cts)
	{
		var mqs = new MediaQueryState(userId, cts);

		lock (_userQueriesLock)
		{
			_userQueries.Add(mqs);
		}

		var suggestions = await api.GetSuggestionsAsync(jwt, query.Query, cts.Token);

		if (cts.IsCancellationRequested)
			return false;

		var results = suggestions.Select(GetInlineQueryResult).ToArray();

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

		await _botClient.AnswerInlineQuery(query.Id, results, cacheTime: 10, isPersonal: true);

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
