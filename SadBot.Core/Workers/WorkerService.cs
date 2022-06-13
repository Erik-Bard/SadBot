using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SadBot.Core.Configuration;
using SadBot.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using TenorSharp;

namespace SadBot.Core.Workers
{
    public class WorkerService : BackgroundService, IWorkerService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly TenorClient _tenorClient;
        private readonly ILogger _logger;
        private readonly DiscordKeyOptions _options;
        private const string ChannelName = "sadmemer-memes";
        public WorkerService(DiscordSocketClient client, CommandService commands, TenorClient tenorClient,
            ILogger<DiscordClientService> logger, IOptions<DiscordKeyOptions> options)
        {
            _client = client;
            _commands = commands;
            _tenorClient = tenorClient;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Posting meme at: {DateTime.UtcNow.ToString()}");
                string channelId = _options.ChannelId;
                string guildId = _options.GuildId;
                SocketGuild guildAsCache = await TryGetGuildFromCache(guildId);
                if (guildAsCache == null)
                {
                    await PostUsingRestClient("sad ok meme", guildId, channelId);
                }
                if(guildAsCache != null)
                {
                    await PostUsingCacheClient("sad ok meme", channelId, guildAsCache);
                }
            }
        }

        private Task<SocketGuild> TryGetGuildFromCache(string guildId) 
        {
            try
            {
                SocketGuild guildCache = _client.GetGuild(ulong.Parse(guildId));

                if (guildCache == null)
                    _logger.LogInformation("Cache is empty, grabbing from Rest.");

                return Task.FromResult(guildCache);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error has occured. Please see more: {ex.Message}");
                return null;
            }
        }

        private async Task<RestGuild> RetrieveGuildFromRest(string guildId)
        {
            try
            {
                RestGuild guildRest = await _client.Rest.GetGuildAsync(ulong.Parse(guildId));
                if (guildRest == null)
                    throw new ArgumentException("Guild Id is invalid.");
                return guildRest;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error has occured. Please see more: {ex.Message}");
                return null;
            }
        }
        
        private async Task PostUsingRestClient(string memeSearch, string guildId, string channelId)
        {
            RestGuild guild = await RetrieveGuildFromRest(guildId);
            RestTextChannel channel;
            do
            {
                channel = await guild.GetTextChannelAsync(ulong.Parse(channelId));
                await CreateChannelIfNotExistRest(guild);

                await Task.Delay(TimeSpan.FromMinutes(1));
            } while (channel == null);

            var meme = await _tenorClient.SearchAsync(memeSearch, 50, 0);
            Random rand = new Random();
            int number = rand.Next(0, 50);
            await channel.SendMessageAsync($"{meme.GifResults[number].ItemUrl}");
        }

        private async Task PostUsingCacheClient(string memeSearch, string channelId, SocketGuild guild)
        {
            SocketTextChannel channel = guild.GetTextChannel(ulong.Parse(channelId));

            var meme = await _tenorClient.SearchAsync(memeSearch, 50, 0);
            Random rand = new Random();
            int number = rand.Next(0, 50);
            await channel.SendMessageAsync($"{meme.GifResults[number].ItemUrl}");
        }

        private async Task CreateChannelIfNotExistRest(RestGuild guild)
        {
            var channels = await guild.GetTextChannelsAsync();
            foreach (var item in channels)
            {
                if(item.Name != ChannelName)
                {
                    await guild.CreateTextChannelAsync(ChannelName);
                }
            }
        }
    }
}
