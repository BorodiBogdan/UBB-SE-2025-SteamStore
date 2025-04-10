using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SteamStore.Constants;
using SteamStore.Data;
using SteamStore.Repositories.Interfaces;
using Windows.System;

namespace SteamStore.Tests.Repositories
{
    public class UserGameRepositoryTest
    {
        private readonly Mock<IDataLink> mockDataLink;
        private readonly User testUser;
        private readonly UserGameRepository userGameRepository;

        private const int TEST_USER_ID = 100;
        private const float TEST_USER_WALLET = 100;
        private const float TEST_USER_POINTS = 100;
        private const int TEST_GAME_ID = 1;
        private const string TEST_GAME_NAME = "Test Game";
        private const decimal TEST_GAME_PRICE = 29.99m;

        public UserGameRepositoryTest()
        {
            mockDataLink = new Mock<IDataLink>();
            testUser = new User { UserIdentifier = TEST_USER_ID, WalletBalance = TEST_USER_WALLET, PointsBalance = TEST_USER_POINTS };
            userGameRepository = new UserGameRepository(mockDataLink.Object, testUser);
        }

        [Fact]
        public void IsGamePurchased_ReturnsTrue_WhenGamePurchased()
        {
            var game = new Game { Identifier = TEST_GAME_ID };

            mockDataLink.Setup(dl => dl.ExecuteScalar<int>("IsGamePurchased", It.IsAny<SqlParameter[]>())).Returns(1);

            var result = userGameRepository.IsGamePurchased(game);

            Assert.True(result);
        }

        [Fact]
        public void IsGamePurchased_ReturnsFalse_WhenGameNotPurchased()
        {
            var game = new Game { Identifier = TEST_GAME_ID };

            mockDataLink.Setup(dl => dl.ExecuteScalar<int>("IsGamePurchased", It.IsAny<SqlParameter[]>()))
                .Returns(0);

            var result = userGameRepository.IsGamePurchased(game);

            Assert.False(result);
        }

        [Fact]
        public void RemoveGameFromWishlist_ShouldExecuteSuccessfully_WhenValidGameIsProvided()
        {
            var game = new Game { Identifier = 1 };
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("RemoveGameFromWishlist", It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.RemoveGameFromWishlist(game);

            mockDataLink.Verify(dl => dl.ExecuteNonQuery("RemoveGameFromWishlist", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void RemoveGameFromWishlist_ShouldThrowException_WhenDatabaseFails()
        {
            var game = new Game { Identifier = 1 };
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("RemoveGameFromWishlist", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Database error"));

            var exception = Assert.Throws<Exception>(() => userGameRepository.RemoveGameFromWishlist(game));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public void AddGameToPurchased_ThrowsException_WhenInsufficientFunds()
        {
            var game = new Game { Identifier = TEST_GAME_ID, Price = 200m }; // Higher price than wallet balance.

            var exception = Assert.Throws<Exception>(() => userGameRepository.AddGameToPurchased(game));

            Assert.Equal("Insufficient funds", exception.Message);
        }

        [Fact]
        public void AddGameToPurchased_SuccessfullyPurchasesGame_WhenSufficientFunds()
        {
            var game = new Game { Identifier = TEST_GAME_ID, Price = 50m };

            mockDataLink.Setup(dl => dl.ExecuteNonQuery("AddGameToPurchased", It.IsAny<SqlParameter[]>()))
                .Verifiable();

            userGameRepository.AddGameToPurchased(game);

            Assert.Equal(50, testUser.WalletBalance);
            mockDataLink.Verify(dl => dl.ExecuteNonQuery("AddGameToPurchased", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_ShouldExecuteSuccessfully_WhenValidGameIsProvided()
        {
            var game = new Game { Identifier = 1 };
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("AddGameToWishlist", It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.AddGameToWishlist(game);

            mockDataLink.Verify(dl => dl.ExecuteNonQuery("AddGameToWishlist", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_ShouldThrowException_WhenDatabaseFails()
        {
            var game = new Game { Identifier = 1 };
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("AddGameToWishlist", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Database error"));

            var exception = Assert.Throws<Exception>(() => userGameRepository.AddGameToWishlist(game));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public void GetGameTags_ShouldReturnTags_WhenTagsExist()
        {
            var gameId = 1;
            var dataTable = new DataTable();
            dataTable.Columns.Add(SqlConstants.TagNameColumn);
            dataTable.Rows.Add("Action");
            dataTable.Rows.Add("Adventure");

            mockDataLink.Setup(dl => dl.ExecuteReader("getGameTags", It.IsAny<SqlParameter[]>()))
                        .Returns(dataTable);

            var tags = userGameRepository.GetGameTags(gameId);

            Assert.Equal(2, tags.Length);
            Assert.Contains("Action", tags);
            Assert.Contains("Adventure", tags);
        }

        [Fact]
        public void GetGameTags_ShouldThrowException_WhenDatabaseFails()
        {
            var gameId = 1;
            mockDataLink.Setup(dl => dl.ExecuteReader("getGameTags", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Database error"));

            var exception = Assert.Throws<Exception>(() => userGameRepository.GetGameTags(gameId));
            Assert.Equal("Error getting tags for game 1: Database error", exception.Message);
        }

        [Fact]
        public void GetGameOwnerCount_ShouldReturnValidCount_WhenDataIsAvailable()
        {
            var gameId = 1;
            var dataTable = new DataTable();
            dataTable.Columns.Add("OwnerCount");
            dataTable.Rows.Add(5);

            mockDataLink.Setup(dl => dl.ExecuteReader("GetGameOwnerCount", It.IsAny<SqlParameter[]>()))
                        .Returns(dataTable);

            var ownerCount = userGameRepository.GetGameOwnerCount(gameId);

            Assert.Equal(5, ownerCount);
        }

        [Fact]
        public void GetGameOwnerCount_ShouldReturnDefaultValue_WhenNoDataAvailable()
        {
            var gameId = 1;
            var dataTable = new DataTable(); // Empty table

            mockDataLink.Setup(dl => dl.ExecuteReader("GetGameOwnerCount", It.IsAny<SqlParameter[]>()))
                        .Returns(dataTable);

            var ownerCount = userGameRepository.GetGameOwnerCount(gameId);

            Assert.Equal(0, ownerCount); // Default value of 0 owners
        }

        [Fact]
        public void AddPointsForPurchase_ShouldAddPoints_WhenValidPurchaseAmount()
        {
            var purchaseAmount = 10.0f;
            testUser.PointsBalance = 0; // Initially no points
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.AddPointsForPurchase(purchaseAmount);

            Assert.Equal(1210, testUser.PointsBalance); // 121 points for every $1
            mockDataLink.Verify(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddPointsForPurchase_ShouldThrowException_WhenDatabaseFails()
        {
            var purchaseAmount = 10.0f;
            mockDataLink.Setup(dl => dl.ExecuteNonQuery("UpdateUserPointBalance", It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception("Database error"));

            var exception = Assert.Throws<Exception>(() => userGameRepository.AddPointsForPurchase(purchaseAmount));
            Assert.Equal("Failed to add points for purchase: Database error", exception.Message);
        }

        [Fact]
        public void GetUserPointsBalance_ReturnsCorrectBalance()
        {
            testUser.PointsBalance = 150;
            var result = userGameRepository.GetUserPointsBalance();
            Assert.Equal(150, result);
        }

        [Fact]
        public void GetWishlistGames_ShouldReturnGames_WhenDataIsAvailable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(SqlConstants.GameIdColumn, typeof(int));
            dataTable.Columns.Add(SqlConstants.GameNameColumn, typeof(string));
            dataTable.Columns.Add(SqlConstants.GamePriceColumn, typeof(decimal));
            dataTable.Columns.Add("Description", typeof(string));
            dataTable.Columns.Add(SqlConstants.ImageUrlColumn, typeof(string));
            dataTable.Columns.Add(SqlConstants.MinimumRequirementsColumn, typeof(string));
            dataTable.Columns.Add(SqlConstants.RecommendedRequirementsColumn, typeof(string));
            dataTable.Columns.Add(SqlConstants.GameStatusColumn, typeof(string));
            dataTable.Columns.Add(SqlConstants.DiscountColumn, typeof(decimal));
            dataTable.Columns.Add(SqlConstants.RatingColumn, typeof(decimal));

            // Adding rows with mock data
            dataTable.Rows.Add(1, "Game1", 19.99m, "Description1", "ImagePath1", "MinReq1", "RecReq1", "Available", 10.0m, 4.5m);
            dataTable.Rows.Add(2, "Game2", 29.99m, "Description2", "ImagePath2", "MinReq2", "RecReq2", "Available", 15.0m, 4.0m);

            // Setting up the mockDataLink to return the mock DataTable
            mockDataLink.Setup(dl => dl.ExecuteReader("GetWishlistGames", It.IsAny<SqlParameter[]>()))
                        .Returns(dataTable);

            // Act
            var games = userGameRepository.GetWishlistGames();

            // Assert
            Assert.Equal(2, games.Count);
        }

        [Fact]
        public void GetWishlistGames_ShouldReturnEmpty_WhenNoDataAvailable()
        {
            var dataTable = new DataTable(); // Empty table

            mockDataLink.Setup(dl => dl.ExecuteReader("GetWishlistGames", It.IsAny<SqlParameter[]>()))
                        .Returns(dataTable);

            var games = userGameRepository.GetWishlistGames();

            Assert.Empty(games);
        }
    }
}
