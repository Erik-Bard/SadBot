using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SadBot.Core;
using SadBot.Core.Configuration;
using SadBot.Core.Services;
using SadBot.Core.Workers;
using System;
using System.Threading.Tasks;
using TenorSharp;

namespace SadMemerBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private string tenorKey;

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // How much logging do you want to see?
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                //MessageCacheSize = 50,
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                // Again, log level:
                LogLevel = LogSeverity.Info,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
            });

            _client.Log += Log;
            _commands.Log += Log;

            var builder = new ConfigurationBuilder()
              .AddJsonFile($"appsettings.json", true, true)
              .AddUserSecrets<Program>()
              .AddEnvironmentVariables();
            _config = builder.Build();

            tenorKey = _config.GetRequiredSection("TenorKey").Value;
        }

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            var host = Host.CreateDefaultBuilder()
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 200
                };
                config.Token = _config.GetRequiredSection("DiscordToken").Value;
            })
            // Optionally wire up the command service
            .UseCommandService((context, config) =>
            {
                config.DefaultRunMode = RunMode.Async;
                config.CaseSensitiveCommands = false;
            })
            // Optionally wire up the interactions service
            .UseInteractionService((context, config) =>
            {
                config.LogLevel = LogSeverity.Info;
                config.UseCompiledLambda = true;
            })
            .ConfigureServices((context, services) =>
            {
                //Add any other services here
                services.Configure<DiscordKeyOptions>(_config.GetSection(DiscordKeyOptions.DiscordSettings));
                services.AddSingleton(new TenorClient(tenorKey));
                services.AddHostedService<CommandHandler>();
                services.AddHostedService<BotStatusService>();
                services.AddHostedService<WorkerService>();
            }).Build();

            await host.RunAsync();
        }

        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();


            return Task.CompletedTask;
        }
    }
}
