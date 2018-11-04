using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Modules
{
    /// <summary>
    /// Provides useful help information about several commands.
    /// </summary>
    [Group("help")]
    class HelpModule : MorkoModuleBase
    {
        private IServiceProvider _provider;
        private CommandService _commandService;
        public HelpModule(IServiceProvider provider, CommandService commandService)
        {
            _provider = provider;
            _commandService = commandService;
        }

        private async Task<bool> TestObjectPreconditions(dynamic obj, CommandInfo command = null)
        {
            foreach (PreconditionAttribute attribute in obj.Preconditions)
            {
                if (!(await attribute.CheckPermissionsAsync(Context, command, _provider)).IsSuccess)
                    return false;
            }

            return true;
        }

        [Command, Summary("Gets help about all server commands.")]
        public async Task HelpAsync()
        {
            EmbedBuilder userPm = new EmbedBuilder();
            userPm.WithTitle("Server Help Information");
            userPm.WithDescription("Tip: You can use !help command <command> for detailed information on a single command.");
            userPm.WithColor(Color.Green);

            foreach (ModuleInfo module in _commandService.Modules)
            {
                if (!(await TestObjectPreconditions(module)))
                    continue;

                StringBuilder stringBuilder = new StringBuilder();

                foreach (CommandInfo command in module.Commands)
                {
                    string name = command.Name;
                    if (String.IsNullOrEmpty(name))
                        name = module.Name;

                    if (!(await TestObjectPreconditions(command, command)))
                    {
                        stringBuilder.AppendLine(String.Format("!{0}: No permissions to use this command.", command.Name));
                    }
                    else
                    {
                        string summary = command.Summary;
                        if (String.IsNullOrEmpty(summary))
                            summary = "Command has no summary.";

                        stringBuilder.AppendLine(String.Format("!{0}: {1}", name, summary));
                    }
                }

                if (String.IsNullOrEmpty(stringBuilder.ToString()))
                    continue;

                userPm.AddField(module.Name, stringBuilder.ToString());
            }

            await Context.User.SendMessageAsync(String.Empty, false, userPm.Build());
        }

        [Command("command"), Summary("Gets help about a specific command, usually in more detail.")]
        public async Task CommandAsync([Summary("The command you want to get information about.")] string commandName)
        {
            foreach (CommandInfo command in _commandService.Commands)
            {
                if (commandName.ToLower() != command.Name.ToLower())
                    continue;

                if (!(await TestObjectPreconditions(command, command)))
                    throw new Exception("You don't have permission to view information about this command.");

                EmbedBuilder userPm = new EmbedBuilder();
                userPm.WithTitle("Server Command Information");
                userPm.WithDescription(String.Format("Showing extended help for command {0}.", commandName));

                string summary = command.Summary;
                if (String.IsNullOrEmpty(summary))
                    summary = "Command has no summary.";

                userPm.AddField("Summary", summary);

                StringBuilder usageBuilder = new StringBuilder();
                usageBuilder.Append(String.Format("!{0}", commandName));

                if (command.Parameters.Count > 0)
                {
                    StringBuilder parameterBuilder = new StringBuilder();

                    usageBuilder.Append(": ");
                    foreach (ParameterInfo par in command.Parameters)
                    {
                        string parSummary = par.Summary;
                        if (String.IsNullOrEmpty(parSummary))
                            parSummary = "Parameter has no summary.";

                        usageBuilder.Append(String.Format("<{0}> ", par.Name));
                        parameterBuilder.AppendLine(String.Format("Parameter: {0}. Summary: {1}.", par.Name, parSummary));
                    }

                    userPm.AddField("Parameters", parameterBuilder.ToString());
                }

                userPm.AddField("Usage", usageBuilder.ToString());
                userPm.WithColor(Color.Green);

                await Context.User.SendMessageAsync(String.Empty, false, userPm.Build());
                return;
            }

            throw new Exception("The command was not found.");
        }
    }
}
