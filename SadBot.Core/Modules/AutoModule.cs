using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadBot.Core.Modules
{
    public class AutoModule : ModuleBase<SocketCommandContext>
    {
		[Command("gif")]
		[Summary("Requires a gif search parameter")]
		public async Task GifAsync([Summary("The gif to search for")]
		string gifToSearchFor)
		{
			// We can also access the channel from the Command Context.
			await Context.Channel.SendMessageAsync($"This was {gifToSearchFor} sent lmao");
		}
	}
}
