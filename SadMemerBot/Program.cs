using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SadBot.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using TenorSharp;

namespace SadMemerBot
{
    public class Program
    {
        private DiscordSocketClient _client;

        // Keep the CommandService and DI container around for use with commands.
        // These two types require you install the Discord.Net.Commands package.
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private CommandHandler _handler;
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

            // Subscribe the logging handler to both the client and the CommandService.
            _client.Log += Log;
            _commands.Log += Log;

            // Setup Environment
            var builder = new ConfigurationBuilder()
              .AddJsonFile($"appsettings.json", true, true)
              .AddUserSecrets<Program>()
              .AddEnvironmentVariables();
            _config = builder.Build();

            var tenorKey = _config.GetRequiredSection("TenorKey").Value;
            // Setup your DI container.
            _services = ConfigureServices(_client, _commands, tenorKey);
            _handler = _services.GetRequiredService<CommandHandler>();
        }

        public static Task Main(string[] args) => new Program().MainAsync();

        // If any services require the client, or the CommandService, or something else you keep on hand,
        // pass them as parameters into this method as needed.
        // If this method is getting pretty long, you can seperate it out into another file using partials.
        private static IServiceProvider ConfigureServices(DiscordSocketClient client, CommandService commandService, string tenorKey)
        {
            var map = new ServiceCollection()
                // Repeat this for all the service classes
                // and other dependencies that your commands might need.
                .AddSingleton(new CommandHandler(client, commandService))
                .AddSingleton(new TenorClient(tenorKey));
            return map.BuildServiceProvider();
        }

        public async Task MainAsync()
        {
            var myToken = _config.GetRequiredSection("DiscordToken").Value;
            // Centralize the logic for commands into a separate method.
            await _handler.InstallCommandsAsync(_services);
            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, myToken);
            await _client.StartAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
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

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
            return Task.CompletedTask;
        }
    }
}
