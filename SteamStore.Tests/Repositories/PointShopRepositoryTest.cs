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
        const string userName = "John Doe";
        const int userIdentifier = 1;
        const float initialPointsBalance = 999999.99f;

        const int itemIdentifier1 = 1;
        const int itemIdentifier3 = 3;
        const float itemPointPrice = 9999999.99f;
        const float newPointBalance = 100;

        private readonly PointShopRepository _repository;
        private readonly PointShopRepository _nullRepository;
        private readonly User _testUser;

        public PointShopRepositoryTest()
        {
            _testUser = new User
            {
                UserIdentifier = userIdentifier,
                Name = userName,
                PointsBalance = initialPointsBalance
            };

            _repository = new PointShopRepository(_testUser, TestDataLink.GetDataLink());
            _nullRepository = new PointShopRepository(null, TestDataLink.GetDataLink());
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
        public void PurchaseItem_NullUser()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = itemIdentifier1
            };

            var exception = Assert.Throws<InvalidOperationException>(() => _nullRepository.PurchaseItem(item));
            Assert.Contains("User is not initialized", exception.Message);
        }

        [Fact]
        public void PurchaseItem_NullItem()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _repository.PurchaseItem(null));
            Assert.Contains("Cannot purchase a null item", exception.Message);

        }

        [Fact]
        public void PurchaseItem_InsufficentPoints()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = itemIdentifier3,
                PointPrice = itemPointPrice
            };

            var exception = Assert.Throws<Exception>(() => _repository.PurchaseItem(item));
            Assert.Contains("Insufficient points to purchase this item", exception.Message);
        }

        [Fact]
        public void ActivateItem_ShouldNotThrow()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = itemIdentifier1
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
        public void ActivateItem_NullUser()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = itemIdentifier1
            };
            var exception = Assert.Throws<InvalidOperationException>(() => _nullRepository.ActivateItem(item));
            Assert.Contains("User is not initialized", exception.Message);
        }

        [Fact]
        public void DeactivateItem_ShouldNotThrow()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = itemIdentifier1
            };
            _repository.DeactivateItem(item);
        }

        [Fact]
        public void DeactivateItem_ShouldThrowException_WhenUserIsNotInitialized()
        {
            var item = new PointShopItem { ItemIdentifier = itemIdentifier1 };

            var exception = Assert.Throws<InvalidOperationException>(() => _nullRepository.DeactivateItem(item));
            Assert.Equal("User is not initialized", exception.Message);
        }

        [Fact]
        public void DeactivateItem_NullItem()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _repository.DeactivateItem(null));
            Assert.Contains("Cannot deactivate a null item", exception.Message);

        }

        [Fact]
        public void UpdateUserPointBalance_ShouldUpdatePoints()
        {
            _testUser.PointsBalance = newPointBalance;
            _repository.UpdateUserPointBalance();

            var updatedBalance = _repository.GetUserItems()
                .FirstOrDefault()?.PointPrice;
            Assert.Equal(newPointBalance, _testUser.PointsBalance);
            _testUser.PointsBalance = initialPointsBalance;
            _repository.UpdateUserPointBalance();
        }

    }
}
