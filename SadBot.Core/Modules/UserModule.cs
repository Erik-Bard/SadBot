using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadBot.Core.Modules
{
    public class UserModule : ModuleBase<SocketCommandContext>
	{
		// userinfo --> username#0282
		// userinfo @username
		// userinfo 96642168176807936
		// whois (name)
		[Command("userinfo")]
		[Summary
		("Returns info about the current user, or the user parameter, if one passed.")]
		[Alias("user", "whois")]
		public async Task UserInfoAsync([Summary("The (optional) user to get info from")]SocketUser user = null)
		{
			var userInfo = user ?? Context.Client.CurrentUser;
			await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
		}
	}
}
