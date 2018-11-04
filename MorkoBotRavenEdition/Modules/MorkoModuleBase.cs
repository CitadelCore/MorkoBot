using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Modules
{
    class MorkoModuleBase : ModuleBase<CommandContext>
    {
        protected EmbedBuilder GetResponseEmbed(string info, Color? color = null)
        {
            if (color == null)
                color = Color.Green;

            string title = "Module";

            if (GetType().GetCustomAttributes(typeof(SummaryAttribute), true).FirstOrDefault() is SummaryAttribute summaryAttribute)
                title = summaryAttribute.Text;

            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = title,
                Description = info,
                Color = color,
            };

            return builder;
        }
    }
}
