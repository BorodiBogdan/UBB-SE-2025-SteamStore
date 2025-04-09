using Moq;
using SteamStore.Repositories.Interfaces;

namespace SteamStore.Tests.Services;

public class CartServiceTests
{
	private readonly CartService _cartService;
	private readonly Mock<ICartRepository> _repositoryMock;

	public CartServiceTests()
	{
		_repositoryMock = new Mock<ICartRepository>();
		_cartService = new CartService(_repositoryMock.Object);
	}

	[Fact]
	public void GetCartGames_ShouldCallRepository()
	{
		var mockedRepositoryData = new List<Game>();

		_repositoryMock.Setup(repository => repository.GetCartGames())
			.Returns(mockedRepositoryData)
			.Verifiable();

		_cartService.GetCartGames();

		_repositoryMock.Verify(repository => repository.GetCartGames());
	}

	[Fact]
	public void GetCartGames_ShouldReturnData()
	{
		var games = new List<Game>();

		_repositoryMock.Setup(repository => repository.GetCartGames())
			.Returns(games)
			.Verifiable();

		var actualGames = _cartService.GetCartGames();

		Assert.Same(games, actualGames);
	}

	[Fact]
	public void RemoveGameFromCart_ShouldCallRepository()
	{
		var game = new Game
		{
			Identifier = 1
		};

		_repositoryMock.Setup(repository => repository.RemoveGameFromCart(It.IsAny<Game>()))
			.Verifiable();

		_cartService.RemoveGameFromCart(game);

		_repositoryMock.Verify(f => f.RemoveGameFromCart(game));
	}

	[Fact]
	public void AddGameToCart_ShouldCallRepository()
	{
		var game = new Game
		{
			Identifier = 1
		};

		_repositoryMock.Setup(repository => repository.AddGameToCart(It.IsAny<Game>()))
			.Verifiable();

		_cartService.AddGameToCart(game);

		_repositoryMock.Verify(f => f.AddGameToCart(game));
	}

	[Fact]
	public void RemoveGamesFromCart_ShouldCallRepositoryForEach()
	{
		var games = new List<Game>
		{
			new Game
			{
				Identifier = 1
			},
			new Game
			{
				Identifier = 2
			}
		};

		_repositoryMock.Setup(repository => repository.RemoveGameFromCart(It.IsAny<Game>()))
			.Verifiable();

		_cartService.RemoveGamesFromCart(games);

		_repositoryMock.Verify(f => f.RemoveGameFromCart(games[0]));
		_repositoryMock.Verify(f => f.RemoveGameFromCart(games[1]));
	}

	[Fact]
	public void GetUserFunds_ShouldCallRepository()
	{
		_repositoryMock.Setup(repository => repository.GetUserFunds())
			.Verifiable();

		_cartService.GetUserFunds();

		_repositoryMock.Verify(f => f.GetUserFunds());
	}

	[Fact]
	public void GetTotalSumToBePaid_ShouldCallRepository()
	{
		var games = new List<Game>
		{
			new Game
			{
				Identifier = 1,
				Price = 10
			},
			new Game
			{
				Identifier = 2,
				Price = 20
			}
		};
		_repositoryMock.Setup(repository => repository.GetCartGames())
			.Returns(games)
			.Verifiable();

		_cartService.GetTotalSumToBePaid();

		_repositoryMock.Verify(f => f.GetCartGames());
	}

	[Fact]
	public void GetTotalSumToBePaid_ShouldReturnProperSumToBePaid()
	{
		var games = new List<Game>
		{
			new Game
			{
				Identifier = 1,
				Price = 10
			},
			new Game
			{
				Identifier = 2,
				Price = 20
			}
		};
		var expectedTotal = 30;

		_repositoryMock.Setup(repository => repository.GetCartGames())
			.Returns(games);

		var result = _cartService.GetTotalSumToBePaid();

		Assert.Equal(expectedTotal, result);
	}
}
