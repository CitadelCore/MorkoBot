using Discord;
using Discord.Commands;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MorkoBotRavenEdition.Modules
{
    /// <summary>
    /// Provides useful help information about several commands.
    /// </summary>
    [Group("help")]
    internal class HelpModule : MorkoModuleBase
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

        [Command, Summary(@"Gets help about all server commands.")]
        public async Task HelpAsync()
        {
            var userPm = new EmbedBuilder();
            userPm.WithTitle(@"Server Help Information");
            userPm.WithDescription(@"Tip: You can use !help command <command> for detailed information on a single command.");
            userPm.WithColor(Color.Green);

            foreach (var module in _commandService.Modules)
            {
                if (!(await TestObjectPreconditions(module)))
                    continue;

                var stringBuilder = new StringBuilder();

                foreach (var command in module.Commands)
                {
                    if (!(await TestObjectPreconditions(command, command)))
                    {
                        stringBuilder.AppendLine($@"!{command.Name}: No permissions to use this command.");
                    }
                    else
                    {
                        var summary = command.Summary;
                        if (string.IsNullOrEmpty(summary))
                            summary = @"Command has no summary.";

                        stringBuilder.AppendLine($@"!{module.Name} {command.Name}: {summary}");
                    }
                }

                if (string.IsNullOrEmpty(stringBuilder.ToString()))
                    continue;


                userPm.AddField(module.Name, stringBuilder.ToString());
            }

            await Context.User.SendMessageAsync(string.Empty, false, userPm.Build());
        }

        [Command("command"), Summary(@"Gets help about a specific command, usually in more detail.")]
        public async Task CommandAsync([Summary(@"The command you want to get information about.")] string commandName)
        {
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

                await Context.User.SendMessageAsync(string.Empty, false, userPm.Build());
                return;
            }

            throw new Exception(@"The command was not found.");
        }
    }
}
