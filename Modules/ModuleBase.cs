using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using MorkoBotRavenEdition.Utilities;

namespace MorkoBotRavenEdition.Modules
{
    internal class ModuleBase : ModuleBase<ICommandContext>
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

        protected async Task SendStatusAsync(string info, Color? color = null, IMessageChannel channel = null) {
            if (channel == null) channel = Context.Channel;
            var title = "Module";

            if (GetType().GetCustomAttributes(typeof(SummaryAttribute), true).FirstOrDefault() is SummaryAttribute summaryAttribute)
                title = summaryAttribute.Text;

            await channel.SendStatusAsync(title, info, color);
        }
    }
}
