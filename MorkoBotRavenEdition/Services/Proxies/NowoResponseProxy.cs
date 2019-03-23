using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal sealed class NowoResponseProxy : ResponseProxy
    {
        public NowoResponseProxy()
        {
            ProxyType = ResponseProxyType.MessageOnly;
        }

        static NowoResponseProxy()
        {
            //Samples = BuildSampleList();
        }

        private static readonly Dictionary<char, IList<char>> Normaliser = new Dictionary<char, IList<char>>
        {
            { 'o', new List<char>
            {
                'u', 'e', '-',
                '⊙', '●', '♼', 'ø',
                '☆', '✧', '♥', '◕',
                'ᅌ', '◔', 'ʘ', '⓪',
            }},

            { 'w', new List<char>
            {
                'ω', '꒳'
            }},
        };

        private static readonly IList<string> ExtraSamples = new List<string>
        {
            "⎝⎠⧹⧸⧹⧸⎝⎠", "(。O ω O。)", "(。O⁄ ⁄ω⁄ ⁄ O。)", "(O ᵕ O)",
            "♥(。ᅌ ω ᅌ。)", "(⁄ʘ⁄ ⁄ ω⁄ ⁄ ʘ⁄)♡", "o ω ͡o", "o ᵕ ͡o",
            "o ꒳ ͡o", "o͡ ꒳ o͡", "°꒳°", "°ᵕ°", "°ω°", " ̷(ⓞ̷ ̷꒳̷ ̷ⓞ̷)"
        };

        private static readonly IList<string> Samples;

        internal override async Task Run(DiscordSocketClient client, SocketUserMessage message)
        {
            var content = message.Content;
        }
    }
}
