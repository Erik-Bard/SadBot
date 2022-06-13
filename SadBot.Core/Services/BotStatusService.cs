using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SadBot.Core.Services
{
    public class BotStatusService : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger;
        public BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger) 
            : base(client, logger)
        {
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.WaitForReadyAsync(stoppingToken);
            _logger.LogInformation("Client is ready!");

            await _client.SetActivityAsync(new Game("Set my status!"));
        }
    }
}
