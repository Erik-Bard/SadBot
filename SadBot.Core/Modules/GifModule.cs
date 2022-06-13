using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using TenorSharp;

namespace SadBot.Core.Modules
{
    public class GifModule : ModuleBase<SocketCommandContext>
    {
		private readonly TenorClient _tenorClient;
		public GifModule(TenorClient tenorClient)
        {
			_tenorClient = tenorClient;
        }

		[Command("gif")]
		[Summary("Requires a gif search parameter")]
		public async Task GifAsync([Summary("The gif to search for")]
		string gifToSearchFor)
		{
            var meme = await _tenorClient.SearchAsync(gifToSearchFor, 50, 0);
			Random rand = new Random();
			int number = rand.Next(0, 50);
			await Context.Channel.SendMessageAsync($"{meme.GifResults[number].ItemUrl}");
		}

		[Command("userinfo")]
		[Summary
		("Returns info about the current user, or the user parameter, if one passed.")]
		[Alias("user", "whois")]
		public async Task UserInfoAsync(
		[Summary("The (optional) user to get info from")]
		SocketUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}
	}
}
