using Discord;
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
using System.Configuration;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition
{
    /// <summary>
    /// Main class that dosen't suck no more.
    /// </summary>
    internal class MorkoBot
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;
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
            {
                db.Database.Migrate();
            }

            Console.WriteLine(@"[PREINIT] Database migrations completed.");

            _client = new DiscordSocketClient(dsc);
            _commandService = new CommandService();

            // Add all modules
            await _commandService.AddModuleAsync(typeof(AdminModule));
            await _commandService.AddModuleAsync(typeof(GameModule));
            await _commandService.AddModuleAsync(typeof(GuildModule));
            await _commandService.AddModuleAsync(typeof(HelpModule));
            await _commandService.AddModuleAsync(typeof(MapModule));
            //await _commandService.AddModuleAsync(typeof(RoleplayModule));
            await _commandService.AddModuleAsync(typeof(ShopModule));
            await _commandService.AddModuleAsync(typeof(UserModule));

            // Start the message bus for the wiki
            Console.WriteLine(@"[PREINIT] Connecting to Azure Message Bus.");
            var queueClient = new QueueClient(ConfigurationManager.AppSettings.Get("WikiConnectionString"), "morkobot");
            Console.WriteLine(@"[PREINIT] Connected to Azure successfully and listening for messages.");

            // Register all services in dependency injection container
            IServiceCollection serviceCollection = new ServiceCollection();
            ILoggerFactory factory = new LoggerFactory();
            factory.AddConsole();
            factory.AddDebug();

            var context = new BotDbContext();
            var userService = new UserService(_client, context);
            var infoService = new GuildInfoService(_client, context);
            serviceCollection.AddSingleton(typeof(ILoggerFactory), factory);
            serviceCollection.AddSingleton(typeof(ILogger), factory.CreateLogger("MorkoBot Audit Provider"));
            serviceCollection.AddSingleton(context);
            serviceCollection.AddSingleton(_client);
            serviceCollection.AddSingleton(userService);
            serviceCollection.AddSingleton(infoService);
            serviceCollection.AddSingleton(queueClient);
            serviceCollection.AddSingleton(_commandService);
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton(new ShopService(serviceCollection.BuildServiceProvider(), _client, context));
            serviceCollection.AddSingleton(new WikiService(_client, userService, factory.CreateLogger("MorkoBot MessageBus Provider")));

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = _serviceProvider.GetService<ILogger>();
            _logger.LogInformation(@"Log provider initialized.");

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

            // Finally, register the wiki service event provider
            var wikiService = _serviceProvider.GetService<WikiService>();
            wikiService.Start(queueClient);

            // Accept input from the console
            await PromptConsole();
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

        private async Task PromptConsole() {
            while (true) {
                Console.Write(@"Enter command: ");
                var command = Console.ReadLine();

                if (!string.IsNullOrEmpty(command))
                    await _client.GetGuild(DefaultGuild).GetTextChannel(DefaultChannel).SendMessageAsync(command);
            }
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
            await _client.SetGameAsync(@"with the Nullifactor");
        }

        private Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            _logger.LogTrace($@"Message ID {arg2.Id} from user {arg2.Author.Id} was updated.");
            return Task.CompletedTask;
        }

        private async Task SentMessage(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (arg.Author.IsBot) return;

            _logger.LogTrace($@"Recieved command with message ID {arg.Id} from user {arg.Author.Id}: {arg.Content}");

            var argPos = 0;

            // S P E C I A L stuff
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author == _client.CurrentUser)
            {
                if (message.Content.ToLower().Contains("morko") || message.Content.ToLower().Contains("mörkö"))
                    await message.AddReactionAsync(Emote.Parse("<:morko:329887947736350720>"));

                if (message.Content.ToLower().Contains("raven"))
                    await message.AddReactionAsync(Emote.Parse("<:raven:354971314877890560>"));

                if (message.Content.ToLower().Contains("nullifactor"))
                    await message.AddReactionAsync(Emote.Parse("<:5pm:423182998343516170>"));

                if (message.Content.ToLower().Contains("perkele"))
                    await message.AddReactionAsync(Emote.Parse("<:perkele:374644476800401408>"));

                if (message.Content.ToLower().Contains("oh god") || message.Content.ToLower().Contains("ohgodno"))
                    await message.AddReactionAsync(Emote.Parse("<:ohgodno:374303106961375242>"));

                return;
            }

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

                _logger.LogWarning($@"Message ID {arg.Id} encountered an execution failure: {result.ErrorReason}");
                await MessageUtilities.SendPmSafely(context.User, context.Channel, string.Empty, false, userPm.Build());
            }
            else
            {
                _logger.LogTrace($@"Message ID {arg.Id} completed execution successfully.");
                await message.AddReactionAsync(new Emoji("✅"));
            }

            // Delete asynchronously after 2 seconds.
            await Task.Delay(2000);
            await message.DeleteAsync();
        }

        private Task Log(LogMessage arg)
        {
            if (_logger == null)
            {
                Console.WriteLine(arg.ToString());
            }
            else
            {
                LogLevel level = LogLevel.None;

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
                }

                _logger.Log(level, arg.Message);
            }
            
            return Task.CompletedTask;
        }
    }
}
