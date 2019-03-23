using MorkoBotRavenEdition.Services;

// Disable these; used by EF Core
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MorkoBotRavenEdition.Models.Shop
{
    internal class UserItemDefinition
    {
        public string Name;
        public string Description;
        public string Emoji;
        public ShopService.CurrencyType CurrencyType;
        public int Price;
        public int MinLevel;
        public bool Sellable;
        public ShopService.UseDelegate ItemUsed;
        public ShopService.AddDelegate ItemAdded;
        public ShopService.RemoveDelegate ItemRemoved;
    }
}
