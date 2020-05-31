using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Diagnostics;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Models.Guild;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MorkoBotRavenEdition.Models.Tasks
{
    internal class BackfillTask {
        public readonly ManualResetEventSlim Event = new ManualResetEventSlim();
        public IMessageChannel InvokerChannel;

        public string Error { get; private set; }
        public string CurrentChannel { get; private set; }

        public int MessagesProcessed = 0;
        public int MessagesAdded = 0;
        public long TimeTaken = 0;
        
        private ILoggerFactory _loggerFactory;

        public BackfillTask(ILoggerFactory factory) {
            _loggerFactory = factory;
        }

        public void Run(IMessageChannel invoker, IGuild guild) {
            InvokerChannel = invoker;
            
            Task.Factory.StartNew(async () => {
                var channels = await guild.GetTextChannelsAsync();
                var logger = _loggerFactory.CreateLogger($"Backfill Thread {Thread.CurrentThread.ManagedThreadId}");
                logger.LogDebug("Starting backfill");

                var timer = new Stopwatch();
                timer.Start();

                try {
                    using (var context = new BotDbContext()) {
                        foreach (var channel in channels) {
                            logger.LogDebug($"Downloading channel {channel.Name}");

                            CurrentChannel = channel.Name;
                            var messages = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();

                            logger.LogDebug($"Processing channel {channel.Name}");
                            var msgIds = messages.Select(m => (long)m.Id).Distinct().ToArray();
                            var msgsInDb = context.LoggedMessages.Where(m => msgIds.Contains(m.Message)).Select(m => m.Message).ToArray();
                            var msgsNotInDb = messages.Where(m => !msgsInDb.Contains((long)m.Id));

                            var channelProcessed = messages.Count();
                            MessagesProcessed += channelProcessed;
                            logger.LogDebug($"{channelProcessed} messages processed for channel {channel.Name}");

                            foreach (var message in msgsNotInDb) {
                                if (!(message is IUserMessage userMessage)) continue;
                                var logged = LoggedMessage.FromDiscordMessage(userMessage);
                                if (await context.LoggedMessages.ContainsAsync(logged)) continue;

                                MessagesAdded += 1;
                                context.LoggedMessages.Add(logged);
                            }
                        }

                        logger.LogDebug($"Finished message processing; writing {MessagesAdded} new messages to DB");
                        await context.SaveChangesAsync();
                    }
                } catch (Exception e) {
                    Error = e.ToString();
                    logger.LogError(Error);
                }

                timer.Stop();
                TimeTaken = timer.ElapsedMilliseconds;

                logger.LogDebug($"Backfill finished; {TimeTaken}ms elapsed.");
                Event.Set();
            });

            Event.Reset();
        }
    }
}