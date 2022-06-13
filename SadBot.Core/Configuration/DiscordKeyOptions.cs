using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadBot.Core.Configuration
{
    public class DiscordKeyOptions
    {
        public const string DiscordSettings = "DiscordSettings";
        public string GuildId { get; set; } = String.Empty;
        public string ChannelId { get; set; } = String.Empty;
    }
}
