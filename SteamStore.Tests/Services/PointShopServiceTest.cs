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
        private readonly User _testUser;
        private readonly PointShopService _service;

        public PointShopServiceTest()
        {
            _testUser = new User
            {
                UserIdentifier = 1,
                Name = "John Doe",
                PointsBalance = 999999.99f
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
                ItemIdentifier = 7,
                PointPrice = 500
            };

            try
            {
                _service.PurchaseItem(newItem);

                Assert.True(_testUser.PointsBalance < 999999.99f);

                var userItems = _service.GetUserItems();
                Assert.Contains(userItems, item => item.ItemIdentifier == newItem.ItemIdentifier);
            }
            finally
            {
                _service.ResetUserInventory();

            }
            var afterCleanupItems = _service.GetUserItems();
            Assert.DoesNotContain(afterCleanupItems, item => item.ItemIdentifier == newItem.ItemIdentifier);
        }

        [Fact]
        public void ActivateItem_ShouldSetItemAsActive()
        {
            var itemToActivate = new PointShopItem
            {
                ItemIdentifier = 3
            };

            _service.ActivateItem(itemToActivate);

            var userItems = _service.GetUserItems();
            Assert.Contains(userItems, item => item.ItemIdentifier == 3 && item.IsActive);
        }


        [Fact]
        public void DeactivateItem_ShouldSetItemAsInactive()
        {
            var itemToDeactivate = new PointShopItem
            {
                ItemIdentifier = 1
            };

            _service.DeactivateItem(itemToDeactivate);

            var userItems = _service.GetUserItems();
            Assert.Contains(userItems, item => item.ItemIdentifier == 1 && !item.IsActive);
        }

        [Fact]
        public void GetFilteredItems_ShouldReturnFilteredItems()
        {
            var filteredItems = _service.GetFilteredItems("ProfileBackground", "Blue", 0, 1000);
            Assert.NotNull(filteredItems);
        }

        [Fact]
        public void CanUserPurchaseItem_ShouldReturnTrue_WhenUserCanPurchase()
        {
            var selectedItem = new PointShopItem
            {
                ItemIdentifier = 7,
                PointPrice = 500
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
                ItemIdentifier = 1,
                PointPrice = 500
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
                ItemIdentifier = 2,
                Name = "Red Background",
                PointPrice = 500,
                ItemType = "ProfileBackground"
            };

            var transactionHistory = new ObservableCollection<PointShopTransaction>();
            var result = _service.TryPurchaseItem(selectedItem, transactionHistory, _testUser, out var newTransaction);

            Assert.Equal(500, newTransaction.PointsSpent);

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
                new PointShopItem { ItemIdentifier = 1, IsActive = true },
                new PointShopItem { ItemIdentifier = 3, IsActive = false }
            };

            var toggledItem = _service.ToggleActivationForItem(1, userItems);

            Assert.True(toggledItem.IsActive);
        }

        [Fact]
        public void ToggleActivationForItem_NullItem()
        {
            var userItems = new ObservableCollection<PointShopItem>
            {
                new PointShopItem { ItemIdentifier = 1, IsActive = true },
                new PointShopItem { ItemIdentifier = 3, IsActive = false }
            };

            var toggledItem = _service.ToggleActivationForItem(2, userItems);

            Assert.Null(toggledItem);
        }


        [Fact]
        public void ToggleActivationForItem_AlreadyActive()
        {
            var userItems = new ObservableCollection<PointShopItem>
            {
                new PointShopItem { ItemIdentifier = 1, IsActive = true },
                new PointShopItem { ItemIdentifier = 3, IsActive = false }
            };

            var toggledItem = _service.ToggleActivationForItem(3, userItems);

            Assert.False(toggledItem.IsActive);
        }
    }
}

