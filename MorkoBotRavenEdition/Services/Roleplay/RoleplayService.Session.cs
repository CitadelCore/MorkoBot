using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using MorkoBotRavenEdition.Models.Roleplay;

namespace MorkoBotRavenEdition.Services.Roleplay
{
    internal partial class RoleplayService
    {
        public async Task<RoleplaySession> NewSession(RoleplayMultiverse multiverse)
        {
            var session = new RoleplaySession()
            {
                MultiverseId = multiverse.MultiverseId,
            };

            _dbContext.Add(session);
            await _dbContext.SaveChangesAsync();

            return session;
        }

        public async Task StartSession(RoleplaySession session)
        {
            session.StartTime = DateTime.Now;
            session.Active = true;
            session.Paused = false;

            _dbContext.Update(session);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EndSession(RoleplaySession session)
        {
            session.EndTime = DateTime.Now;
            session.Active = false;
            session.Paused = false;

            _dbContext.Update(session);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a character as a participant to a session.
        /// </summary>
        public async Task<RoleplaySessionParticipant> AddSessionParticipant(int sessionId, int characterId)
        {
            if (_dbContext.RoleplaySessionParticipants.Any(
                p => p.SessionId == sessionId && p.CharacterId == characterId))
                return null; // todo?

            var participant = new RoleplaySessionParticipant()
            {
                CharacterId = characterId,
                SessionId = sessionId,
            };

            _dbContext.Add(participant);
            await _dbContext.SaveChangesAsync();

            return participant;
        }

        /// <summary>
        /// Removes a character from a session.
        /// </summary>
        public async Task RemoveSessionParticipant(RoleplaySessionParticipant participant)
        {
            _dbContext.Remove(participant);
            await _dbContext.SaveChangesAsync();
        }
    }
}
