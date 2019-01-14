using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static MorkoBotRavenEdition.Services.ShopService;

namespace MorkoBotRavenEdition.Modules
{
    [Group("shop")]
    internal class ShopModule : MorkoModuleBase
    {
        private readonly UserService _userService;
        private readonly ShopService _shopService;
        

        public ShopModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userService = serviceProvider.GetService<UserService>();
            _shopService = serviceProvider.GetService<ShopService>();
        }

        [Command("items"), Summary(@"Displays a list of items you can purchase from the shop.")]
        public async Task GetItemsAsync()
        {
            var items = _shopService.GetItems();
            await Context.User.SendMessageAsync(@"Welcome to the shop! Here's a list of the items you can purchase.");

            foreach (var item in items)
            {
                var emojiPrefix = string.Empty;
                if (!string.IsNullOrWhiteSpace(item.Emoji))
                    emojiPrefix = item.Emoji + " ";

                string currency;
                switch (item.CurrencyType)
                {
                    case CurrencyType.Experience:
                        currency = "<:olut:329889326051753986> XP";
                        break;
                    case CurrencyType.Health:
                        currency = "<:eeg:359363156885110794> HP";
                        break;
                    case CurrencyType.SewerCoin:
                        currency = "<:sewercoin:354606163112755230> OC";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await Context.User.SendMessageAsync($"{emojiPrefix}{item.Name}: **{item.Price}**{currency}, <:geocache:357917503894061059>**{item.MinLevel}** \"*{item.Description}*\"");
            }
        }

        [Command("buy"), Summary(@"Purchases an item for yourself.")]
        public async Task BuyItemAsync([Summary(@"The item to purchase.")] string itemName)
        {
            var profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);
            var item = _shopService.DefinitionFromItem(itemName);
            await _shopService.BuyItem(profile, item);

            var emojiPrefix = string.Empty;
            if (!string.IsNullOrWhiteSpace(item.Emoji))
                emojiPrefix = item.Emoji + " ";
            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed($"Successfully bought the item \"{emojiPrefix}{item.Name}\"!", Color.Green).Build());
        }

        [Command("add"), Summary(@"Adds an item to a user's inventory.")]
        public async Task AddItemAsync([Summary(@"The item to add.")] string itemName, [Summary(@"The amount of items to add (optional).")] int amount = 1, [Summary(@"The user to add the item to (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            await UserService.ThrowIfHasNoPermissions(Context, ServiceProvider, "Discord Admin");
            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            var item = _shopService.DefinitionFromItem(itemName);
            await _shopService.AddItem(profile, item, amount);

            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed($"Successfully added {amount} of the item \"{item.Name}\" to the user {user.Username}.", Color.Green).Build());
        }

        [Command("trash"), Summary(@"Removes an item from a user's inventory.")]
        public async Task TrashItemAsync([Summary(@"The item to remove.")] string itemName, [Summary(@"The amount of items to remove (optional).")] int amount = 1, [Summary(@"The user to remove the item from (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, ServiceProvider, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);
            }

            var profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            var item = _shopService.DefinitionFromItem(itemName);
            await _shopService.RemoveItem(profile, item, amount);
            
            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed($"Successfully removed {amount} of the item \"{item.Name}\" from the user {user.Username}.", Color.Green).Build());
        }

        [Command("use"), Summary("Uses an item, if the item supports it.")]
        public async Task UseItemAsync([Summary("The item to use.")] string itemName)
        {
            var profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);
            var item = _shopService.DefinitionFromItem(itemName);
            await _shopService.UseItem(profile, item);

            await Context.User.SendMessageAsync(string.Empty, false, GetResponseEmbed("Successfully removed used the item \"{item.Name}\"!", Color.Green).Build());
        }
    }
}
