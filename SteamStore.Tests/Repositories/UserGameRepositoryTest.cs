using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Moq;
using SteamStore.Constants;
using SteamStore.Data;
using SteamStore.Repositories.Interfaces;
using Xunit;

namespace SteamStore.Tests.Repositories
{
    public class UserGameRepositoryTest
    {
        private readonly Mock<IDataLink> mockDataLink;
        private readonly User mockUser;
        private readonly UserGameRepository userGameRepository;

        private const int TestUserId = 100;
        private const float InitialWalletBalance = 100.0f;
        private const float InitialPointsBalance = 100.0f;
        private const float PointBalanceMinimum = 0.0f;
        private const int TestGameId = 1;
        private const decimal TestGamePriceExpensive = 200.0m;
        private const decimal TestGamePriceCheap = 10.0m;
        private const float TestGameCheckPrice = 90.0f;
        private const int GetNumberGameTagsExpected = 2;
        private const string ExceptionMessageDatabaseError = "Database error";
        private const string ExceptionMessageInsufficientFunds = "Insufficient funds";
        private const string ExceptionMessageFailAddPoints = "Failed to add points for purchase: Database error";
        private const string TestTag1 = "Action";
        private const string TestTag2 = "Adventure";
        private const string OwnerCountParameter = "OwnerCount";
        private const int OwnerCountValue = 5;
        private const int OwnerCountMinimum = 0;
        private const float TestPurchaseAmount = 10.0f;
        private const int TestRewardPoints = 1210;
        private const int Game1Identifier = 1;
        private const string Game1Name = "Game1";
        private const decimal Game1Price = 19.99m;
        private const string Game1Description = "Desc1";
        private const string Game1Image = "Image1";
        private const string Game1MinimumRequirement = "Min1";
        private const string Game1RecommendedRequirement = "Rec1";
        private const string Game1Status = "Available";
        private const decimal Game1Discount = 10.0m;
        private const decimal Game1Rating = 4.5m;
        private const int Game2Identifier = 2;

        private const string Game2Name = "Game2";
        private const decimal Game2Price = 29.99m;
        private const string Game2Description = "Desc2";
        private const string Game2Image = "Image2";
        private const string Game2MinimumRequirement = "Min2";
        private const string Game2RecommendedRequirement = "Rec2";
        private const string Game2Status = "Available";
        private const decimal Game2Discount = 15.0m;
        private const decimal Game2Rating = 4.0m;
        private const int ExpectedCountGamesWishlist = 2;

        public UserGameRepositoryTest()
        {
            mockDataLink = new Mock<IDataLink>();
            mockUser = new User { UserIdentifier = TestUserId, WalletBalance = InitialWalletBalance, PointsBalance = InitialPointsBalance };
            userGameRepository = new UserGameRepository(mockDataLink.Object, mockUser);
        }

        [Fact]
        public void IsGamePurchased_ReturnsTrue_WhenGameIsPurchased()
        {
            var purchasedGame = new Game { Identifier = TestGameId };
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int>(SqlConstants.IsGamePurchasedProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(1);

            var isPurchased = userGameRepository.IsGamePurchased(purchasedGame);

            Assert.True(isPurchased);
        }

        [Fact]
        public void IsGamePurchased_ReturnsFalse_WhenGameIsNotPurchased()
        {
            var unpurchasedGame = new Game { Identifier = TestGameId };
            mockDataLink.Setup(dataLink => dataLink.ExecuteScalar<int>(SqlConstants.IsGamePurchasedProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(0);

            var isPurchased = userGameRepository.IsGamePurchased(unpurchasedGame);

            Assert.False(isPurchased);
        }

        [Fact]
        public void RemoveGameFromWishlist_CallsExecuteNonQuery_WhenGameIsValid()
        {
            var gameToRemove = new Game { Identifier = TestGameId };
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.RemoveGameFromWishlistProcedure, It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.RemoveGameFromWishlist(gameToRemove);

            mockDataLink.Verify(dataLink => dataLink.ExecuteNonQuery(SqlConstants.RemoveGameFromWishlistProcedure, It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void RemoveGameFromWishlist_ThrowsException_WhenDatabaseErrorOccurs()
        {
            var gameToRemove = new Game { Identifier = TestGameId };
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.RemoveGameFromWishlistProcedure, It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception(ExceptionMessageDatabaseError));

            var exception = Assert.Throws<Exception>(() => userGameRepository.RemoveGameFromWishlist(gameToRemove));
            Assert.Equal(ExceptionMessageDatabaseError, exception.Message);
        }

        [Fact]
        public void AddGameToPurchased_ThrowsException_WhenFundsAreInsufficient()
        {
            var expensiveGame = new Game { Identifier = TestGameId, Price = TestGamePriceExpensive };
            var exception = Assert.Throws<Exception>(() => userGameRepository.AddGameToPurchased(expensiveGame));

            Assert.Equal(ExceptionMessageInsufficientFunds, exception.Message);
        }

        [Fact]
        public void AddGameToPurchased_UpdatesWalletBalance_WhenPurchaseIsSuccessful()
        {
            var affordableGame = new Game { Identifier = TestGameId, Price = TestGamePriceCheap };
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.AddGameToPurchasedGamesProcedure, It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.AddGameToPurchased(affordableGame);

            Assert.Equal(TestGameCheckPrice, mockUser.WalletBalance);
            mockDataLink.Verify(dataLink => dataLink.ExecuteNonQuery(SqlConstants.AddGameToPurchasedGamesProcedure, It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_CallsExecuteNonQuery_WhenGameIsValid()
        {
            var gameToAdd = new Game { Identifier = TestGameId };
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.AddGameToWishlistProcedure, It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.AddGameToWishlist(gameToAdd);

            mockDataLink.Verify(dataLink => dataLink.ExecuteNonQuery(SqlConstants.AddGameToWishlistProcedure, It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_ThrowsException_WhenDatabaseFails()
        {
            var gameToAdd = new Game { Identifier = TestGameId };
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.AddGameToWishlistProcedure, It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception(ExceptionMessageDatabaseError));

            var exception = Assert.Throws<Exception>(() => userGameRepository.AddGameToWishlist(gameToAdd));
            Assert.Equal(ExceptionMessageDatabaseError, exception.Message);
        }

        [Fact]
        public void GetGameTags_ReturnsTagList_WhenTagsAreAvailable()
        {
            var tagsTable = new DataTable();
            tagsTable.Columns.Add(SqlConstants.TagNameColumn);
            tagsTable.Rows.Add(TestTag1);
            tagsTable.Rows.Add(TestTag2);

            mockDataLink.Setup(dataLink => dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(tagsTable);

            var tags = userGameRepository.GetGameTags(TestGameId);

            Assert.Equal(GetNumberGameTagsExpected, tags.Length);
            Assert.Contains(TestTag1, tags);
            Assert.Contains(TestTag2, tags);
        }

        [Fact]
        public void GetGameTags_ThrowsException_WhenDatabaseFails()
        {
            mockDataLink.Setup(dataLink => dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception(ExceptionMessageDatabaseError));

            var exception = Assert.Throws<Exception>(() => userGameRepository.GetGameTags(TestGameId));
            Assert.Equal($"Error getting tags for game {TestGameId}: {ExceptionMessageDatabaseError}", exception.Message);
        }

        [Fact]
        public void GetGameOwnerCount_ReturnsCorrectCount_WhenDataExists()
        {
            var ownerCountTable = new DataTable();
            ownerCountTable.Columns.Add(OwnerCountParameter);
            ownerCountTable.Rows.Add(OwnerCountValue);

            mockDataLink.Setup(dataLink => dataLink.ExecuteReader(SqlConstants.GetGameOwnerCountProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(ownerCountTable);

            var result = userGameRepository.GetGameOwnerCount(TestGameId);

            Assert.Equal(OwnerCountValue, result);
        }

        [Fact]
        public void GetGameOwnerCount_ReturnsZero_WhenNoDataAvailable()
        {
            var emptyTable = new DataTable();

            mockDataLink.Setup(dataLink => dataLink.ExecuteReader(SqlConstants.GetGameOwnerCountProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(emptyTable);

            var result = userGameRepository.GetGameOwnerCount(TestGameId);

            Assert.Equal(OwnerCountMinimum, result);
        }

        [Fact]
        public void AddPointsForPurchase_IncreasesUserPoints_WhenValidAmountIsProvided()
        {
            mockUser.PointsBalance = PointBalanceMinimum;
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.UpdateUserPointBalance, It.IsAny<SqlParameter[]>()))
                        .Verifiable();

            userGameRepository.AddPointsForPurchase(TestPurchaseAmount);

            Assert.Equal(TestRewardPoints, mockUser.PointsBalance);
            mockDataLink.Verify(dataLink => dataLink.ExecuteNonQuery(SqlConstants.UpdateUserPointBalance, It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void AddPointsForPurchase_ThrowsException_WhenDatabaseFails()
        {
            mockDataLink.Setup(dataLink => dataLink.ExecuteNonQuery(SqlConstants.UpdateUserPointBalance, It.IsAny<SqlParameter[]>()))
                        .Throws(new Exception(ExceptionMessageDatabaseError));

            var exception = Assert.Throws<Exception>(() => userGameRepository.AddPointsForPurchase(TestPurchaseAmount));

            Assert.Equal(ExceptionMessageFailAddPoints, exception.Message);
        }

        [Fact]
        public void GetUserPointsBalance_ReturnsCorrectBalance()
        {
            mockUser.PointsBalance = InitialPointsBalance;

            var result = userGameRepository.GetUserPointsBalance();

            Assert.Equal(InitialPointsBalance, result);
        }

        [Fact]
        public void GetWishlistGames_ReturnsListOfGames_WhenDataExists()
        {
            var wishlistTable = new DataTable();
            wishlistTable.Columns.Add(SqlConstants.GameIdColumn, typeof(int));
            wishlistTable.Columns.Add(SqlConstants.GameNameColumn, typeof(string));
            wishlistTable.Columns.Add(SqlConstants.GamePriceColumn, typeof(decimal));
            wishlistTable.Columns.Add(SqlConstants.DescriptionIdColumnWithCapitalLetter, typeof(string));
            wishlistTable.Columns.Add(SqlConstants.ImageUrlColumn, typeof(string));
            wishlistTable.Columns.Add(SqlConstants.MinimumRequirementsColumn, typeof(string));
            wishlistTable.Columns.Add(SqlConstants.RecommendedRequirementsColumn, typeof(string));
            wishlistTable.Columns.Add(SqlConstants.GameStatusColumn, typeof(string));
            wishlistTable.Columns.Add(SqlConstants.DiscountColumn, typeof(decimal));
            wishlistTable.Columns.Add(SqlConstants.RatingColumn, typeof(decimal));
            wishlistTable.Rows.Add(Game1Identifier, Game1Name, Game1Price, Game1Description, Game1Image, Game1MinimumRequirement, Game1RecommendedRequirement, Game1Status, Game1Discount, Game1Rating);
            wishlistTable.Rows.Add(Game2Identifier, Game2Name, Game2Price, Game2Description, Game2Image, Game2MinimumRequirement, Game2RecommendedRequirement, Game2Status, Game2Discount, Game2Rating);


            mockDataLink.Setup(dataLink => dataLink.ExecuteReader(SqlConstants.GetWishlistGamesProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(wishlistTable);

            var wishlist = userGameRepository.GetWishlistGames();

            Assert.Equal(ExpectedCountGamesWishlist, wishlist.Count);
        }

        [Fact]
        public void GetWishlistGames_ReturnsEmptyList_WhenNoDataExists()
        {
            var emptyWishlist = new DataTable();

            mockDataLink.Setup(dataLink => dataLink.ExecuteReader(SqlConstants.GetWishlistGamesProcedure, It.IsAny<SqlParameter[]>()))
                        .Returns(emptyWishlist);

            var wishlist = userGameRepository.GetWishlistGames();

            Assert.Empty(wishlist);
        }
    }
}
