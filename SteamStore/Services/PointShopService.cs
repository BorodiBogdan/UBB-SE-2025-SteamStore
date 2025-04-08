// <copyright file="PointShopService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    }
}