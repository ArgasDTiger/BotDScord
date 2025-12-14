using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NetCord.Gateway.Voice;

namespace BotDScord.Features.Voice.Services;

public sealed class AudioStreamer : IAudioStreamer
{
    private readonly ILogger<AudioStreamer> _logger;

    public AudioStreamer(ILogger<AudioStreamer> logger)
    {
        _logger = logger;
    }
    
    public async Task StreamByUrl(string url, OpusEncodeStream opusStream)
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
        arguments.Add(url);

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

        var errorTask = Task.Run(async () =>
        {
            string errors = await ffmpeg.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(errors))
            {
                _logger.LogWarning("[FFmpeg] Error: {Error}", errors);
            }
        });

        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(opusStream);
        await errorTask;
        _logger.LogDebug("[FFmpeg] Stream has completed.");
    }
}