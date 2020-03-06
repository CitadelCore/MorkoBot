using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Discord;

namespace MorkoBotRavenEdition.Services
{
    public class BanTrackerService
    {
        private readonly IDictionary<ulong, Timer> _banTimers = new Dictionary<ulong, Timer>();

        public void StartTrackingBan(IGuild guild, ulong userId, int hours)
        {
            var timer = new Timer {Interval = hours * 60 * 60 * 1000};
            timer.Elapsed += (sender, args) =>
            {
                StopTrackingBan(guild, userId);
            };

            _banTimers.Add(userId, timer);
        }

        public void StopTrackingBan(IGuild guild, ulong userId)
        {
            var timer = _banTimers[userId];
            if (timer == null) return;

            timer.Stop();
            _banTimers.Remove(userId);

            guild.RemoveBanAsync(userId);
        }
    }
}
