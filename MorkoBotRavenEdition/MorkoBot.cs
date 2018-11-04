using Discord;
using Discord.Commands;
using Discord.Rest;
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
using System.Reflection;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition
{
    /// <summary>
    /// Main class that dosen't suck no more.
    /// </summary>
    class MorkoBot
    {
        private DiscordSocketClient Client;
        private CommandService commandService;
        private IServiceProvider serviceProvider;

        private ILogger _logger;
        public static ulong DefaultGuild { get; private set; }
        public static ulong DefaultChannel { get; private set; }

        static void Main(string[] args)
            => new MorkoBot().MorkoAsync().GetAwaiter().GetResult();

        public async Task MorkoAsync() {
            Console.WriteLine("[PREINIT] Welcome to MorkoBot Raven Edition, the edition that dosen't suck!");

            DefaultGuild = Convert.ToUInt64(ConfigurationManager.AppSettings.Get("DefaultGuildId"));
            DefaultChannel = Convert.ToUInt64(ConfigurationManager.AppSettings.Get("DefaultChannelId"));

            DiscordSocketConfig dsc = new DiscordSocketConfig
            {
                MessageCacheSize = 50,
            };

            Console.WriteLine("[PREINIT] Initializing and migrating database.");
            using (BotDbContext db = new BotDbContext())
            {
                db.Database.Migrate();
            }

            Console.WriteLine("[PREINIT] Database migrations completed.");

            Client = new DiscordSocketClient(dsc);
            commandService = new CommandService();

            await commandService.AddModuleAsync(typeof(MapModule));
            await commandService.AddModuleAsync(typeof(AdminModule));
            await commandService.AddModuleAsync(typeof(GameModule));
            await commandService.AddModuleAsync(typeof(UserModule));
            await commandService.AddModuleAsync(typeof(GuildModule));
            await commandService.AddModuleAsync(typeof(HelpModule));
            await commandService.AddModuleAsync(typeof(ShopModule));

            Console.WriteLine("[PREINIT] Connecting to Azure Message Bus.");
            QueueClient queueClient = new QueueClient(ConfigurationManager.AppSettings.Get("WikiConnectionString"), "morkobot");
            Console.WriteLine("[PREINIT] Connected to Azure successfully and listening for messages.");

            IServiceCollection serviceCollection = new ServiceCollection();
            ILoggerFactory factory = new LoggerFactory();
            factory.AddConsole();
            factory.AddDebug();

            BotDbContext _context = new BotDbContext();
            UserService userService = new UserService(Client, _context);
            GuildInfoService infoService = new GuildInfoService(Client, _context);
            serviceCollection.AddSingleton(typeof(ILoggerFactory), factory);
            serviceCollection.AddSingleton(typeof(ILogger), factory.CreateLogger("MorkoBot Audit Provider"));
            serviceCollection.AddSingleton(_context);
            serviceCollection.AddSingleton(Client);
            serviceCollection.AddSingleton(userService);
            serviceCollection.AddSingleton(infoService);
            serviceCollection.AddSingleton(queueClient);
            serviceCollection.AddSingleton(commandService);
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton(new ShopService(serviceCollection.BuildServiceProvider(), Client, _context));
            serviceCollection.AddSingleton(new WikiService(Client, queueClient, userService, factory.CreateLogger("MorkoBot MessageBus Provider")));

            serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = serviceProvider.GetService<ILogger>();
            _logger.LogInformation("Log provider initialized.");

#if DEBUG
            _logger.LogInformation("This is a debug build. Bot will run in development mode with developer account token. Local SQLite database will be used.");
            await Client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings.Get("DevToken"));
#else
            _logger.LogInformation("This is a production build. Starting bot in production mode with regular token. Remote SQL database will be used.");
            await Client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings.Get("BotToken"));
#endif

            await Client.StartAsync();

            Client.Log += Log;
            Client.MessageReceived += SentMessage;
            Client.MessageUpdated += Client_MessageUpdated;
            Client.MessageDeleted += Client_MessageDeleted;
            Client.Ready += Client_Ready;
            Client.UserJoined += Client_UserJoined;
            Client.UserLeft += Client_UserLeft;
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;

            await PromptConsole();
        }

        private Task Client_GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            return Task.CompletedTask;
        }

        private Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if (arg1.HasValue)
            {
                _logger.LogInformation(String.Format("Message ID {0} was deleted. Original content: {1}", arg1.Id, arg1.Value));
            }
            else
            {
                _logger.LogInformation(String.Format("Message ID {0} was deleted. Could not retrieve the message content because it was not found in the cache.", arg1.Id));
            }

            
            return Task.CompletedTask;
        }

        private async Task PromptConsole() {
            while (true) {
                Console.Write("Enter command: ");
                string command = Console.ReadLine();

                if (!String.IsNullOrEmpty(command))
                    await Client.GetGuild(DefaultGuild).GetTextChannel(DefaultChannel).SendMessageAsync(command);
            }
        }

        private Task Client_UserLeft(SocketGuildUser arg)
        {
            _logger.LogTrace(String.Format("User {0} has left guild {1}.", arg.Id, arg.Guild.Id));
            return Task.CompletedTask;
        }

        private Task Client_UserJoined(SocketGuildUser arg)
        {
            _logger.LogTrace(String.Format("User {0} has joined guild {1}.", arg.Id, arg.Guild.Id));
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            await Client.SetStatusAsync(UserStatus.DoNotDisturb);
            await Client.SetGameAsync("with the Nullifactor");
        }

        private Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            _logger.LogTrace(String.Format("Message ID {0} from user {1} was updated.", arg2.Id, arg2.Author.Id));
            return Task.CompletedTask;
        }

        private async Task SentMessage(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;
            if (message == null) return;

            _logger.LogTrace(String.Format("Recieved command with message ID {0} from user {1}: {2}", arg.Id, arg.Author.Id, arg.Content));

            int argPos = 0;

            // S P E C I A L stuff
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos)) || message.Author == Client.CurrentUser)
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

            CommandContext context = new CommandContext(Client, message);

            IDisposable typing = arg.Channel.EnterTypingState();

            IResult result = await commandService.ExecuteAsync(context, argPos, serviceProvider);
            typing.Dispose();

            // Handle command failure and success.
            if (!result.IsSuccess)
            {
                await message.AddReactionAsync(new Emoji("❎"));

                EmbedBuilder userPm = new EmbedBuilder();
                userPm.WithTitle("Command failure");
                userPm.WithDescription(result.ErrorReason);
                userPm.WithColor(Color.Orange);

                _logger.LogWarning(String.Format("Message ID {0} encountered an execution failure: {1}", arg.Id, result.ErrorReason));
                await MessageUtilities.SendPMSafely(context.User, context.Channel, String.Empty, false, userPm.Build());
            }
            else
            {
                _logger.LogTrace(String.Format("Message ID {0} completed execution successfully.", arg.Id));
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
