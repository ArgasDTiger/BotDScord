using NetCord.Gateway.Voice;

namespace BotDScord.Features.Voice.Models;

public sealed record VoiceClientResult(VoiceClient VoiceClient, bool IsNewClient);