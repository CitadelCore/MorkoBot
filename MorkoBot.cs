﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MorkoBotRavenEdition.Modules;
using MorkoBotRavenEdition.Services;
using MorkoBotRavenEdition.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using MorkoBotRavenEdition.Services.Proxies;
using ModuleBase = MorkoBotRavenEdition.Modules.ModuleBase;

namespace MorkoBotRavenEdition
{
    /// <summary>
    /// Main class that dosen't suck no more.
    /// </summary>
    internal class MorkoBot
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private MessageRouter _router;
        private MessageLoggerService _messageLogger;
        private IServiceProvider _serviceProvider;

        private ILogger _logger;
        public static ulong DefaultGuild { get; private set; }
        public static ulong DefaultChannel { get; private set; }
        
        private static void Main()
            => new MorkoBot().MorkoAsync().GetAwaiter().GetResult();

        private async Task MorkoAsync() {
            Console.WriteLine(@"[PREINIT] Welcome to MorkoBot Raven Edition, the edition that dosen't suck!");

            DefaultGuild = Convert.ToUInt64(ConfigurationManager.AppSettings.Get("DefaultGuildId"));
            DefaultChannel = Convert.ToUInt64(ConfigurationManager.AppSettings.Get("DefaultChannelId"));

            var dsc = new DiscordSocketConfig
            {
                MessageCacheSize = 50,
            };

            Console.WriteLine(@"[PREINIT] Initializing and migrating database.");
            using (var db = new BotDbContext())
                db.Database.Migrate();

            Console.WriteLine(@"[PREINIT] Database migrations completed.");

            _client = new DiscordSocketClient(dsc);
            _commandService = new CommandService();

            // Register all services in dependency injection container
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_client);
            serviceCollection.AddLogging(options =>
            {
                options.AddConsole();
                options.AddDebug();
            });

            serviceCollection.AddSingleton<BanTrackerService>();
            serviceCollection.AddSingleton<BotDbContext>();
            serviceCollection.AddSingleton<UserService>();
            serviceCollection.AddSingleton<GuildInfoService>();
            serviceCollection.AddSingleton<MessageLoggerService>();
            serviceCollection.AddSingleton<MessageRouter>();
            serviceCollection.AddSingleton(_commandService);
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton<WikiService>();

            // Register response proxies
            serviceCollection.AddSingleton<LoisteCompetitionProxy>();
            serviceCollection.AddSingleton<NowoResponseProxy>();
            serviceCollection.AddSingleton<SpecialResponseProxy>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Core");
            _logger.LogInformation(@"Log provider initialized.");

            // Init the message logger
            _messageLogger = _serviceProvider.GetService<MessageLoggerService>();

            _router = _serviceProvider.GetRequiredService<MessageRouter>();

            // Add all modules
            var modules = new List<Type>
            {
                typeof(AdminModule),
                typeof(GameModule),
                typeof(GuildModule),
                typeof(HelpModule),
                typeof(MapModule),
                //typeof(RoleplayModule),
                typeof(UserModule),
                typeof(BotModule)
            };

            foreach (var module in modules)
                await _commandService.AddModuleAsync(module, _serviceProvider);

            // Set up database
#if DEBUG
            _logger.LogInformation(@"This is a debug build. Bot will run in development mode with developer account token. Local SQLite database will be used.");
            await _client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings.Get("DevToken"));
#else
            _logger.LogInformation(@"This is a production build. Starting bot in production mode with regular token. Remote SQL database will be used.");
            await _client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings.Get("BotToken"));
#endif
            // Start the client so it's ready for use by services
            await _client.StartAsync();

            _client.Log += Log;
            _client.MessageReceived += SentMessage;
            _client.MessageUpdated += Client_MessageUpdated;
            _client.MessageDeleted += Client_MessageDeleted;
            _client.Ready += Client_Ready;
            _client.UserJoined += Client_UserJoined;
            _client.UserLeft += Client_UserLeft;
            _client.GuildMemberUpdated += Client_GuildMemberUpdated;

            // Accept input from the console
            Thread.Sleep(-1);
        }

        private Task Client_GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            return Task.CompletedTask;
        }

        private Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            _logger.LogInformation(arg1.HasValue
                ? $@"Message ID {arg1.Id} was deleted. Original content: {arg1.Value}"
                : $@"Message ID {arg1.Id} was deleted. Could not retrieve the message content because it was not found in the cache.");

            return Task.CompletedTask;
        }

        private Task Client_UserLeft(SocketGuildUser arg)
        {
            _logger.LogTrace($@"User {arg.Id} has left guild {arg.Guild.Id}.");
            return Task.CompletedTask;
        }

        private Task Client_UserJoined(SocketGuildUser arg)
        {
            _logger.LogTrace($@"User {arg.Id} has joined guild {arg.Guild.Id}.");
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetGameAsync(@"with Reality");
        }

        private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            // update logged here
            await _messageLogger.LogUpdate(arg2, arg1.Id);
        }

        private async Task SentMessage(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || arg.Author.IsBot) return;

            try {
                // log the message
                await _messageLogger.LogSend(arg);
            } catch (Exception e) {
                _logger.LogError($"Failed to log message");
                _logger.LogError(e.ToString());
                throw;
            }

            var argPos = 0;
            var isCommand = !(!message.HasCharPrefix('!', ref argPos) || 
                                   message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
                                   message.Author == _client.CurrentUser);

            _router.Evaluate(_client, message, isCommand);
            if (!isCommand) return;

            // into the command handler section now
            _logger.LogTrace($@"Recieved command with message ID {arg.Id} from user {arg.Author.Id}: {arg.Content}");

            var context = new CommandContext(_client, message);
            var typing = arg.Channel.EnterTypingState();

            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
            typing.Dispose();

            // Handle command failure and success.
            if (!result.IsSuccess)
            {
                await message.AddReactionAsync(new Emoji("❎"));

                var userPm = new EmbedBuilder();
                userPm.WithTitle(@"Command failure");
                userPm.WithDescription(result.ErrorReason);
                userPm.WithColor(Color.Orange);

                _logger.LogTrace($@"Message ID {arg.Id} encountered an execution failure: {result.ErrorReason}");
                await arg.Channel.SendMessageAsync(string.Empty, false, userPm.Build());
            }
            else
            {
                _logger.LogTrace($@"Message ID {arg.Id} completed execution successfully.");
                await message.AddReactionAsync(new Emoji("✅"));
            }

            // Delete asynchronously after 2 seconds.
            //await Task.Delay(2000);
            //await message.DeleteAsync();
        }

        private Task Log(LogMessage arg)
        {
            if (_logger == null)
            {
                Console.WriteLine(arg.ToString());
            }
            else
            {
                LogLevel level;

                switch (arg.Severity)
                {
                    case LogSeverity.Critical:
                        level = LogLevel.Critical;
                        break;
                    case LogSeverity.Debug:
                        level = LogLevel.Debug;
                        break;
                    case LogSeverity.Error:
                        level = LogLevel.Error;
                        break;
                    case LogSeverity.Info:
                        level = LogLevel.Information;
                        break;
                    case LogSeverity.Verbose:
                        level = LogLevel.Trace;
                        break;
                    case LogSeverity.Warning:
                        level = LogLevel.Warning;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _logger.Log(level, arg.Message);
            }
            
            return Task.CompletedTask;
        }
    }
}