using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using Microsoft.UI.Xaml.Controls.Primitives;
using Moq;
using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Services;
using SteamStore.Tests.TestUtils;
using Xunit;

namespace SteamStore.Tests.Services
{
    public class PointShopServiceTest
    {
        const string userName = "John Doe";
        const int userIdentifier = 1;
        const float initialPointsBalance = 999999.99f;

        const int purchaseItemIdentifier = 7;
        const int purchaseItemPointPrice = 500;

        const int activateItemIdentifier = 3;
        const int deactivateItemIdentifier = 1;

        const string filterType = "ProfileBackground";
        const string filterText = "Blue";
        const double priceMinimum = 0;
        const double priceMaximum = 1000;

        const int canPurchaseItemIdentifier = 1;

        const int itemIdentifier1 = 1;
        const int itemIdentifier2 = 2;
        const int itemIdentifier3 = 3;
        const string itemName = "Red Background";
        const int itemPointPrice = 500;
        const string itemType = "ProfileBackground";

        private readonly User _testUser;
        private readonly PointShopService _service;

        public PointShopServiceTest()
        {
            _testUser = new User
            {
                UserIdentifier = userIdentifier,
                Name = userName,
                PointsBalance = initialPointsBalance
            };

            _service = new PointShopService(_testUser, TestDataLink.GetDataLink());
        }

        [Fact]
        public void GetCurrentUser_ShouldReturnCurrentUser()
        {
            var user = _service.GetCurrentUser();
            Assert.Equal(_testUser.UserIdentifier, user.UserIdentifier);
        }


        [Fact]
        public void GetAllItems_ShouldReturnListOfPointShopItems()
        {
            var items = _service.GetAllItems();
            Assert.All(items, item => Assert.NotNull(item.Name));
        }

        [Fact]
        public void GetUserItems_ShouldReturnListOfUserPointShopItems()
        {
            var userItems = _service.GetUserItems();
            Assert.All(userItems, item => Assert.NotNull(item.Name));
        }

        [Fact]
        public void PurchaseItem_ShouldDeductPointsAndAddItem()
        {
            var newItem = new PointShopItem
            {
                ItemIdentifier = purchaseItemIdentifier,
                PointPrice = purchaseItemPointPrice
            };

            try
            {
                _service.PurchaseItem(newItem);
                var userItems = _service.GetUserItems();
                Assert.Contains(userItems, item => item.ItemIdentifier == newItem.ItemIdentifier);
            }
            finally
            {
                _service.ResetUserInventory();

            }
        }

        [Fact]
        public void ActivateItem_ShouldSetItemAsActive()
        {
            var itemToActivate = new PointShopItem
            {
                ItemIdentifier = activateItemIdentifier
            };

            _service.ActivateItem(itemToActivate);

            var userItems = _service.GetUserItems();
            Assert.Contains(userItems, item => item.ItemIdentifier == activateItemIdentifier && item.IsActive);
        }


        [Fact]
        public void DeactivateItem_ShouldSetItemAsInactive()
        {
            var itemToDeactivate = new PointShopItem
            {
                ItemIdentifier = deactivateItemIdentifier
            };

            _service.DeactivateItem(itemToDeactivate);

            var userItems = _service.GetUserItems();
            Assert.Contains(userItems, item => item.ItemIdentifier == deactivateItemIdentifier && !item.IsActive);
        }

        [Fact]
        public void GetFilteredItems_ShouldReturnFilteredItems()
        {
            var filteredItems = _service.GetFilteredItems(filterType, filterText, priceMinimum, priceMaximum);
            Assert.NotNull(filteredItems);
        }

        [Fact]
        public void CanUserPurchaseItem_ShouldReturnTrue_WhenUserCanPurchase()
        {
            var selectedItem = new PointShopItem
            {
                ItemIdentifier = purchaseItemIdentifier,
                PointPrice = purchaseItemPointPrice
            };

            var userItems = _service.GetUserItems();
            var canPurchase = _service.CanUserPurchaseItem(_testUser, selectedItem, userItems);

            Assert.True(canPurchase);

            if (userItems.Exists(item => item.ItemIdentifier == selectedItem.ItemIdentifier))
            {
                _service.DeactivateItem(selectedItem);
                userItems.RemoveAll(item => item.ItemIdentifier == selectedItem.ItemIdentifier);
            }
        }

        [Fact]
        public void CanUserPurchaseItem_AlreadyOwns()
        {
            var selectedItem = new PointShopItem
            {
                ItemIdentifier = canPurchaseItemIdentifier,
                PointPrice = purchaseItemPointPrice
            };

            var userItems = _service.GetUserItems();
            var canPurchase = _service.CanUserPurchaseItem(_testUser, selectedItem, userItems);

            Assert.False(canPurchase);
        }

        [Fact]
        public void CanUserPurchaseItem_NullItem()
        {
            var userItems = _service.GetUserItems();
            var canPurchase = _service.CanUserPurchaseItem(_testUser, null, userItems);
            Assert.False(canPurchase);
        }

        [Fact]
        public void GetAvailableItems_ShouldReturnItemsNotOwnedByUser()
        {
            var availableItems = _service.GetAvailableItems(_testUser);
            Assert.All(availableItems, item => Assert.DoesNotContain(_service.GetUserItems(), userItem => userItem.ItemIdentifier == item.ItemIdentifier));
        }

        [Fact]
        public void TryPurchaseItem_ReturnTrue()
        {
            var selectedItem = new PointShopItem
            {
                ItemIdentifier = itemIdentifier2,
                Name = itemName,
                PointPrice = itemPointPrice,
                ItemType = itemType
            };

            var transactionHistory = new ObservableCollection<PointShopTransaction>();
            var result = _service.TryPurchaseItem(selectedItem, transactionHistory, _testUser, out var newTransaction);

            Assert.Equal(itemPointPrice, newTransaction.PointsSpent);

            var userItems = _service.GetUserItems();
            if (userItems.Exists(item => item.ItemIdentifier == selectedItem.ItemIdentifier))
            {
                _service.DeactivateItem(selectedItem);
                userItems.RemoveAll(item => item.ItemIdentifier == selectedItem.ItemIdentifier);
            }
        }

        [Fact]
        public void TryPurchaseItem_ReturnFasle()
        {
            var transactionHistory = new ObservableCollection<PointShopTransaction>();
            var result = _service.TryPurchaseItem(null, transactionHistory, _testUser, out var newTransaction);

            Assert.False(result);
        }

        [Fact]
        public void ToggleActivationForItem_ShouldActivateOrDeactivateItem()
        {
            var userItems = new ObservableCollection<PointShopItem>
            {
                new PointShopItem { ItemIdentifier = itemIdentifier1, IsActive = true },
                new PointShopItem { ItemIdentifier = itemIdentifier3, IsActive = false }
            };

            var toggledItem = _service.ToggleActivationForItem(itemIdentifier1, userItems);

            Assert.True(toggledItem.IsActive);
        }

        [Fact]
        public void ToggleActivationForItem_NullItem()
        {
            var userItems = new ObservableCollection<PointShopItem>
            {
                new PointShopItem { ItemIdentifier = itemIdentifier1, IsActive = true },
                new PointShopItem { ItemIdentifier = itemIdentifier3, IsActive = false }
            };

            var toggledItem = _service.ToggleActivationForItem(itemIdentifier2, userItems);

            Assert.Null(toggledItem);
        }


        [Fact]
        public void ToggleActivationForItem_AlreadyActive()
        {
            var userItems = new ObservableCollection<PointShopItem>
            {
                new PointShopItem { ItemIdentifier = itemIdentifier1, IsActive = true },
                new PointShopItem { ItemIdentifier = itemIdentifier3, IsActive = false }
            };

            var toggledItem = _service.ToggleActivationForItem(itemIdentifier3, userItems);

            Assert.False(toggledItem.IsActive);
        }
    }
}

