using Discord;
using Discord.Commands;
using MorkoBotRavenEdition.Models;
using MorkoBotRavenEdition.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static MorkoBotRavenEdition.Services.ShopService;

namespace MorkoBotRavenEdition.Modules
{
    [Group("shop")]
    class ShopModule : MorkoModuleBase
    {
        private readonly UserService _userService;
        private readonly ShopService _shopService;
        private readonly IServiceProvider _serviceProvider;

        public ShopModule(UserService userService, ShopService shopService, IServiceProvider serviceProvider)
        {
            _userService = userService;
            _shopService = shopService;
            _serviceProvider = serviceProvider;
        }

        [Command("items"), Summary("Displays a list of items you can purchase from the shop.")]
        public async Task GetItemsAsync()
        {
            IEnumerable<UserItemDefinition> items = _shopService.GetItems();
            await Context.User.SendMessageAsync("Welcome to the shop! Here's a list of the items you can purchase.");

            foreach (UserItemDefinition item in items)
            {
                string emojiPrefix = String.Empty;
                if (!String.IsNullOrWhiteSpace(item.Emoji))
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

                await Context.User.SendMessageAsync(String.Format("{0}{1}: **{2}**{3}, <:geocache:357917503894061059>**{4}** \"*{5}*\"", emojiPrefix, item.Name, item.Price, currency, item.MinLevel, item.Description));
            };
        }

        [Command("buy"), Summary("Purchases an item for yourself.")]
        public async Task BuyItemAsync([Summary("The item to purchase.")] string itemName)
        {
            UserProfile profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);
            UserItemDefinition item = _shopService.DefinitionFromItem(itemName);
            await _shopService.BuyItem(profile, item);

            string emojiPrefix = String.Empty;
            if (!String.IsNullOrWhiteSpace(item.Emoji))
                emojiPrefix = item.Emoji + " ";
            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully bought the item \"{0}{1}\"!", emojiPrefix, item.Name), Color.Green).Build());
        }

        [Command("add"), Summary("Adds an item to a user's inventory.")]
        public async Task AddItemAsync([Summary("The item to add.")] string itemName, [Summary("The amount of items to add (optional).")] int amount = 1, [Summary("The user to add the item to (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            await UserService.ThrowIfHasNoPermissions(Context, _serviceProvider, "Discord Admin");
            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            UserItemDefinition item = _shopService.DefinitionFromItem(itemName);
            await _shopService.AddItem(profile, item, amount);

            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully added {0} of the item \"{1}\" to the user {2}.", amount, item.Name, user.Username), Color.Green).Build());
        }

        [Command("trash"), Summary("Removes an item from a user's inventory.")]
        public async Task TrashItemAsync([Summary("The item to remove.")] string itemName, [Summary("The amount of items to remove (optional).")] int amount = 1, [Summary("The user to remove the item from (optional).")] IUser user = null)
        {
            if (user == null)
                user = Context.User;

            if (user != Context.User)
            {
                await UserService.ThrowIfHasNoPermissions(Context, _serviceProvider, "Discord Moderator");
                await _userService.ThrowIfCannotModify(Context.User, user);
            }

            UserProfile profile = await _userService.GetProfile(user.Id, Context.Guild.Id);
            UserItemDefinition item = _shopService.DefinitionFromItem(itemName);
            await _shopService.RemoveItem(profile, item, amount);

            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully removed {0} of the item \"{1}\" from the user {2}.", amount, item.Name, user.Username), Color.Green).Build());
        }

        [Command("use"), Summary("Uses an item, if the item supports it.")]
        public async Task UseItemAsync([Summary("The item to use.")] string itemName)
        {
            UserProfile profile = await _userService.GetProfile(Context.User.Id, Context.Guild.Id);
            UserItemDefinition item = _shopService.DefinitionFromItem(itemName);
            await _shopService.UseItem(profile, item);

            await Context.User.SendMessageAsync(String.Empty, false, GetResponseEmbed(String.Format("Successfully removed used the item \"{0}\"!", item.Name), Color.Green).Build());
        }
    }
}
