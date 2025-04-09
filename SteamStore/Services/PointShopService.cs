// <copyright file="PointShopService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SteamStore.Constants;
    using SteamStore.Data;
    using SteamStore.Models;
    using SteamStore.Repositories;
    using SteamStore.Repositories.Interfaces;
    using SteamStore.Services.Interfaces;

    public class PointShopService : IPointShopService
    {
        private const string FILTERTYPEALL = "All";
        private readonly IPointShopRepository repository;
        private readonly User currentUser;

        public PointShopService(User currentUser, IDataLink dataLink)
        {
            this.currentUser = currentUser;
            this.repository = new PointShopRepository(currentUser, dataLink);
        }

        public User GetCurrentUser()
        {
            return this.currentUser;
        }

        public List<PointShopItem> GetAllItems()
        {
            try
            {
                return this.repository.GetAllItems();
            }
            catch (Exception exception)
            {
                throw new Exception($"Error retrieving items: {exception.Message}", exception);
            }
        }

        public List<PointShopItem> GetUserItems()
        {
            try
            {
                return this.repository.GetUserItems();
            }
            catch (Exception exception)
            {
                throw new Exception($"Error retrieving user items: {exception.Message}", exception);
            }
        }

        public void PurchaseItem(PointShopItem item)
        {
            try
            {
                this.repository.PurchaseItem(item);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error purchasing item: {exception.Message}", exception);
            }
        }

        public void ActivateItem(PointShopItem item)
        {
            try
            {
                this.repository.ActivateItem(item);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error activating item: {exception.Message}", exception);
            }
        }

        public void DeactivateItem(PointShopItem item)
        {
            try
            {
                this.repository.DeactivateItem(item);
            }
            catch (Exception exception)
            {
                throw new Exception($"Error deactivating item: {exception.Message}", exception);
            }
        }

        public List<PointShopItem> GetFilteredItems(string filterType, string searchText, double minPrice, double maxPrice)
        {
            try
            {
                var allItems = this.GetAllItems();
                var userItems = this.GetUserItems();

                var availableItems = allItems.Where(item =>
                    !userItems.Any(userItem => userItem.ItemIdentifier == item.ItemIdentifier)).ToList();

                if (!string.IsNullOrEmpty(filterType) && filterType != FILTERTYPEALL)
                {
                    availableItems = availableItems
                        .Where(item => item.ItemType.Equals(filterType, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                availableItems = availableItems
                    .Where(item => item.PointPrice >= minPrice && item.PointPrice <= maxPrice)
                    .ToList();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    availableItems = availableItems
                        .Where(item =>
                            item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return availableItems;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetFilteredItems: {exception.Message}");
                return new List<PointShopItem>();
            }
        }

        public bool CanUserPurchaseItem(User user, PointShopItem selectedItem, IEnumerable<PointShopItem> userItems)
        {
            if (user == null || selectedItem == null)
            {
                return false;
            }

            bool alreadyOwns = false;
            foreach (var item in userItems)
            {
                if (item.ItemIdentifier == selectedItem.ItemIdentifier)
                {
                    alreadyOwns = true;
                    break;
                }
            }

            bool hasEnoughPoints = user.PointsBalance >= selectedItem.PointPrice;

            return !alreadyOwns && hasEnoughPoints;
        }

        public List<PointShopItem> GetAvailableItems(User user)
        {
            var allItems = this.GetAllItems();
            var userItems = this.GetUserItems();
            const int InitialIndexAllItems = 0;
            const int InitialIndexUserItems = 0;

            var availableItems = new List<PointShopItem>();

            for (int indexForAllItems = InitialIndexAllItems; indexForAllItems < allItems.Count; indexForAllItems++)
            {
                bool owned = false;

                for (int indexForUsersItems = InitialIndexUserItems; indexForUsersItems < userItems.Count; indexForUsersItems++)
                {
                    if (allItems[indexForAllItems].ItemIdentifier == userItems[indexForUsersItems].ItemIdentifier)
                    {
                        owned = true;
                        break;
                    }
                }

                if (!owned)
                {
                    availableItems.Add(allItems[indexForAllItems]);
                }
            }

            return availableItems;
        }

        public bool TryPurchaseItem(PointShopItem selectedItem, ObservableCollection<PointShopTransaction> transactionHistory, User user, out PointShopTransaction newTransaction)
        {
            const int InitialIndexOfTransaction = 0;
            const int IncrementingValue = 1;
            newTransaction = null;

            if (selectedItem == null || user == null)
            {
                return false;
            }

            // Purchase item
            try
            {
                // Check if transaction already exists
                bool exists = false;
                for (int idexOfTransaction = InitialIndexOfTransaction; idexOfTransaction < transactionHistory.Count; idexOfTransaction++)
                {
                    var t = transactionHistory[idexOfTransaction];
                    if (t.ItemName == selectedItem.Name &&
                        Math.Abs(t.PointsSpent - selectedItem.PointPrice) < PointShopConstants.MINMALDIFFERENCEVALUECOMPARISON)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    newTransaction = new PointShopTransaction(
                        transactionHistory.Count + IncrementingValue,
                        selectedItem.Name,
                        selectedItem.PointPrice,
                        selectedItem.ItemType);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public PointShopItem ToggleActivationForItem(int itemId, ObservableCollection<PointShopItem> userItems)
        {
            PointShopItem item = null;

            foreach (var userItem in userItems)
            {
                if (userItem.ItemIdentifier == itemId)
                {
                    item = userItem;
                    break;
                }
            }
            if (item == null)
            {
                return item;
            }

            if (item.IsActive)
            {
                this.DeactivateItem(item);
                return item;
            }
            else
            {
                this.ActivateItem(item);
                return item;
            }
        }
    }
}