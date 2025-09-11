using UltimateMessengerSuggestions.Telegram.Models.Internal.Enums;

namespace UltimateMessengerSuggestions.Telegram.Services.Interfaces;

internal interface IMediaProcessorService
{
	string ProcessPictureLink(string pictureUrl, PictureSize size);
}
