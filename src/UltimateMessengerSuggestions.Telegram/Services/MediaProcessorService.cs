using Microsoft.Extensions.Options;
using UltimateMessengerSuggestions.Telegram.Common.Options;
using UltimateMessengerSuggestions.Telegram.Models.Internal.Enums;
using UltimateMessengerSuggestions.Telegram.Services.Interfaces;

namespace UltimateMessengerSuggestions.Telegram.Services;

internal class MediaProcessorService : IMediaProcessorService
{
	private readonly MediaOptions _mediaOptions;

	public MediaProcessorService(IOptions<MediaOptions> mediaOptions)
	{
		_mediaOptions = mediaOptions.Value;
	}

	public string ProcessPictureLink(string pictureUrl, PictureSize size)
	{
		if (!_mediaOptions.InHouseMediaHosts.Any(host =>
			pictureUrl.StartsWith(host, StringComparison.OrdinalIgnoreCase)))
			return pictureUrl;
		switch (size)
		{
			case PictureSize.Preview:
				return $"{pictureUrl}&x=64&y=64&a=0";
			case PictureSize.Full:
				return $"{pictureUrl}&x=4096&y=4096&a=1";
			default:
				return pictureUrl;
		}
	}

}
