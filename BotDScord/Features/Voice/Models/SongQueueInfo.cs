using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace BotDScord.Features.Voice.Models;

public sealed record SongQueueInfo(IStreamInfo StreamInfo, Video Video);