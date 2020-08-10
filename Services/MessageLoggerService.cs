using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Models.Guild;
using MorkoBotRavenEdition.Models.Tasks;
using MorkoBotRavenEdition.Utilities;

namespace MorkoBotRavenEdition.Services
{
    internal class MessageLoggerService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BotDbContext _context;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private IDictionary<ulong, BackfillTask> _backfillTasks = new Dictionary<ulong, BackfillTask>();

        public MessageLoggerService(IServiceProvider serviceProvider, BotDbContext context, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _logger = loggerFactory.CreateLogger<MessageLoggerService>();
            _loggerFactory = loggerFactory;

            var infoThread = new Thread(async () => await WatchBackfillAsync());

            infoThread.Start();
        }

        private async Task WatchBackfillAsync() {
            while(true) {
                // wait for 10 seconds
                Thread.Sleep(10000);

                // report state of all
                var toRemove = new List<ulong>();
                foreach (var task in _backfillTasks) {
                    var val = task.Value;
                    if (!val.Event.IsSet) {
                        await val.InvokerChannel.SendStatusAsync("Message Backfill", $@"Processing channel {val.CurrentChannel}", Color.Blue);
                        continue;
                    } else {
                        toRemove.Add(task.Key);
                    }

                    if (!string.IsNullOrWhiteSpace(val.Error)) {
                        await val.InvokerChannel.SendStatusAsync("Message Backfill", $@"Backfill failure!", Color.Red);
                    } else {
                        await val.InvokerChannel.SendStatusAsync("Message Backfill", $@"Backfill completed in {val.TimeTaken}ms, {val.MessagesProcessed} processed, {val.MessagesAdded} added.", Color.Green);
                    }
                }

                // remove completed
                toRemove.ForEach(r => _backfillTasks.Remove(r));
            }
        }

        public async Task LogSend(IUserMessage message)
        {
            var logged = LoggedMessage.FromDiscordMessage(message);
            if (await _context.LoggedMessages.ContainsAsync(logged)) return;

            _context.LoggedMessages.Add(logged);
            await _context.SaveChangesAsync();
        }

        public async Task LogUpdate(IUserMessage message, ulong originalId)
        {
            var logged = LoggedMessage.FromDiscordMessage(message);
            logged.OriginalId = (long) originalId;

            _context.LoggedMessages.Add(logged);
            await _context.SaveChangesAsync();
        }

        public async Task LogDelete(ulong id) {
            // find the original
            var dbm = await _context.LoggedMessages.FirstOrDefaultAsync(m => m.Message == (long) id);
            if (dbm == null) return;

            dbm.Deleted = true;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Exports a log to a paste service and returns the path as a URL
        /// </summary>
        /// <param name="date">The date to filter by</param>
        /// <returns>Link to the paste, otherwise null if failure</returns>
        public async Task<string> LogExportAsync(IDiscordClient client, long guild, long? channel, DateTime? start = null, DateTime? end = null) 
        {
            var task = _serviceProvider.GetRequiredService<LogExportTask>();
            return await task.RunAsync(guild, channel, start, end);
        }

        public void RunBackfill(ICommandContext context) {
            // refuse to run a duplicate job on the same guild
            if (_backfillTasks.ContainsKey(context.Guild.Id) && !_backfillTasks[context.Guild.Id].Event.IsSet)
                throw new Exception("A backfill job is already running for this guild. Please wait.");

            var task = new BackfillTask(_loggerFactory);
            task.Run(context.Channel, context.Guild);

            _backfillTasks[context.Guild.Id] = task;
        }
    }
}
