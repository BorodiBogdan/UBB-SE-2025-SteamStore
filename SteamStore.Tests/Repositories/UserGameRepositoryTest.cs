using Moq;
using SteamStore.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Tests.Repositories
{
    public class UserGameRepositoryTest
    {
        private readonly Mock<IDataLink> mockDataLink;
        private readonly User testUser;
        private readonly UserGameRepository repository;

        public UserGameRepositoryTest()
        {
            mockDataLink = new Mock<IDataLink>();
            testUser = new User { UserIdentifier = 1, WalletBalance = 50, PointsBalance = 0 };
            repository = new UserGameRepository(mockDataLink.Object, testUser);
        }

        [Fact]
        public void IsGamePurchased_ReturnsTrue_WhenGameIsOwned()
        {
            var game = new Game { Identifier = 1 };
            mockDataLink.Setup(dl => dl.ExecuteScalar<int>("IsGamePurchased", It.IsAny<SqlParameter[]>())).Returns(1);
            var result = repository.IsGamePurchased(game);
            Assert.True(result);
        }

        [Fact]
        public void AddGameToPurchased_SufficientFunds_AddsPointsAndDeductsBalance()
        {
            var game = new Game { Identifier = 2, Price = 10 };

            mockDataLink.Setup(dl => dl.ExecuteNonQuery("AddGameToPurchased", It.IsAny<SqlParameter[]>()))
                        .Verifiable();
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            repository.AddGameToPurchased(game);

            Assert.Equal(40, testUser.WalletBalance);
            Assert.Equal(1210, testUser.PointsBalance);
            mockDataLink.VerifyAll();
        }

        [Fact]
        public void AddGameToPurchased_InsufficientFunds_ThrowsException()
        {
            var game = new Game { Identifier = 2, Price = 100 };
            var ex = Assert.Throws<Exception>(() => repository.AddGameToPurchased(game));
            Assert.Equal("Insufficient funds", ex.Message);
        }

        [Fact]
        public void AddPointsForPurchase_AddsCorrectPoints()
        {
            repository.AddPointsForPurchase(5);
            Assert.Equal(605, testUser.PointsBalance);
            mockDataLink.Verify(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void GetUserPointsBalance_ReturnsCorrectBalance()
        {
            testUser.PointsBalance = 150;
            var result = repository.GetUserPointsBalance();
            Assert.Equal(150, result);
        }

        [Fact]
        public void RemoveGameFromWishlist_ExecutesCommand()
        {
            var game = new Game { Identifier = 3 };
            repository.RemoveGameFromWishlist(game);
            mockDataLink.Verify(dl => dl.ExecuteNonQuery("RemoveGameFromWishlist", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_ExecutesCommand()
        {
            var game = new Game { Identifier = 3 };
            repository.AddGameToWishlist(game);
            mockDataLink.Verify(dl => dl.ExecuteNonQuery("AddGameToWishlist", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void GetGameOwnerCount_ReturnsCorrectCount()
        {
            var table = new DataTable();
            table.Columns.Add("OwnerCount", typeof(int));
            var row = table.NewRow();
            row["OwnerCount"] = 5;
            table.Rows.Add(row);

            mockDataLink.Setup(dl => dl.ExecuteReader("GetGameOwnerCount", It.IsAny<SqlParameter[]>())).Returns(table);

            var count = repository.GetGameOwnerCount(1);
            Assert.Equal(5, count);
        }

        [Fact]
        public void GetGameTags_ReturnsCorrectTags()
        {
            var table = new DataTable();
            table.Columns.Add("tag_name", typeof(string));
            table.Rows.Add("RPG");
            table.Rows.Add("Adventure");

            mockDataLink.Setup(dl => dl.ExecuteReader("getGameTags", It.IsAny<SqlParameter[]>())).Returns(table);

            var tags = repository.GetGameTags(1);
            Assert.Equal(new[] { "RPG", "Adventure" }, tags);
        }

    }
}
