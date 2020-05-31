using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

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

        internal abstract Task Run(IDiscordClient client, IUserMessage message);
    }
}
