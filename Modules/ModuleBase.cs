using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace MorkoBotRavenEdition.Modules
{
    internal class ModuleBase : ModuleBase<CommandContext>
    {
        protected readonly IServiceProvider ServiceProvider;

        protected ModuleBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected EmbedBuilder GetResponseEmbed(string info, Color? color = null)
        {
            if (color == null)
                color = Color.Green;

            var title = "Module";

            if (GetType().GetCustomAttributes(typeof(SummaryAttribute), true).FirstOrDefault() is SummaryAttribute summaryAttribute)
                title = summaryAttribute.Text;

            var builder = new EmbedBuilder
            {
                Title = title,
                Description = info,
                Color = color,
            };

            return builder;
        }
    }
}
