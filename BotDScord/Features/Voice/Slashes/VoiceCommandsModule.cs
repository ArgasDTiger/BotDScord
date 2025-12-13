using System.Diagnostics;
using BotDScord.Features.Voice.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BotDScord.Features.Voice.Slashes;

public sealed class VoiceCommandsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<VoiceCommandsModule> _logger;
    private readonly IVoiceClientHandler _voiceClientHandler;

    public VoiceCommandsModule(ILogger<VoiceCommandsModule> logger, IVoiceClientHandler voiceClientHandler)
    {
        _logger = logger;
        _voiceClientHandler = voiceClientHandler;
    }

    [SlashCommand("play", "Plays music from YouTube", Contexts = [InteractionContextType.Guild])]
    public async Task PlayAsync(string youtubeUrl)
    {
        var guild = Context.Guild;
        var client = Context.Client;
        var user = Context.User;

        if (guild is null)
        {
            await RespondAsync(InteractionCallback.Message("This command can only be used on the server!"));
            return;
        }

        if (!guild.VoiceStates.TryGetValue(user.Id, out var voiceState))
        {
            await RespondAsync(InteractionCallback.Message("You are not connected to any voice channel!"));
            return;
        }

        bool isBotInVoice = guild.VoiceStates.TryGetValue(client.Id, out var botVoiceState);

        if (isBotInVoice && botVoiceState!.ChannelId != voiceState.ChannelId)
        {
            await RespondAsync(InteractionCallback.Message("I am already connected to another voice channel!"));
            return;
        }

        await RespondAsync(InteractionCallback.DeferredMessage());

        var voiceClientResult = await _voiceClientHandler.GetVoiceClient(client, guild, user.Id);

        if (voiceClientResult.TryPickT1(out var error, out var voiceClient))
        {
            await RespondAsync(InteractionCallback.Message(error.Message));
            return;
        }

        // TODO: handle exception
        var streamInfo = await GetYoutubeAudioStream(youtubeUrl);

        if (streamInfo is null)
        {
            await FollowupAsync(new InteractionMessageProperties { Content = "Failed to get audio from YouTube!" });
            return;
        }

        await FollowupAsync(new InteractionMessageProperties() { Content = "Playing from YouTube!" });

        var outStream = voiceClient.CreateOutputStream();
        OpusEncodeStream opusStream = new(outStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);

        await StreamYoutubeAudioAsync(streamInfo, opusStream);

        await opusStream.FlushAsync();
    }

    private async Task<IStreamInfo?> GetYoutubeAudioStream(string youtubeUrl)
    {
        try
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(youtubeUrl);
            return streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[YoutubeExplode] Error: {Message}", ex.Message);
            return null;
        }
    }

    private async Task StreamYoutubeAudioAsync(IStreamInfo streamInfo, OpusEncodeStream opusStream)
    {
        ProcessStartInfo startInfo = new("ffmpeg")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var arguments = startInfo.ArgumentList;

        arguments.Add("-reconnect");
        arguments.Add("1");
        arguments.Add("-reconnect_streamed");
        arguments.Add("1");
        arguments.Add("-reconnect_delay_max");
        arguments.Add("5");
        arguments.Add("-i");
        arguments.Add(streamInfo.Url);

        arguments.Add("-loglevel");
        arguments.Add("error");
        arguments.Add("-ac");
        arguments.Add("2");
        arguments.Add("-f");
        arguments.Add("s16le");
        arguments.Add("-ar");
        arguments.Add("48000");
        arguments.Add("pipe:1");

        _logger.LogDebug("[FFmpeg] Starting stream...");
        var ffmpeg = Process.Start(startInfo)!;

        _ = Task.Run(async () =>
        {
            string errors = await ffmpeg.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(errors))
            {
                _logger.LogWarning("[FFmpeg] Error: {Error}", errors);
            }
        });

        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(opusStream);

        _logger.LogDebug("[FFmpeg] Stream has completed.");
    }
}