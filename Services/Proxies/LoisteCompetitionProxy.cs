using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MorkoBotRavenEdition.Models.Infra;

namespace MorkoBotRavenEdition.Services.Proxies
{
    internal class LoisteCompetitionProxy : ResponseProxy
    {
        private const ulong COMPETITION_CHANNEL_ID = 590270170723778580;
        private readonly BotDbContext _context;

        public LoisteCompetitionProxy(BotDbContext context)
        {
            _context = context;
        }

        internal override async Task Run(IDiscordClient client, IUserMessage message)
        {
            if (message.Channel.Id != COMPETITION_CHANNEL_ID) return;

            // Check for image, if not present, delete it
            if (message.Attachments.Count != 1)
            {
                await message.DeleteAsync();
                return;
            }

            if (!await CanSubmitAsync(message.Author))
            {
                await message.DeleteAsync();
                return;
            }

            // Give it a check mark
            await message.AddReactionAsync(new Emoji("✅"));

            // Add the entry
            await _context.AddAsync(new InfraCompetitionEntry
            {
                Identifier = (long) message.Id,
                UserId = (long) message.Author.Id,
                Entered = DateTime.UtcNow,
                ImageUrl = message.Attachments.Single().ProxyUrl
            });

            await _context.SaveChangesAsync();
        }

        private async Task<bool> CanSubmitAsync(IUser user)
        {
            // TODO: Ensure entries aren't closed

            // Check for existing entries this month
            if (await _context.InfraCompetitionEntries.AnyAsync(
                e => !e.Deleted && e.Identifier == (long) user.Id
            )) return false;

            return true;
        }
    }
}
