using Moq;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services;
using System.Collections.ObjectModel;
using Xunit;

namespace SteamStore.Tests.Services
{
	public class DeveloperServiceTests
	{
		private readonly DeveloperService _service;
		private readonly Mock<IGameRepository> _gameRepositoryMock = new();
		private readonly Mock<ITagRepository> _tagRepositoryMock = new();
		private readonly Mock<IUserGameRepository> _userGameRepoMock = new();

		private readonly User _testUser = new() { UserIdentifier = 42 };

		public DeveloperServiceTests()
		{
			_service = new DeveloperService
			{
				GameRepository = _gameRepositoryMock.Object,
				TagRepository = _tagRepositoryMock.Object,
				UserGameRepository = _userGameRepoMock.Object,
				User = _testUser
			};
		}

		[Fact]
		public void ValidateGame_ShouldCallRepository()
		{
			_service.ValidateGame(1);
			_gameRepositoryMock.Verify(repo => repo.ValidateGame(1), Times.Once);
		}

		[Fact]
		public void ValidateInputForAddingAGame_ShouldReturnGame_WhenValid()
		{
			var gameIdText = "1";
			var name = "Test";
			var priceText = "10";
			var description = "Desc";
			var imageUrl = "img.png";
			var tralerUrl = "trailer";
			var gameplayUrl = "gameplay";
			var minimumRequirement = "min";
			var recommendedRequirement = "rec";
			var dicountText = "5";
			var tags = new List<Tag> { new Tag { TagId = 1 } };

			var expectedIdentifier = 1;
			var expectedName = "Test";
			var expectedPrice = 10;
			var expectedStatus = "Pending";

			var game = _service.ValidateInputForAddingAGame(gameIdText, name, priceText, description, imageUrl, tralerUrl, gameplayUrl, minimumRequirement, recommendedRequirement, dicountText, tags);

			Assert.NotNull(game);
			Assert.Equal(expectedIdentifier, game.Identifier);
			Assert.Equal(expectedName, game.Name);
			Assert.Equal(expectedPrice, game.Price);
			Assert.Equal(expectedStatus, game.Status);
		}

		[Theory]
		[InlineData("", "name", "10", "desc", "img", "trailer", "gameplay", "min", "rec", "5")]
		[InlineData("1", "", "10", "desc", "img", "trailer", "gameplay", "min", "rec", "5")]
		[InlineData("abc", "name", "10", "desc", "img", "trailer", "gameplay", "min", "rec", "5")]
		[InlineData("1", "name", "-10", "desc", "img", "trailer", "gameplay", "min", "rec", "5")]
		[InlineData("1", "name", "10", "desc", "img", "trailer", "gameplay", "min", "rec", "abc")]
		[InlineData("1", "name", "10", "desc", "img", "trailer", "gameplay", "min", "rec", "150")]
		public void ValidateInputForAddingAGame_ShouldThrow_WhenInvalid(
			string id, string name, string price, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string recommendedRequirement, string discount)
		{
			Assert.Throws<Exception>(() =>
				_service.ValidateInputForAddingAGame(id, name, price, description, imageUrl, trailerUrl, gameplayUrl, minimumRequirement, recommendedRequirement, discount, new List<Tag> { new() }));
		}

		[Fact]
		public void FindGameInObservableCollectionById_ShouldReturnGame()
		{
			var list = new ObservableCollection<Game> { new Game { Identifier = 1 } };

			var searchedIdentifier = 1;

			var result = _service.FindGameInObservableCollectionById(searchedIdentifier, list);

			Assert.NotNull(result);
		}

		[Fact]
		public void CreateGame_ShouldCallRepositoryWithUserId()
		{
			var game = new Game { Identifier = 1 };

			_service.CreateGame(game);

			Assert.Equal(_testUser.UserIdentifier, game.PublisherIdentifier);
			_gameRepositoryMock.Verify(r => r.CreateGame(game), Times.Once);
		}

		[Fact]
		public void CreateGameWithTags_ShouldCallCreateAndInsert()
		{
			var game = new Game { Identifier = 1 };
			var tags = new List<Tag> { new() { TagId = 2 } };

			var expectedGameIdentifier = 1;
			var expectedTagId = 2;

			_service.CreateGameWithTags(game, tags);

			_gameRepositoryMock.Verify(r => r.CreateGame(game), Times.Once);
			_gameRepositoryMock.Verify(r => r.InsertGameTag(expectedGameIdentifier, expectedTagId), Times.Once);
		}

		[Fact]
		public void UpdateGame_ShouldCallUpdateWithUserId()
		{
			var game = new Game { Identifier = 1 };

			var expectedGameIdentifier = 1;

			_service.UpdateGame(game);
			_gameRepositoryMock.Verify(r => r.UpdateGame(expectedGameIdentifier, game), Times.Once);
		}

		[Fact]
		public void UpdateGameWithTags_ShouldCallUpdateAndDeleteTagsAndInsert()
		{
			var game = new Game { Identifier = 1 };
			var tags = new List<Tag> { new() { TagId = 2 } };

			var expectedGameIdentifier = 1;
			var expectedTagIdentifier = 2;

			_service.UpdateGameWithTags(game, tags);

			_gameRepositoryMock.Verify(r => r.UpdateGame(expectedGameIdentifier, game), Times.Once);
			_gameRepositoryMock.Verify(r => r.DeleteGameTags(expectedGameIdentifier), Times.Once);
			_gameRepositoryMock.Verify(r => r.InsertGameTag(expectedGameIdentifier, expectedTagIdentifier), Times.Once);
		}

		[Fact]
		public void DeleteGame_ShouldCallRepo()
		{
			var expectedGameIdentifier = 1;

			_service.DeleteGame(expectedGameIdentifier);
			_gameRepositoryMock.Verify(r => r.DeleteGame(expectedGameIdentifier), Times.Once);
		}

		[Fact]
		public void GetDeveloperGames_ShouldReturnGames()
		{
			_gameRepositoryMock.Setup(r => r.GetDeveloperGames(_testUser.UserIdentifier)).Returns(new List<Game>());

			var result = _service.GetDeveloperGames();

			Assert.NotNull(result);
		}

		[Fact]
		public void GetUnvalidated_ShouldReturnList()
		{
			_gameRepositoryMock.Setup(r => r.GetUnvalidated(_testUser.UserIdentifier)).Returns(new List<Game>());

			var result = _service.GetUnvalidated();

			Assert.NotNull(result);
		}

		[Fact]
		public void RejectGame_ShouldCallRepo()
		{
			var expectedGameIdentifier = 1;

			_service.RejectGame(expectedGameIdentifier);

			_gameRepositoryMock.Verify(r => r.RejectGame(expectedGameIdentifier));
		}

		[Fact]
		public void RejectGameWithMessage_ShouldCallRepo()
		{
			var expectedGameIdentifier = 1;
			var expectedMessage = "msg";

			_service.RejectGameWithMessage(expectedGameIdentifier, expectedMessage);

			_gameRepositoryMock.Verify(r => r.RejectGameWithMessage(expectedGameIdentifier, expectedMessage));
		}

		[Fact]
		public void GetRejectionMessage_ShouldCallRepo()
		{
			var expectedGameIdentifier = 1;
			var expectedMessage = "msg";

			_gameRepositoryMock.Setup(r => r.GetRejectionMessage(expectedGameIdentifier)).Returns(expectedMessage);

			var actualMessage = _service.GetRejectionMessage(expectedGameIdentifier);

			Assert.Equal(expectedMessage, actualMessage);
		}

		[Fact]
		public void InsertGameTag_ShouldCallRepo()
		{
			var expectedGameIdentifier = 1;
			var expectedTagIdentifier = 2;

			_service.InsertGameTag(expectedGameIdentifier, expectedTagIdentifier);

			_gameRepositoryMock.Verify(r => r.InsertGameTag(expectedGameIdentifier, expectedTagIdentifier));
		}

		[Fact]
		public void GetAllTags_ShouldReturnTags()
		{
			_tagRepositoryMock.Setup(r => r.GetAllTags()).Returns(new Collection<Tag>());

			var tags = _service.GetAllTags();

			Assert.NotNull(tags);
		}

		[Fact]
		public void IsGameIdInUse_ShouldReturnTrue()
		{
			var expectedGameIdentifier = 1;

			_gameRepositoryMock.Setup(r => r.IsGameIdInUse(expectedGameIdentifier)).Returns(true);

			var result = _service.IsGameIdInUse(expectedGameIdentifier);

			Assert.True(result);
		}

		[Fact]
		public void GetGameTags_ShouldReturnList()
		{
			var expectedGameIdentifier = 1;

			_gameRepositoryMock.Setup(r => r.GetGameTags(expectedGameIdentifier)).Returns(new List<Tag>());

			var result = _service.GetGameTags(expectedGameIdentifier);

			Assert.NotNull(result);
		}

		[Fact]
		public void DeleteGameTags_ShouldCallRepo()
		{
			var expectedGameIdentifier = 1;

			_service.DeleteGameTags(expectedGameIdentifier);

			_gameRepositoryMock.Verify(r => r.DeleteGameTags(expectedGameIdentifier));
		}

		[Fact]
		public void GetGameOwnerCount_ShouldReturnCount()
		{
			var expectedGameIdentifier = 1;
			var expectedGameOwnerCount = 7;

			_userGameRepoMock.Setup(r => r.GetGameOwnerCount(expectedGameIdentifier)).Returns(expectedGameOwnerCount);

			var result = _service.GetGameOwnerCount(expectedGameIdentifier);

			Assert.Equal(7, result);
		}

		[Fact]
		public void GetCurrentUser_ShouldReturnUser()
		{
			var user = _service.GetCurrentUser();

			Assert.Equal(_testUser, user);
		}

		[Fact]
		public void CreateValidatedGame_ShouldThrowIfIdInUse()
		{
			var gameIdText = "1";
			var name = "Test";
			var priceText = "10";
			var description = "Desc";
			var imageUrl = "img.png";
			var tralerUrl = "trailer";
			var gameplayUrl = "gameplay";
			var minimumRequirement = "min";
			var recommendedRequirement = "rec";
			var dicountText = "0";
			var tags = new List<Tag> { new Tag { TagId = 1 } };

			var expectedIdentifier = 1;

			_gameRepositoryMock.Setup(r => r.IsGameIdInUse(expectedIdentifier)).Returns(true);
			Assert.Throws<Exception>(() =>
				_service.CreateValidatedGame(gameIdText, name, priceText, description, imageUrl, tralerUrl, gameplayUrl, minimumRequirement, recommendedRequirement, dicountText, tags));
		}

		[Fact]
		public void DeleteGame_ShouldRemoveFromCollection()
		{
			var gameList = new ObservableCollection<Game> { new() { Identifier = 1 } };
			var expectedIdentifier = 1;

			_service.DeleteGame(expectedIdentifier, gameList);

			Assert.Empty(gameList);
		}

		[Fact]
		public void UpdateGameAndRefreshList_ShouldUpdateCorrectly()
		{
			var existing = new Game { Identifier = 1 };
			var updated = new Game { Identifier = 1, Name = "Updated" };
			var games = new ObservableCollection<Game> { existing };

			_service.UpdateGameAndRefreshList(updated, games);

			Assert.Single(games);
			Assert.Contains(updated, games);
		}

		[Fact]
		public void RejectGameAndRemoveFromUnvalidated_ShouldWork()
		{
			var games = new ObservableCollection<Game> { new Game { Identifier = 1 } };
			var expectedIdentifier = 1;

			_service.RejectGameAndRemoveFromUnvalidated(expectedIdentifier, games);

			Assert.Empty(games);
		}

		[Fact]
		public void IsGameIdInUse_WithCollections_ShouldShortCircuit()
		{
			var devGames = new ObservableCollection<Game> { new Game { Identifier = 1 } };
			var unvalidated = new ObservableCollection<Game> { new Game { Identifier = 2 } };

			var expectedFirstIdentifier = 1;
			var expectedSecondIdentifier = 2;
			var expectedThirdIdentifier = 3;

			Assert.True(_service.IsGameIdInUse(expectedFirstIdentifier, devGames, unvalidated));
			Assert.True(_service.IsGameIdInUse(expectedSecondIdentifier, devGames, unvalidated));
			Assert.False(_service.IsGameIdInUse(expectedThirdIdentifier, devGames, unvalidated));
		}

		[Fact]
		public void GetMatchingTagsForGame_ShouldReturnMatched()
		{
			var allTags = new List<Tag> { new() { TagId = 1 }, new() { TagId = 2 } };
			var gameTags = new List<Tag> { new() { TagId = 1 } };
			var expectedIdentifier = 1;
			var expectedTagIdentifier = 1;

			_gameRepositoryMock.Setup(r => r.GetGameTags(expectedIdentifier)).Returns(gameTags);
			var result = _service.GetMatchingTagsForGame(expectedIdentifier, allTags);

			Assert.Single(result);
			Assert.Equal(expectedTagIdentifier, result[0].TagId);
		}
	}
}
