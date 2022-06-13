using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SadBot.Core.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SadBot.Core
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _socketClient;

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config)
            : base(client, logger)
        {
            _provider = provider;
            _commandService = commandService;
            _config = config;
            _socketClient = client;
        }
        private async Task InstallCommandsAsync(IServiceProvider services)
        {
            await _commandService.AddModuleAsync<InfoModule>(services);
            await _commandService.AddModuleAsync<UserModule>(services);
            await _commandService.AddModuleAsync<GifModule>(services);
        }
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            if (msg.Author.Id == _socketClient.CurrentUser.Id || msg.Author.IsBot) return;

            int pos = 0;
            if(msg.HasCharPrefix('!', ref pos))
            {
                var context = new SocketCommandContext(_socketClient, msg);
                var result = await _commandService.ExecuteAsync(context, pos, _provider);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync($"You're an idiot, @{msg.Author}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _socketClient.MessageReceived += HandleCommandAsync;
            //_commandService.CommandExecuted += HandleCommandAsync;
            await InstallCommandsAsync(_provider);
        }
    }
}
