using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models.Guild;

namespace MorkoBotRavenEdition.Services
{
    internal class MessageLoggerService
    {
        private static BotDbContext _context;

        public MessageLoggerService(BotDbContext context)
        {
            _context = context;
        }

        public async Task LogSend(SocketMessage message)
        {
            var logged = LoggedMessage.FromSocketMessage(message);

            _context.LoggedMessages.Add(logged);
            await _context.SaveChangesAsync();
        }

        public async Task LogUpdate(SocketMessage message, ulong originalId)
        {
            var logged = LoggedMessage.FromSocketMessage(message);
            logged.OriginalId = (long) originalId;

            _context.LoggedMessages.Add(logged);
            await _context.SaveChangesAsync();
        }
    }
}
