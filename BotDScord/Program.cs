using BotDScord.Extensions;
using BotDScord.Features.Voice.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway(options =>
    {
        options.Intents = GatewayIntents.All;
    })
    .AddGatewayHandlers(typeof(Program).Assembly)
    .AddApplicationCommands();

builder.Services.AddSingleton<IVoiceClientHandler, VoiceClientHandler>();

var host = builder.Build();

host.RegisterEvents();

host.AddSlashCommand("ping", "Ping!", () => "Pong!");

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();