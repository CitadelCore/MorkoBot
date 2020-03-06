using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal abstract class ResponseProxy
    {
        public enum ResponseProxyType
        {
            All,
            MessageOnly,
            CommandOnly
        }

        public ResponseProxyType ProxyType = ResponseProxyType.All;

        internal abstract Task Run(DiscordSocketClient client, SocketUserMessage message);
    }
}
