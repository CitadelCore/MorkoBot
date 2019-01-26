using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using log4net.Core;

namespace MorkoBotRavenEdition.Services.Roleplay
{
#if ROLEPLAY_ENABLED
    internal partial class RoleplayService
    {
        private readonly DiscordSocketClient _socketClient;
        private readonly UserService _userService;
        private readonly BotDbContext _dbContext;

        public RoleplayService(DiscordSocketClient socketClient, UserService userService, BotDbContext dbContext)
        {
            _socketClient = socketClient;
            _userService = userService;
            _dbContext = dbContext;
        }
    }
#endif
}
