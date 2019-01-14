using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using static MorkoBotRavenEdition.Services.ShopService;

namespace MorkoBotRavenEdition.Models
{
    internal class UserItemDefinition
    {
        public string Name;
        public string Description;
        public string Emoji;
        public CurrencyType CurrencyType;
        public int Price;
        public int MinLevel;
        public bool Sellable;
        public UseDelegate ItemUsed;
        public AddDelegate ItemAdded;
        public RemoveDelegate ItemRemoved;
    }
}
