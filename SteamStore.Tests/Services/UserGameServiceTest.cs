﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SteamStore.Constants;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using Windows.UI.WebUI;

namespace SteamStore.Tests.Services
{
    public class UserGameServiceTest
    {
        private readonly Mock<IUserGameRepository> mockUserGameRepository;
        private readonly Mock<IGameRepository> mockGameRepository;
        private readonly Mock<ITagRepository> mockTagRepository;
        private readonly UserGameService userGameService;

        // Constants to avoid magic strings and numbers
        private const string GAME_NAME_1 = "Game1";
        private const string GAME_NAME_2 = "Game2";
        private const string GAME_NAME_3 = "Game3";
        private const string TAG_NAME_1 = "RPG";
        private const string TAG_NAME_2 = "FPS";
        private const string TAG_NAME_3 = "MMG";
        private const string TAG_NAME_4 = "BLP";
        private const int USER_POINTS_BEFORE_PURCHASE = 10;
        private const int USER_POINTS_AFTER_PURCHASE = 15;
        private const decimal EXPECTED_TAG_SCORE = 1m;
        private const decimal RATING_HIGH = 4.7m;
        private const decimal RATING_MEDIUM = 4.3m;
        private const decimal RATING_LOW = 2.5m;
        private const decimal RATING_POOR = 1.5m;
        private const decimal DISCOUNT_HIGH = 50;
        private const decimal DISCOUNT_LOW = 10;
        private const int PRICE_HIGH = 20;
        private const int PRICE_LOW = 10;
        private const int RECENT_PURCHASES_GAME_1 = 5;
        private const int RECENT_PURCHASES_GAME_2 = 10;

        public UserGameServiceTest()
        {
            mockUserGameRepository = new Mock<IUserGameRepository>();
            mockGameRepository = new Mock<IGameRepository>();
            mockTagRepository = new Mock<ITagRepository>();

            userGameService = new UserGameService
            {
                UserGameRepository = mockUserGameRepository.Object,
                GameRepository = mockGameRepository.Object,
                TagRepository = mockTagRepository.Object
            };
        }

        [Fact]
        public void RemoveGameFromWishlist_CallsRepository()
        {
            var gameToRemove = new Game();
            userGameService.RemoveGameFromWishlist(gameToRemove);
            mockUserGameRepository.Verify(repository => repository.RemoveGameFromWishlist(gameToRemove), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_WhenAlreadyOwned_ThrowsException()
        {
            var gameToAdd = new Game { Name = GAME_NAME_1 };
            mockUserGameRepository.Setup(repository => repository.IsGamePurchased(gameToAdd)).Returns(true);

            var exception = Assert.Throws<Exception>(() => userGameService.AddGameToWishlist(gameToAdd));
            Assert.Equal(string.Format(ExceptionMessages.GameAlreadyOwned, GAME_NAME_1), exception.Message);
        }

        [Fact]
        public void AddGameToWishlist_WhenNotOwned_CallsRepository()
        {
            var gameToAdd = new Game { Name = GAME_NAME_2 };
            mockUserGameRepository.Setup(repository => repository.IsGamePurchased(gameToAdd)).Returns(false);

            userGameService.AddGameToWishlist(gameToAdd);
            mockUserGameRepository.Verify(repository => repository.AddGameToWishlist(gameToAdd), Times.Once);
        }

        [Fact]
        public void AddGameToWishlist_WhenSqlException_FormatsMessage()
        {
            var gameToAdd = new Game { Name = GAME_NAME_3 };
            mockUserGameRepository.Setup(repository => repository.IsGamePurchased(gameToAdd)).Returns(false);
            mockUserGameRepository.Setup(repository => repository.AddGameToWishlist(gameToAdd)).Throws(new Exception("ExecuteNonQuery failed"));

            var exception = Assert.Throws<Exception>(() => userGameService.AddGameToWishlist(gameToAdd));
            Assert.Equal(string.Format(ExceptionMessages.GameAlreadyInWishlist, GAME_NAME_3), exception.Message);
        }

        [Fact]
        public void PurchaseGames_CallsRepository()
        {
            var gameToPurchase = new Game { Name = GAME_NAME_1 };
            var gamesToPurchase = new List<Game> { gameToPurchase };

            mockUserGameRepository.SetupSequence(repository => repository.GetUserPointsBalance())
                                  .Returns(USER_POINTS_BEFORE_PURCHASE)
                                  .Returns(USER_POINTS_AFTER_PURCHASE);

            userGameService.PurchaseGames(gamesToPurchase);

            mockUserGameRepository.Verify(repository => repository.AddGameToPurchased(gameToPurchase), Times.Once);
            mockUserGameRepository.Verify(repository => repository.RemoveGameFromWishlist(gameToPurchase), Times.Once);
        }

        [Fact]
        public void PurchaseGames_CalculatesPointsCorrectly()
        {
            var gameToPurchase = new Game { Name = GAME_NAME_1 };
            var gamesToPurchase = new List<Game> { gameToPurchase };

            mockUserGameRepository.SetupSequence(repository => repository.GetUserPointsBalance())
                                  .Returns(USER_POINTS_BEFORE_PURCHASE)
                                  .Returns(USER_POINTS_AFTER_PURCHASE);

            userGameService.PurchaseGames(gamesToPurchase);

            Assert.Equal(USER_POINTS_AFTER_PURCHASE - USER_POINTS_BEFORE_PURCHASE, userGameService.LastEarnedPoints);
        }

        [Fact]
        public void ComputeNoOfUserGamesForEachTag_ShouldSetCountToOne_WhenTagIsUsedInOneGame()
        {
            var tag = new Tag { Tag_name = TAG_NAME_1 };
            var gameWithTag = new Game { Name = GAME_NAME_1, Tags = new[] { TAG_NAME_1 } };

            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(new Collection<Game> { gameWithTag });

            var tagCollection = new Collection<Tag> { tag };

            userGameService.ComputeNoOfUserGamesForEachTag(tagCollection);

            Assert.Equal(1, tag.NumberOfUserGamesWithTag);
        }

        [Fact]
        public void ComputeNoOfUserGamesForEachTag_ShouldSetCountToZero_WhenTagNotPresentInAnyGame()
        {
            var tag = new Tag { Tag_name = TAG_NAME_2 };
            var gameWithoutTag = new Game { Name = GAME_NAME_1, Tags = new[] { TAG_NAME_1 } };

            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(new Collection<Game> { gameWithoutTag });

            var tagCollection = new Collection<Tag> { tag };

            userGameService.ComputeNoOfUserGamesForEachTag(tagCollection);

            Assert.Equal(0, tag.NumberOfUserGamesWithTag);
        }

        [Fact]
        public void ComputeNoOfUserGamesForEachTag_ShouldCorrectlyCountMultipleGamesForSameTag()
        {
            var tag = new Tag { Tag_name = TAG_NAME_1 };
            var game1 = new Game { Name = GAME_NAME_1, Tags = new[] { TAG_NAME_1 } };
            var game2 = new Game { Name = GAME_NAME_2, Tags = new[] { TAG_NAME_1, TAG_NAME_2 } };

            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(new Collection<Game> { game1, game2 });

            var tagCollection = new Collection<Tag> { tag };

            userGameService.ComputeNoOfUserGamesForEachTag(tagCollection);

            Assert.Equal(2, tag.NumberOfUserGamesWithTag);
        }

        [Fact]
        public void ComputeNoOfUserGamesForEachTag_ShouldHandleMultipleTagsAndGames()
        {
            var tag1 = new Tag { Tag_name = TAG_NAME_1 };
            var tag2 = new Tag { Tag_name = TAG_NAME_2 };

            var multiTagGame = new Game { Name = GAME_NAME_1, Tags = new[] { TAG_NAME_1, TAG_NAME_2 } };

            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(new Collection<Game> { multiTagGame });

            var tagCollection = new Collection<Tag> { tag1, tag2 };

            userGameService.ComputeNoOfUserGamesForEachTag(tagCollection);

            Assert.Equal(1, tag1.NumberOfUserGamesWithTag);
            Assert.Equal(1, tag2.NumberOfUserGamesWithTag);
        }

        [Fact]
        public void GetFavoriteUserTags_ReturnsTop3_VerifyCount()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 },
                new Tag { Tag_name = TAG_NAME_3 }
            };

            var userGames = new Collection<Game>
            {
                new Game { Tags = new[] { TAG_NAME_2, TAG_NAME_3 } },
                new Game { Tags = new[] { TAG_NAME_1, TAG_NAME_2 } }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(userGames);

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Equal(3, favoriteTags.Count);
        }

        [Fact]
        public void GetFavoriteUserTags_ReturnsTop3_VerifyTag()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 },
                new Tag { Tag_name = TAG_NAME_3 }
            };

            var userGames = new Collection<Game>
            {
                new Game { Tags = new[] { TAG_NAME_2, TAG_NAME_3 } },
                new Game { Tags = new[] { TAG_NAME_1, TAG_NAME_2 } }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(userGames);

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Equal(TAG_NAME_2, favoriteTags[0].Tag_name);
        }

        [Fact]
        public void GetFavoriteUserTags_ReturnsEmpty_WhenNoGames()
        {
            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(new Collection<Tag>());
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(new Collection<Game>());

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Empty(favoriteTags);
        }

        [Fact]
        public void GetFavoriteUserTags_ReturnsOnlyExistingTags_VerifyCount()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 }
            };

            var userGames = new Collection<Game>
            {
                new Game { Tags = new[] { TAG_NAME_1 } },
                new Game { Tags = new[] { TAG_NAME_1 } },
                new Game { Tags = new[] { TAG_NAME_2 } }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(userGames);

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Equal(2, favoriteTags.Count);
        }

        [Fact]
        public void GetFavoriteUserTags_ReturnsOnlyExistingTags_VerifyTag()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 }
            };

            var userGames = new Collection<Game>
            {
                new Game { Tags = new[] { TAG_NAME_1 } },
                new Game { Tags = new[] { TAG_NAME_1 } },
                new Game { Tags = new[] { TAG_NAME_2 } }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(userGames);

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Equal(TAG_NAME_1, favoriteTags[0].Tag_name);
        }

        [Fact]
        public void GetFavoriteUserTags_IgnoresTagsNotInList()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 }
            };

            var userGames = new Collection<Game>
            {
                new Game { Tags = new[] { TAG_NAME_1, TAG_NAME_2, TAG_NAME_3 } }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(userGames);

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Single(favoriteTags);
            Assert.Equal(TAG_NAME_1, favoriteTags[0].Tag_name);
        }

        [Fact]
        public void GetFavoriteUserTags_ReturnsTop3_WhenMoreTags()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 },
                new Tag { Tag_name = TAG_NAME_3 },
                new Tag { Tag_name = TAG_NAME_4 }
            };

            var userGames = new Collection<Game>
            {
                new Game { Tags = new[] { TAG_NAME_1, TAG_NAME_2, TAG_NAME_3 } },
                new Game { Tags = new[] { TAG_NAME_2, TAG_NAME_4 } },
                new Game { Tags = new[] { TAG_NAME_4 } }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(userGames);

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Equal(3, favoriteTags.Count);
        }

        [Fact]
        public void ComputeTagScoreForGames_CalculatesProperly()
        {
            var gameToScore = new Game { Tags = new[] { TAG_NAME_1, TAG_NAME_2 } };

            var allUserGames = new Collection<Game>
            {
                gameToScore,
                new Game { Tags = new[] { TAG_NAME_2 } },
                new Game { Tags = new[] { TAG_NAME_3 } }
            };

            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 },
                new Tag { Tag_name = TAG_NAME_3 }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(allUserGames);

            userGameService.ComputeTagScoreForGames(allUserGames);

            Assert.True(Math.Abs(EXPECTED_TAG_SCORE - gameToScore.TagScore) < 0.0001m);
        }

        [Fact]
        public void ComputeTagScoreForGames_SetsZero_WhenNoMatchingTags()
        {
            var gameWithoutValidTags = new Game { Tags = new[] { TAG_NAME_4 } };

            var allUserGames = new Collection<Game> { gameWithoutValidTags };

            var knownTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 },
                new Tag { Tag_name = TAG_NAME_3 }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(knownTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(allUserGames);

            userGameService.ComputeTagScoreForGames(allUserGames);

            Assert.Equal(0, gameWithoutValidTags.TagScore);
        }

        [Fact]
        public void ComputeTagScoreForGames_CalculatesCorrectly_WithUnevenTagCounts()
        {
            var gameToEvaluate = new Game { Tags = new[] { TAG_NAME_2 } };

            var allUserGames = new Collection<Game>
            {
                gameToEvaluate,
                new Game { Tags = new[] { TAG_NAME_2 } },
                new Game { Tags = new[] { TAG_NAME_2 } },
                new Game { Tags = new[] { TAG_NAME_1 } }
            };

            var knownTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1 },
                new Tag { Tag_name = TAG_NAME_2 }
            };

            mockTagRepository.Setup(repo => repo.GetAllTags()).Returns(knownTags);
            mockUserGameRepository.Setup(repo => repo.GetAllUserGames()).Returns(allUserGames);

            userGameService.ComputeTagScoreForGames(allUserGames);

            Assert.True(gameToEvaluate.TagScore > 0);
        }

        [Fact]
        public void ComputeTrendingScores_SetsTrendingScore()
        {
            var trendingGames = new Collection<Game>
        {
            new Game { Name = GAME_NAME_1, NumberOfRecentPurchases = RECENT_PURCHASES_GAME_1 },
            new Game { Name = GAME_NAME_2, NumberOfRecentPurchases = RECENT_PURCHASES_GAME_2 }
        };

            userGameService.ComputeTrendingScores(trendingGames);

            Assert.Equal(0.5m, trendingGames[0].TrendingScore);
            Assert.Equal(1.0m, trendingGames[1].TrendingScore);
        }

        [Fact]
        public void ComputeTrendingScores_HandlesSingleGame()
        {
            var singleGame = new Collection<Game>
        {
            new Game { Name = GAME_NAME_1, NumberOfRecentPurchases = RECENT_PURCHASES_GAME_1 }
        };

            userGameService.ComputeTrendingScores(singleGame);

            Assert.Equal(1.0m, singleGame[0].TrendingScore);
        }

        [Fact]
        public void GetRecommendedGames_ReturnsTop10()
        {
            var recommendedGames = new Collection<Game>
            {
                new Game { NumberOfRecentPurchases = RECENT_PURCHASES_GAME_1, Tags = new[] { TAG_NAME_1 } },
                new Game { NumberOfRecentPurchases = RECENT_PURCHASES_GAME_2, Tags = new[] { TAG_NAME_1 } }
            };

            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1, NumberOfUserGamesWithTag = 2 }
            };

            mockGameRepository.Setup(repository => repository.GetAllGames()).Returns(recommendedGames);
            mockTagRepository.Setup(repository => repository.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(repository => repository.GetAllUserGames()).Returns(recommendedGames);

            var recommendedResult = userGameService.GetRecommendedGames();

            Assert.Equal(2, recommendedResult.Count);
        }

        [Fact]
        public void GetRecommendedGames_ReturnsTop10_WhenMoreThan10Games()
        {
            var allTags = new Collection<Tag>
            {
                new Tag { Tag_name = TAG_NAME_1, NumberOfUserGamesWithTag = 15 }
            };

            var allGames = Enumerable.Range(1, 15).Select(i => new Game
            {
                Name = $"Game{i}",
                NumberOfRecentPurchases = i,
                Tags = new[] { TAG_NAME_1 }
            }).ToList();

            mockGameRepository.Setup(r => r.GetAllGames()).Returns(new Collection<Game>(allGames));
            mockTagRepository.Setup(r => r.GetAllTags()).Returns(allTags);
            mockUserGameRepository.Setup(r => r.GetAllUserGames()).Returns(new Collection<Game>(allGames));

            var recommendedGames = userGameService.GetRecommendedGames();

            Assert.Equal(10, recommendedGames.Count);
        }

        [Fact]
        public void SearchWishListByName_ReturnsMatches_SpecificWord()
        {
            var wishlistGames = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1 },
                new Game { Name = GAME_NAME_2 }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(wishlistGames);

            var matchedGames = userGameService.SearchWishListByName(GAME_NAME_1);
            Assert.Single(matchedGames);
            Assert.Equal(GAME_NAME_1, matchedGames[0].Name);
        }

        [Fact]
        public void SearchWishListByName_ReturnsMatches_AllWords()
        {
            var wishlistGames = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1 },
                new Game { Name = GAME_NAME_2 }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(wishlistGames);

            var matchedGames = userGameService.SearchWishListByName("Game");
            Assert.Equal(wishlistGames, matchedGames);
        }

        [Fact]
        public void FilterWishListGames_FiltersByOverwhelminglyPositive()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Rating = RATING_HIGH },
                new Game { Rating = RATING_MEDIUM },
                new Game { Rating = RATING_LOW },
                new Game { Rating = RATING_POOR }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var filteredGames = userGameService.FilterWishListGames(FilterCriteria.OVERWHELMINGLYPOSITIVE);
            Assert.Single(filteredGames);
            Assert.Equal(RATING_HIGH, filteredGames[0].Rating);
        }

        [Fact]
        public void FilterWishListGames_FiltersByVeryPositive()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Rating = RATING_HIGH },
                new Game { Rating = RATING_MEDIUM },
                new Game { Rating = RATING_LOW },
                new Game { Rating = RATING_POOR }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var filteredGames = userGameService.FilterWishListGames(FilterCriteria.VERYPOSITIVE);
            Assert.Single(filteredGames);
            Assert.Equal(RATING_MEDIUM, filteredGames[0].Rating);
        }

        [Fact]
        public void FilterWishListGames_FiltersByMixed()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Rating = RATING_HIGH },
                new Game { Rating = RATING_MEDIUM },
                new Game { Rating = RATING_LOW },
                new Game { Rating = RATING_POOR }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var filteredGames = userGameService.FilterWishListGames(FilterCriteria.MIXED);
            Assert.Single(filteredGames);
            Assert.Equal(RATING_LOW, filteredGames[0].Rating);
        }

        [Fact]
        public void FilterWishListGames_FiltersByNegative()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Rating = RATING_HIGH },
                new Game { Rating = RATING_MEDIUM },
                new Game { Rating = RATING_LOW },
                new Game { Rating = RATING_POOR }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var filteredGames = userGameService.FilterWishListGames(FilterCriteria.NEGATIVE);
            Assert.Single(filteredGames);
            Assert.Equal(RATING_POOR, filteredGames[0].Rating);
        }

        [Fact]
        public void IsGamePurchased_DelegatesToRepository()
        {
            var game = new Game();
            mockUserGameRepository.Setup(repository => repository.IsGamePurchased(game)).Returns(true);

            Assert.True(userGameService.IsGamePurchased(game));
        }

        [Fact]
        public void SortWishListGames_SortsByRatingAscending()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1, Rating = RATING_MEDIUM },
                new Game { Name = GAME_NAME_2, Rating = RATING_HIGH }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var sortedGames = userGameService.SortWishListGames(FilterCriteria.RATING, true);
            Assert.Equal(RATING_MEDIUM, sortedGames[0].Rating);
            Assert.Equal(RATING_HIGH, sortedGames[1].Rating);
        }

        [Fact]
        public void SortWishListGames_SortsByRatingDescending()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1, Rating = RATING_MEDIUM },
                new Game { Name = GAME_NAME_2, Rating = RATING_HIGH }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var sortedGames = userGameService.SortWishListGames(FilterCriteria.RATING, false);
            Assert.Equal(RATING_HIGH, sortedGames[0].Rating);
            Assert.Equal(RATING_MEDIUM, sortedGames[1].Rating);
        }

        [Fact]
        public void SortWishListGames_SortsByPriceAscending()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1, Price = PRICE_HIGH },
                new Game { Name = GAME_NAME_2, Price = PRICE_LOW }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var sortedGames = userGameService.SortWishListGames(FilterCriteria.PRICE, true);
            Assert.Equal(PRICE_LOW, sortedGames[0].Price);
            Assert.Equal(PRICE_HIGH, sortedGames[1].Price);
        }

        [Fact]
        public void SortWishListGames_SortsByPriceDescending()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1, Price = PRICE_HIGH },
                new Game { Name = GAME_NAME_2, Price = PRICE_LOW }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var sortedGames = userGameService.SortWishListGames(FilterCriteria.PRICE, false);
            Assert.Equal(PRICE_HIGH, sortedGames[0].Price);
            Assert.Equal(PRICE_LOW, sortedGames[1].Price);
        }

        [Fact]
        public void SortWishListGames_SortsByDiscountAscending()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1, Discount = DISCOUNT_HIGH },
                new Game { Name = GAME_NAME_2, Discount = DISCOUNT_LOW }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var sortedGames = userGameService.SortWishListGames(FilterCriteria.DISCOUNT, true);
            Assert.Equal(DISCOUNT_LOW, sortedGames[0].Discount);
            Assert.Equal(DISCOUNT_HIGH, sortedGames[1].Discount);
        }

        [Fact]
        public void SortWishListGames_SortsByDiscountDescending()
        {
            var gamesList = new Collection<Game>
            {
                new Game { Name = GAME_NAME_1, Discount = DISCOUNT_HIGH },
                new Game { Name = GAME_NAME_2, Discount = DISCOUNT_LOW }
            };

            mockUserGameRepository.Setup(repository => repository.GetWishlistGames()).Returns(gamesList);

            var sortedGames = userGameService.SortWishListGames(FilterCriteria.DISCOUNT, false);
            Assert.Equal(DISCOUNT_HIGH, sortedGames[0].Discount);
            Assert.Equal(DISCOUNT_LOW, sortedGames[1].Discount);
        }

        [Fact]
        public void GetFavoriteUserTags_WhenNoTags_ReturnsEmptyList()
        {
            mockTagRepository.Setup(repository => repository.GetAllTags()).Returns(new Collection<Tag>());
            mockUserGameRepository.Setup(repository => repository.GetAllUserGames()).Returns(new Collection<Game>());

            var favoriteTags = userGameService.GetFavoriteUserTags();

            Assert.Empty(favoriteTags);
        }
    }
}
