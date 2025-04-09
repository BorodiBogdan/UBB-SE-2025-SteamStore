using System;
using System.Data;
using System.Data.SqlClient;
using Moq;
using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories;
using Xunit;

namespace SteamStore.Tests.Repositories
{
    public class PointShopRepositoryTest
    {
        private readonly Mock<IDataLink> _mockDataLink;
        private readonly User _testUser;
        private readonly PointShopRepository _repository;
        private readonly DataTable _dataTable;

        public PointShopRepositoryTest()
        {
            _mockDataLink = new Mock<IDataLink>();
            _testUser = new User
            {
                UserIdentifier = 1,
                Name = "TestUser",
                PointsBalance = 1000,
                WalletBalance = 200,
                UserRole = User.Role.User
            };
            _repository = new PointShopRepository(_testUser, _mockDataLink.Object);

            _dataTable =  new DataTable();
            _dataTable.Columns.Add("ItemId", typeof(int));
            _dataTable.Columns.Add("Name", typeof(string));
            _dataTable.Columns.Add("Description", typeof(string));
            _dataTable.Columns.Add("ImagePath", typeof(string));
            _dataTable.Columns.Add("PointPrice", typeof(double));
            _dataTable.Columns.Add("ItemType", typeof(string));
            _dataTable.Columns.Add("IsActive", typeof(bool));

            _dataTable.Rows.Add(1, "Item1", "Description1", "/path1", 100.0, "Avatar", true);
            _dataTable.Rows.Add(2, "Item2", "Description2", "/path2", 200.0, "ProfileBackground", false);

        }

        [Fact]
        public void GetAllItems_ReturnsCorrectNumber()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteReader("GetAllPointShopItems", null))
                .Returns(_dataTable);

            var items = _repository.GetAllItems();

            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void GetAllItems_ThrowsException()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteReader("GetAllPointShopItems", null))
                .Throws(new Exception("Database error"));
            Assert.Throws<Exception>(() => _repository.GetAllItems());
        }

        [Fact]
        public void GetAllItems_ReturnsCorrectElement()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteReader("GetAllPointShopItems", null))
                .Returns(_dataTable);

            var items = _repository.GetAllItems();

            Assert.Equal("Item1", items[0].Name);
        }

        [Fact]
        public void GetUserItems_ReturnsCount()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteReader("GetUserPointShopItems", It.Is<SqlParameter[]>(p => (int)p[0].Value == _testUser.UserIdentifier)))
                .Returns(_dataTable);

            var items = _repository.GetUserItems();

            Assert.Equal(2, items.Count);
            Assert.True(items[0].IsActive);
            Assert.False(items[1].IsActive);
        }

        [Fact]
        public void GetUserItems_ReturnsActive()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteReader("GetUserPointShopItems", It.Is<SqlParameter[]>(p => (int)p[0].Value == _testUser.UserIdentifier)))
                .Returns(_dataTable);

            var items = _repository.GetUserItems();

            Assert.True(items[0].IsActive);
        }

        [Fact]
        public void GetUserItems_ReturnsNotActive()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteReader("GetUserPointShopItems", It.Is<SqlParameter[]>(p => (int)p[0].Value == _testUser.UserIdentifier)))
                .Returns(_dataTable);

            var items = _repository.GetUserItems();

            Assert.False(items[1].IsActive);
        }

        [Fact]
        public void PurchaseItem_UpdateUserPointsAndCallDatabase()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = 1,
                PointPrice = 500
            };

            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("PurchasePointShopItem", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            _repository.PurchaseItem(item);

            Assert.Equal(500, _testUser.PointsBalance);
            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("PurchasePointShopItem", It.IsAny<SqlParameter[]>()), Times.Once);
            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void ActivateItem_CallDatabase()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = 1
            };

            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("ActivatePointShopItem", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            _repository.ActivateItem(item);

            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("ActivatePointShopItem", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void ActivateItem_Exception()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("ActivatePointShopItem", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            Assert.Throws<ArgumentNullException>(() => _repository.ActivateItem(null));
        }

        [Fact]
        public void DeactivateItem_CallDatabase()
        {
            var item = new PointShopItem
            {
                ItemIdentifier = 1
            };

            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("DeactivatePointShopItem", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            _repository.DeactivateItem(item);

            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("DeactivatePointShopItem", It.IsAny<SqlParameter[]>()), Times.Once);
            
        }

        [Fact]
        public void DeactivateItem_Exception()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("DeactivatePointShopItem", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            Assert.Throws<ArgumentNullException>(() => _repository.DeactivateItem(null));
        }

        [Fact]
        public void UpdateUserPointBalance_ShouldCallDatabase()
        {
            _mockDataLink
                .Setup(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            _repository.UpdateUserPointBalance();

            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()), Times.Once);
        }
    }
}
