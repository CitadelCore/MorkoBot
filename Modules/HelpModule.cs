using Discord;
using Discord.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Utilities.Exceptions;

namespace MorkoBotRavenEdition.Modules
{
    /// <summary>
    /// Provides useful help information about several commands.
    /// </summary>
    [Summary("Help Module")]
    [Description("Everything you need to know about Morko!")]
    internal class HelpModule : ModuleBase
    {
        private readonly CommandService _commandService;
        public HelpModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _commandService = serviceProvider.GetService<CommandService>();
        }

        private async Task<bool> TestObjectPreconditions(dynamic obj, CommandInfo command = null)
        {
            foreach (PreconditionAttribute attribute in obj.Preconditions)
            {
                if (!(await attribute.CheckPermissionsAsync(Context, command, ServiceProvider)).IsSuccess)
                    return false;
            }

            return true;
        }

        [Command("help"), Summary(@"Gets help about server commands.")]
        public async Task HelpAsync([Summary(@"The command to get help about (optional).")] string command = null)
        {
            if (!string.IsNullOrWhiteSpace(command)) {
                await GetCommandInfoAsync(command);
                return;
            }

            var userPm = new EmbedBuilder();
            userPm.WithTitle(@"Server Help Information");
            userPm.WithDescription(@"Tip: You can use !help <command> for detailed information on a single command.");
            userPm.WithColor(Color.Green);

            foreach (var module in _commandService.Modules)
            {
                if (!(await TestObjectPreconditions(module)))
                    continue;

                var stringBuilder = new StringBuilder();
                foreach (var cmd in module.Commands)
                {
                    if (!(await TestObjectPreconditions(cmd, cmd))) continue;

                    var summary = cmd.Summary;
                    if (string.IsNullOrEmpty(summary))
                        summary = @"Command has no summary.";

                    string cmdStr;
                    if (module.Group != null) {
                        cmdStr = $@"{module.Name} {cmd.Name}";
                    } else {
                        cmdStr = $@"{cmd.Name}";
                    }

                    stringBuilder.AppendLine($@"!{cmdStr}: {summary}");
                }

                if (string.IsNullOrEmpty(stringBuilder.ToString()))
                    continue;

                // Get the description from the attribute or summary
                var description = module.Summary;
                var attr = module.Attributes.FirstOrDefault(a => a.GetType() == typeof(DescriptionAttribute));
                if (attr != null) description = ((DescriptionAttribute) attr).Description;

                userPm.AddField($"{description}", stringBuilder.ToString());
            }

            await Context.Channel.SendMessageAsync(string.Empty, false, userPm.Build());
        }

        private async Task GetCommandInfoAsync(string commandName) {
            foreach (var command in _commandService.Commands)
            {
                if (!string.Equals(commandName, command.Name, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (!(await TestObjectPreconditions(command, command)))
                    throw new Exception(@"You don't have permission to view information about this command.");

                var userPm = new EmbedBuilder();
                userPm.WithTitle(@"Server Command Information");
                userPm.WithDescription($@"Showing extended help for command {commandName}.");

                var summary = command.Summary;
                if (string.IsNullOrEmpty(summary))
                    summary = @"Command has no summary.";

                userPm.AddField(@"Summary", summary);

                var usageBuilder = new StringBuilder();
                usageBuilder.Append($@"!{commandName}");

                if (command.Parameters.Count > 0)
                {
                    var parameterBuilder = new StringBuilder();

                    usageBuilder.Append(": ");
                    foreach (var par in command.Parameters)
                    {
                        var parSummary = par.Summary;
                        if (string.IsNullOrEmpty(parSummary))
                            parSummary = @"Parameter has no summary.";

                        usageBuilder.Append($"<{par.Name}> ");
                        parameterBuilder.AppendLine($@"Parameter: {par.Name}. Summary: {parSummary}.");
                    }

                    userPm.AddField(@"Parameters", parameterBuilder.ToString());
                }

                userPm.AddField(@"Usage", usageBuilder.ToString());
                userPm.WithColor(Color.Green);

                await Context.Channel.SendMessageAsync(string.Empty, false, userPm.Build());
                return;
            }

            throw new ActionException(@"The command was not found.");
        }
    }
}
