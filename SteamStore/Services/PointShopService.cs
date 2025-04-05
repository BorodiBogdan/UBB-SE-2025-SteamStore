using SteamStore.Models;
using SteamStore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamStore.Services
{
    public class PointShopService
    {
        private readonly PointShopRepository _repository;
        private readonly User _currentUser;
        private const string FILTER_TYPE_ALL = "All";

        public PointShopService(User currentUser, DataLink dataLink)
        {
            _currentUser = currentUser;
            _repository = new PointShopRepository(currentUser, dataLink);
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public List<PointShopItem> GetAllItems()
        {
            try
            {
                return _repository.GetAllItems();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving items: {ex.Message}", ex);
            }
        }

        public List<PointShopItem> GetUserItems()
        {
            try
            {
                return _repository.GetUserItems();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user items: {ex.Message}", ex);
            }
        }

        public void PurchaseItem(PointShopItem item)
        {
            try
            {
                _repository.PurchaseItem(item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error purchasing item: {ex.Message}", ex);
            }
        }

        public void ActivateItem(PointShopItem item)
        {
            try
            {
                _repository.ActivateItem(item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error activating item: {ex.Message}", ex);
            }
        }

        public void DeactivateItem(PointShopItem item)
        {
            try
            {
                _repository.DeactivateItem(item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deactivating item: {ex.Message}", ex);
            }
        }

        //// Filter items by type
        //public List<PointShopItem> FilterItemsByType(string itemType)
        //{
        //    try
        //    {
        //        return GetAllItems()
        //            .Where(item => item.ItemType.Equals(itemType, StringComparison.OrdinalIgnoreCase))
        //            .ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error filtering items by type: {ex.Message}", ex);
        //    }
        //}

        //// Filter items by price range
        //public List<PointShopItem> FilterItemsByPriceRange(double minPrice, double maxPrice)
        //{
        //    try
        //    {
        //        return GetAllItems()
        //            .Where(item => item.PointPrice >= minPrice && item.PointPrice <= maxPrice)
        //            .ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error filtering items by price range: {ex.Message}", ex);
        //    }
        //}

        //// Search items by name or description
        //public List<PointShopItem> SearchItems(string searchTerm)
        //{
        //    try
        //    {
        //        return GetAllItems()
        //            .Where(item => 
        //                item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
        //                item.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //            .ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error searching items: {ex.Message}", ex);
        //    }
        //}

        public List<PointShopItem> GetFilteredItems(string filterType, string searchText, double minPrice, double maxPrice)
        {
            try
            {
                var allItems = GetAllItems();
                var userItems = GetUserItems();

                var availableItems = allItems.Where(item =>
                    !userItems.Any(userItem => userItem.ItemId == item.ItemId)).ToList();

                if (!string.IsNullOrEmpty(filterType) && filterType != FILTER_TYPE_ALL)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetFilteredItems: {ex.Message}");
                return new List<PointShopItem>();
            }
        }
    }
} 