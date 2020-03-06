using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using MorkoBotRavenEdition.Utilities;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal sealed class NowoResponseProxy : ResponseProxy
    {
        public NowoResponseProxy()
        {
            ProxyType = ResponseProxyType.MessageOnly;
        }

        private static readonly IList<string> ExtraSamples = new List<string>
        {
            "⎝⎠⧹⧸⧹⧸⎝⎠", "(。O ω O。)", "(。O⁄ ⁄ω⁄ ⁄ O。)", "(O ᵕ O)",
            "♥(。ᅌ ω ᅌ。)", "(⁄ʘ⁄ ⁄ ω⁄ ⁄ ʘ⁄)♡", "o ω ͡o", "o ᵕ ͡o",
            "o ꒳ ͡o", "o͡ ꒳ o͡", "°꒳°", "°ᵕ°", "°ω°", " ̷(ⓞ̷ ̷꒳̷ ̷ⓞ̷)",
            "𝕠𝕨𝕠", "𝕆𝕨𝕆"
        };

        internal override async Task Run(DiscordSocketClient client, SocketUserMessage message)
        {
            var normalised = MessageNormaliser.Normalise(message.Content).ToLower();
            //normalised = normalised.Replace(" ", string.Empty); // prevent stuff like "o w o" by removing whitespace
            if (normalised.Contains("owo") || ExtraSamples.Any(message.Content.Contains))
            {
                // delete 
                await message.DeleteAsync();
            }
        }
    }
}
