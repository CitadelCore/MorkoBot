using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Services.Roleplay;

namespace MorkoBotRavenEdition.Modules.Roleplay
{
    [Group("roleplay"), Alias("rp", "role"), Summary("Roleplay Module")]
    internal partial class RoleplayModule : MorkoModuleBase
    {
        private readonly RoleplayService _roleplayService;
        protected RoleplayModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _roleplayService = serviceProvider.GetService<RoleplayService>();
        }
    }
}
