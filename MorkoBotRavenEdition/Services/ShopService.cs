using Discord.WebSocket;
using MorkoBotRavenEdition.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MorkoBotRavenEdition.Utilities;

namespace MorkoBotRavenEdition.Services
{
    class ShopService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly BotDbContext _context;

        public enum CurrencyType
        {
            SewerCoin,
            Experience,
            Health
        }

        // Item action delegates

        /// <summary>
        /// Called by the user service when an item is used.
        /// </summary>
        public delegate Task<UserProfile> UseDelegate(IServiceProvider serviceProvider, UserProfile profile);

        /// <summary>
        /// Called by the shop service when an item is added to a user's inventory.
        /// </summary>
        public delegate Task<UserProfile> AddDelegate(IServiceProvider serviceProvider, UserProfile profile);

        /// <summary>
        /// Called by the shop service when an item is removed from a user's inventory.
        /// </summary>
        public delegate Task<UserProfile> RemoveDelegate(IServiceProvider serviceProvider, UserProfile profile);

        public ShopService(IServiceProvider serviceProvider, DiscordSocketClient dsc, BotDbContext dbContext)
        {
            _serviceProvider = serviceProvider;
            _client = dsc;
            _context = dbContext;
        }

        /// <summary>
        /// Gets a list of items that can be purchased from the shop.
        /// </summary>
        public IEnumerable<UserItemDefinition> GetItems()
        {
            return HardcodedShopItems.Items;
        }

        /// <summary>
        /// Attempts to purchase an item on behalf of a user.
        /// This method will validate the user's currency balance,
        /// and throw an ArgumentException if it's too low.
        /// </summary>
        public async Task BuyItem(UserProfile profile, UserItemDefinition item, int amount = 1)
        {
            int finalPrice = item.Price * amount;

            // Do internal checks if the user can afford the item
            switch (item.CurrencyType)
            {
                case CurrencyType.Experience:
                    throw new NotImplementedException();
                case CurrencyType.Health:
                    throw new NotImplementedException();
                case CurrencyType.SewerCoin:
                    if (profile.OpenSewerTokens < finalPrice)
                        throw new ArgumentException("You cannot afford this item.", "item");
                    break;
            }

            await AddItem(profile, item, amount);
        }

        /// <summary>
        /// Adds an item to a user. If the user has no items of the type,
        /// it will add a new entry to the database, and if they have one or more items,
        /// it will increment the amount.
        /// </summary>
        public async Task AddItem(UserProfile profile, UserItemDefinition item, int amount = 1)
        {
            UserItem userItem = _context.UserItems.Where(w => w.UserId == profile.Identifier && w.Guild == profile.GuildIdentifier && w.Name == item.Name).FirstOrDefault();
            if (userItem != null)
            {
                userItem.Amount += amount;
                _context.Update(userItem);
            }
            else
            {
                userItem = new UserItem
                {
                    Name = item.Name,
                    UserId = profile.Identifier,
                    Amount = amount,
                    OriginalPrice = item.Price,
                    Guild = profile.GuildIdentifier,
                };

                _context.Add(userItem);
            }

            await _context.SaveChangesAsync();

            // Add action is only invoked ONCE regardless of amount of items.
            await InvokeAddAction(profile, item);
        }

        /// <summary>
        /// Removes an item from a user's inventory.
        /// </summary>
        public async Task RemoveItem(UserProfile profile, UserItemDefinition item, int amount = 1)
        {
            UserItem userItem = _context.UserItems.Where(w => w.UserId == profile.Identifier && w.Guild == profile.GuildIdentifier && w.Name == item.Name).FirstOrDefault();
            if (userItem == null)
                throw new ArgumentException("User profile contains no items of this type in this guild.", "item");

            if (userItem.Amount < amount)
                throw new ArgumentException("Cannot remove more items than the user has.", "amount");

            if (userItem.Amount == amount)
            {
                // ItemRemoved will only be called ONCE if more than one item is removed.
                await InvokeRemoveAction(profile, item);

                _context.Remove(userItem);
                await _context.SaveChangesAsync();
                return;
            }

            if (userItem.Amount > amount)
            {
                // ItemRemoved will only be called ONCE if more than one item is removed.
                await InvokeRemoveAction(profile, item);

                userItem.Amount = userItem.Amount - amount;
                _context.Update(userItem);
                await _context.SaveChangesAsync();
                return;
            }
        }

        /// <summary>
        /// Attempts to use an item by invoking the use action.
        /// This will throw an exception if the item is not usable.
        /// </summary>
        public async Task UseItem(UserProfile profile, UserItemDefinition item)
        {
            await InvokeUseAction(profile, item);
        }

        /// <summary>
        /// Maps a item name to an item definition, and
        /// returns null if the definition cannot be mapped.
        /// </summary>
        public UserItemDefinition DefinitionFromItem(string itemName)
        {
            UserItemDefinition item = GetItems().Where(i => i.Name == itemName).FirstOrDefault();
            if (item == null)
                throw new ArgumentException("Could not find an item with the specified name.", "itemName");

            return item;
        }

        private async Task InvokeUseAction(UserProfile profile, UserItemDefinition item)
        {
            if (item.ItemUsed == null)
                return;

            profile = await item.ItemUsed?.Invoke(_serviceProvider, profile);
            await _serviceProvider.GetService<UserService>().SaveProfile(profile);
        }

        private async Task InvokeAddAction(UserProfile profile, UserItemDefinition item)
        {
            if (item.ItemAdded == null)
                return;

            profile = await item.ItemAdded?.Invoke(_serviceProvider, profile);
            await _serviceProvider.GetService<UserService>().SaveProfile(profile);
        }

        private async Task InvokeRemoveAction(UserProfile profile, UserItemDefinition item)
        {
            if (item.ItemRemoved == null)
                return;

            profile = await item.ItemRemoved?.Invoke(_serviceProvider, profile);
            await _serviceProvider.GetService<UserService>().SaveProfile(profile);
        }
    }
}
