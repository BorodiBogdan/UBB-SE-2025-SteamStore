using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Moq;
using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Services;
using Xunit;

namespace SteamStore.Tests.Services
{
    public class PointShopServiceTest
    {
        private readonly Mock<IDataLink> _mockDataLink;
        private readonly User _testUser;
        private readonly PointShopService _service;

        public PointShopServiceTest()
        {
            _testUser = new User
            {
                UserIdentifier = 1,
                Name = "TestUser",
                PointsBalance = 1000,
                WalletBalance = 200,
                UserRole = User.Role.User
            };

            _mockDataLink = new Mock<IDataLink>();
            _service = new PointShopService(_testUser, _mockDataLink.Object);
        }

        [Fact]
        public void GetAllItems_ShouldReturnListOfPointShopItems()
        {
            var mockDataTable = CreateMockPointShopItemsTable();
            _mockDataLink.Setup(dl => dl.ExecuteReader("GetAllPointShopItems", null)).Returns(mockDataTable);
            var items = _service.GetAllItems();

            Assert.NotNull(items);
            Assert.NotEmpty(items);
            Assert.Equal(3, items.Count);
            Assert.Contains(items, item => item.Name == "Blue Background");
        }

        [Fact]
        public void GetUserItems_ShouldReturnListOfUserPointShopItems()
        {
            var mockDataTable = CreateMockUserItemsTable();
            _mockDataLink.Setup(dl => dl.ExecuteReader("GetUserPointShopItems", It.IsAny<SqlParameter[]>())).Returns(mockDataTable);

            var userItems = _service.GetUserItems();
            Assert.NotNull(userItems);
            Assert.NotEmpty(userItems);
            Assert.Equal(2, userItems.Count);
            Assert.Contains(userItems, item => item.Name == "Blue Background" && item.IsActive);
        }

        [Fact]
        public void PurchaseItem_ShouldDeductPointsAndAddItem()
        {
            var newItem = new PointShopItem
            {
                ItemIdentifier = 2,
                PointPrice = 500
            };

            _mockDataLink.Setup(dl => dl.ExecuteNonQuery("PurchasePointShopItem", It.IsAny<SqlParameter[]>())).Verifiable();
            _mockDataLink.Setup(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>())).Verifiable();

            _service.PurchaseItem(newItem);

            Assert.Equal(500, _testUser.PointsBalance);
            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("PurchasePointShopItem", It.IsAny<SqlParameter[]>()), Times.Once);
            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void ActivateItem_ShouldSetItemAsActive()
        {
            var itemToActivate = new PointShopItem
            {
                ItemIdentifier = 3 
            };

            _mockDataLink.Setup(dl => dl.ExecuteNonQuery("ActivatePointShopItem", It.IsAny<SqlParameter[]>())).Verifiable();
            _service.ActivateItem(itemToActivate);
            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("ActivatePointShopItem", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void DeactivateItem_ShouldSetItemAsInactive()
        {
            var itemToDeactivate = new PointShopItem
            {
                ItemIdentifier = 1
            };

            _mockDataLink.Setup(dl => dl.ExecuteNonQuery("DeactivatePointShopItem", It.IsAny<SqlParameter[]>())).Verifiable();
            _service.DeactivateItem(itemToDeactivate);
            _mockDataLink.Verify(dl => dl.ExecuteNonQuery("DeactivatePointShopItem", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        private DataTable CreateMockPointShopItemsTable()
        {
            var table = new DataTable();
            table.Columns.Add("ItemId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("ImagePath", typeof(string));
            table.Columns.Add("PointPrice", typeof(double));
            table.Columns.Add("ItemType", typeof(string));

            table.Rows.Add(1, "Blue Background", "A blue profile background", null, 500, "ProfileBackground");
            table.Rows.Add(2, "Red Background", "A red profile background", null, 500, "ProfileBackground");
            table.Rows.Add(3, "Gold Frame", "A gold avatar frame", null, 1000, "AvatarFrame");

            return table;
        }

        private DataTable CreateMockUserItemsTable()
        {
            var table = new DataTable();
            table.Columns.Add("ItemId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("ImagePath", typeof(string));
            table.Columns.Add("PointPrice", typeof(double));
            table.Columns.Add("ItemType", typeof(string));
            table.Columns.Add("IsActive", typeof(bool));

            table.Rows.Add(1, "Blue Background", "A blue profile background", null, 500, "ProfileBackground", true);
            table.Rows.Add(3, "Gold Frame", "A gold avatar frame", null, 1000, "AvatarFrame", false);

            return table;
        }
    }
}

