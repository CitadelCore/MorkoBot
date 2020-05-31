﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static MorkoBotRavenEdition.Services.Proxies.ResponseProxy;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal class MessageProxyEvaluator
    {
        private readonly IList<ResponseProxy> _proxies = new List<ResponseProxy>();
        private readonly ILogger _logger;

        public MessageProxyEvaluator(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(MessageProxyEvaluator));
            foreach (var proxy in provider.GetServices<ResponseProxy>()) Register(proxy);
        }

        private void Register(ResponseProxy proxy)
        {
            _logger.LogDebug($"Registered response proxy {proxy}.");
            _proxies.Add(proxy);
        }

        internal async void Evaluate(IDiscordClient client, IUserMessage message, bool isCommand)
        {
            foreach (var proxy in _proxies)
            {
                if (isCommand && proxy.ProxyType != ResponseProxyType.MessageOnly ||
                    !isCommand && proxy.ProxyType != ResponseProxyType.CommandOnly ||
                    proxy.ProxyType == ResponseProxyType.All)
                    await proxy.Run(client, message);
            }
        }
    }
}