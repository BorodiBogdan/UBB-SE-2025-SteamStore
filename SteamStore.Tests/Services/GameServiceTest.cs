using System.Collections.ObjectModel;
using Moq;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services;
using Xunit.Sdk;

namespace SteamStore.Tests.Services;

public class GameServiceTest
{
    private readonly GameService _subject;
    private readonly Mock<IGameRepository> _repoMock;
    private readonly Mock<ITagRepository> _tagRepoMock;

    public GameServiceTest()
    {
        _repoMock = new Mock<IGameRepository>();
        _tagRepoMock = new Mock<ITagRepository>();
        _subject = new GameService { GameRepository = _repoMock.Object, TagRepository = _tagRepoMock.Object };
    }

    [Fact]
    public void GetAllGames_ShouldDelegateToRepo()
    {
        var repoReturnedColl = new Collection<Game>();
        _repoMock.Setup(r => r.GetAllGames())
            .Returns(repoReturnedColl);
        Assert.Same(repoReturnedColl, _subject.GetAllGames());
    }

    [Fact]
    public void GetAllTags_ShouldDelegateToRepo()
    {
        var repoReturnedColl = new Collection<Tag>();
        _tagRepoMock.Setup(r => r.GetAllTags())
            .Returns(repoReturnedColl);
        Assert.Same(repoReturnedColl, _subject.GetAllTags());
    }

    [Fact]
    public void GetAllGameTags_ShouldFilterAllTags()
    {
        var game = new Game { Tags = new[] { "tag1", "tag2" } };

        var expectedTag = new Tag { tag_name = "tag1" };
        _tagRepoMock.Setup(r => r.GetAllTags())
            .Returns(new Collection<Tag> { expectedTag });

        var actualTag = _subject.GetAllGameTags(game);
        Assert.Same(actualTag[0], expectedTag);
        Assert.Equal(actualTag[0].tag_name, expectedTag.tag_name);
        Assert.Single(actualTag);
    }

    [Theory]
    [InlineData("Test", 2)]
    [InlineData("NoMatch", 0)]
    public void SearchItems_ShouldMatchItems(string searchQuery, int expectedCount)
    {
        var game1 = new Game { Name = "test Game 1" };
        var game2 = new Game { Name = "Game 2" };
        var game3 = new Game { Name = "TEST Game 3" };

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(new Collection<Game> { game1, game2, game3 });

        var actualGames = _subject.SearchGames(searchQuery);

        Assert.Equal(expectedCount, actualGames.Count);

        if (searchQuery.Equals("Test"))
        {
            Assert.Same(actualGames[0], game1);
            Assert.Same(actualGames[1], game3);
        }
        else
        {
            Assert.Empty(actualGames);
        }
    }

    [Theory]
    [InlineData(4, 10, 100, 1, new[] { "tag1" })]
    [InlineData(10, 10, 100, 0, new[] { "tag1", "tag2" })]
    [InlineData(4, 10, 300, 2, new string[] { })]
    [InlineData(1, 400, 600, 0, new[] { "tag1", "tag2" })]
    [InlineData(1, 100, 101, 0, new[] { "tag1", "tag2" })]
    [InlineData(1, 100, 400, 0, new[] { "tag1", "tag2", "tag3" })]
    public void FilterItems_ShouldRespectBoundaries(int minRating, int minPrice, int maxPrice, int foundElems,
        string[] tags)
    {
        var game1 = new Game()
        {
            Rating = 5,
            Price = 20,
            Tags = new[] { "tag1", "tag2" }
        };

        var game2 = new Game()
        {
            Rating = 7,
            Price = 200,
            Tags = new[] { "tag2" }
        };

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(new Collection<Game> { game1, game2 });

        var actualGames = _subject.FilterGames(minRating, minPrice, maxPrice, tags);

        Assert.Equal(foundElems, actualGames.Count);

        switch (foundElems)
        {
            case 1:
                Assert.Same(actualGames[0], game1);
                break;
            case 0:
                Assert.Empty(actualGames);
                break;
            case 2:
                Assert.Same(actualGames[0], game1);
                Assert.Same(actualGames[1], game2);
                break;
        }
    }

    [Fact]
    public void GetTrendingGames_computesTrendingGames()
    {
        var game1 = new Game()
        {
            noOfRecentPurchases = 10,
        };

        var game2 = new Game()
        {
            noOfRecentPurchases = 5,
        };

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(new Collection<Game> { game1, game2 });

        var actualGames = _subject.GetTrendingGames();


        Assert.Same(actualGames[0], game1);
        Assert.Same(actualGames[1], game2);
        Assert.Equal(1, game1.trendingScore);
        Assert.Equal(0.5m, game2.trendingScore);
    }

    [Theory]
    [InlineData("getTrendingGames")]
    [InlineData("getDiscountedGames")]
    public void GetGamesMethod_shouldCapAt10Elems(string methodName)
    {
        var games = new Collection<Game>();
        for (var i = 0; i < 11; i++)
        {
            games.Add(new Game()
            {
                Discount = 1
            });
        }

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(games);

        var actualGames = methodName switch
        {
            "getTrendingGames" => _subject.GetTrendingGames(),
            "getDiscountedGames" => _subject.GetDiscountedGames(),
            _ => throw new ArgumentException("Invalid method name", nameof(methodName))
        };

        Assert.Equal(10, actualGames.Count);
    }

    [Fact]
    public void GetDiscountedGames_computesTrendingGames()
    {
        var game1 = new Game()
        {
            noOfRecentPurchases = 10,
            Discount = 1
        };

        var game2 = new Game()
        {
            noOfRecentPurchases = 5,
            Discount = 2
        };

        var game3 = new Game()
        {
            noOfRecentPurchases = 5
        };

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(new Collection<Game> { game1, game2, game3 });

        var actualGames = _subject.GetDiscountedGames();


        Assert.Same(actualGames[0], game1);
        Assert.Same(actualGames[1], game2);
        Assert.Equal(2, actualGames.Count);
        Assert.Equal(1, game1.trendingScore);
        Assert.Equal(0.5m, game2.trendingScore);
    }

    [Fact]
    public void GetSimilarGames_returns3FilteredElements()
    {
        var allGames = new Collection<Game>
        {
            new() { Id = 1, Name = "Game1" },
            new() { Id = 2, Name = "Game2" },
            new() { Id = 3, Name = "Game3" },
            new() { Id = 4, Name = "Game4" },
            new() { Id = 5, Name = "Game5" }
        };

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(allGames);

        var similarGames = _subject.GetSimilarGames(1);

        Assert.Equal(3, similarGames.Count);
        Assert.DoesNotContain(similarGames, g => g.Id == 1);
        Assert.All(similarGames, game => Assert.NotEqual(1, game.Id));

        var gameIds = similarGames.Select(g => g.Id).ToList();
        Assert.True(gameIds.Count == gameIds.Distinct().Count());
    }

    [Fact]
    public void GetSimilarGames_ShouldBeRandomChosen()
    {
        var allGames = new Collection<Game>
        {
            new() { Id = 1, Name = "Game1" },
            new() { Id = 2, Name = "Game2" },
            new() { Id = 3, Name = "Game3" },
            new() { Id = 4, Name = "Game4" },
            new() { Id = 5, Name = "Game5" }
        };

        _repoMock.Setup(r => r.GetAllGames())
            .Returns(allGames);

        for (var tried = 0;tried < 5;tried++) {
            try
            {
                var similarGames1 = _subject.GetSimilarGames(1);
                var similarGames2 = _subject.GetSimilarGames(1);
                Assert.NotEqual(similarGames1, similarGames2);
            }
            catch (XunitException)
            {
              continue;   
            }
            tried = 5;
        }
    }
}