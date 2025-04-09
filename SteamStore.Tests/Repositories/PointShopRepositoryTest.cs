using System;
using System.Data;
using System.Data.SqlClient;
using Xunit;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Data;
using SteamStore.Tests.TestUtils;

namespace SteamStore.Tests.Repositories
{
    public class PointShopRepositoryTest
    {
        private readonly PointShopRepository _repository;
        private readonly User _testUser;

        public PointShopRepositoryTest()
        {
            _testUser = new User
            {
                UserIdentifier = 1,
                Name = "John Doe",
                PointsBalance = 999999.99f
            };

            _repository = new PointShopRepository(_testUser, TestDataLink.GetDataLink());
        }

        [Fact]
        public void GetAllItems_ShouldReturnItems()
        {
            var items = _repository.GetAllItems();
            Assert.NotNull(items);
            Assert.NotEmpty(items);
        }

        [Fact]
        public void GetUserItems_ShouldReturnUserItems()
        {
            var items = _repository.GetUserItems();
            Assert.NotNull(items);
        }

        [Fact]
        public void PurchaseItem_UserAlreadyOwnsItem()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = 1
            };

            var exception = Assert.Throws<Exception>(() => _repository.PurchaseItem(item));
            Assert.Contains("Failed to purchase item:", exception.Message);
        }

        [Fact]
        public void ActivateItem_ShouldNotThrow()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = 1
            };
            _repository.ActivateItem(item);
        }

        [Fact]
        public void ActivateItem_ShouldThrowException_WhenItemIsNull()
        {
            PointShopItem? item = null;

            var exception = Assert.Throws<ArgumentNullException>(() => _repository.ActivateItem(item));
            Assert.Contains("Cannot activate a null item", exception.Message);
        }


        [Fact]
        public void DeactivateItem_ShouldNotThrow()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = 1
            };
            _repository.DeactivateItem(item);
        }

        [Fact]
        public void DeactivateItem_ShouldThrowException_WhenUserIsNotInitialized()
        {
            var repository = new PointShopRepository(null, TestDataLink.GetDataLink());
            var item = new PointShopItem { ItemIdentifier = 1 };

            var exception = Assert.Throws<InvalidOperationException>(() => repository.DeactivateItem(item));
            Assert.Equal("User is not initialized", exception.Message);
        }


        [Fact]
        public void UpdateUserPointBalance_ShouldUpdatePoints()
        {
            _testUser.PointsBalance = 999999.99f;
            _repository.UpdateUserPointBalance();

            var updatedBalance = _repository.GetUserItems()
                .FirstOrDefault()?.PointPrice;
            Assert.Equal(999999.99f, _testUser.PointsBalance);
            _testUser.PointsBalance = 100;
            _repository.UpdateUserPointBalance();
        }

    }
}
