using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using static MorkoBotRavenEdition.Services.Proxies.ResponseProxy;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal class MessageRouter
    {
        private readonly IList<ResponseProxy> _proxies = new List<ResponseProxy>();
        private readonly ILogger _logger;

        public MessageRouter(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger(typeof(MessageRouter));
        }

        internal async void Evaluate(DiscordSocketClient client, SocketUserMessage message, bool isCommand)
        {
            foreach (var proxy in _proxies)
            {
                if (isCommand && proxy.ProxyType != ResponseProxyType.MessageOnly ||
                    !isCommand && proxy.ProxyType != ResponseProxyType.CommandOnly ||
                    proxy.ProxyType == ResponseProxyType.All)
                    await proxy.Run(client, message);
            }
        }

        internal void Register(ResponseProxy proxy)
        {
            _logger.LogDebug($"Registered response proxy {proxy}.");
            _proxies.Add(proxy);
        }

        internal void Register<T>() where T : ResponseProxy, new()
        {
            Register(new T());
        }
    }
}
