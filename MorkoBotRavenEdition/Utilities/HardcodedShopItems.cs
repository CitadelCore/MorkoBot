using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static MorkoBotRavenEdition.Services.ShopService;

namespace MorkoBotRavenEdition.Utilities
{
    class HardcodedShopItems
    {
        public static readonly IList<UserItemDefinition> Items = new List<UserItemDefinition>
        {
            new UserItemDefinition() { Name = "Teddy", Description = "Huggable. Fluffy. What more could you want from a vicious bear?", CurrencyType = CurrencyType.SewerCoin, Price = 5, MinLevel = 1, Emoji = "<:tbear:329886534360760321>" },
            new UserItemDefinition() { Name = "Green Shroom", Description = "Green, glowing, and goes well in :tea:.", CurrencyType = CurrencyType.SewerCoin, Price = 15, MinLevel = 2, Emoji = "<:shroom:328885112777474049>" },
            new UserItemDefinition() { Name = "Blue Shroom", Description = "It glows with a blue healing light...", CurrencyType = CurrencyType.SewerCoin, Price = 50, MinLevel = 5, Emoji = "<:blue_shroom:357917234049581057>" },
            new UserItemDefinition() { Name = "Red Shroom", Description = "Oh god no.", CurrencyType = CurrencyType.Health, Price = 5, MinLevel = 8, Emoji = "<:red_shroom:357917288499904512>" },
            new UserItemDefinition() { Name = "Jar of Polonium-210", Description = "As authorized by Putin himself.", CurrencyType = CurrencyType.SewerCoin, Price = 500, MinLevel = 8, Emoji = "<:polonium:342665205987409930>" },
            new UserItemDefinition() { Name = "Morko's Mask", Description = "Lovingly crafted by the Underground God.", CurrencyType = CurrencyType.SewerCoin, Price = 500, MinLevel = 10, Emoji = "<:morko:462354506919837706>" },
            new UserItemDefinition() { Name = "Sigil of Zuntti", Description = "Maybe.", CurrencyType = CurrencyType.SewerCoin, Price = 50000, MinLevel = 15, Emoji = "<:zuntti:460438012715597835>" },

            new UserItemDefinition() { Name = "T1 Increment Boost", Description = "Allows you to increment twice every hour for 12 hours.", CurrencyType = CurrencyType.SewerCoin, Price = 50, MinLevel = 4, Emoji = ":arrow_forward:"},
            new UserItemDefinition() { Name = "T2 Increment Boost", Description = "Allows you to increment four times every hour for 12 hours.", CurrencyType = CurrencyType.SewerCoin, Price = 200, MinLevel = 6, Emoji = ":fast_forward:"},
            new UserItemDefinition() { Name = "T3 Increment Boost", Description = "Allows you to increment eight times every hour for 12 hours.", CurrencyType = CurrencyType.SewerCoin, Price = 800, MinLevel = 10, Emoji = ":twisted_rightwards_arrows:"},
            new UserItemDefinition() { Name = "T1 Auto-Incrementer", Description = "Increments automatically for you for 8 hours.", CurrencyType = CurrencyType.SewerCoin, Price = 25, MinLevel = 2, Emoji = ":arrow_up:"},
            new UserItemDefinition() { Name = "T2 Auto-Incrementer", Description = "Increments automatically for you for 24 hours.", CurrencyType = CurrencyType.SewerCoin, Price = 50, MinLevel = 4, Emoji = ":arrow_double_up:"},
            new UserItemDefinition() { Name = "T1 Currency Matrix", Description = "Allows lossy conversion of OC into increments.", CurrencyType = CurrencyType.SewerCoin, Price = 250, MinLevel = 12, Emoji = ":asterisk:"},
        };
    }
}
