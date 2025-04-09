using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Tests.TestUtils;

namespace SteamStore.Tests.Repositories;

public class GameRepositoryTest
{
    private readonly GameRepository _subject = new(DataLinkTestUtils.GetDataLink());


    [Fact]
    public void CreateGame()
    {
        var testGame = CreateRandomGame();
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherIdentifier)
            .FirstOrDefault(game => game.Name == testGame.Name);
        AssertUtils.AssertAllPropertiesEqual(testGame, foundGame);
    }

    [Fact]
    public void UpdateGame()
    {
        var insertedGame = CreateRandomGame();
        var updatedGame = GameTestUtils.CreateRandomGame();
        updatedGame.Rating = 0m;
        updatedGame.Identifier = insertedGame.Identifier;
        _subject.UpdateGame(updatedGame.Identifier, updatedGame);
        var foundGame = _subject.GetDeveloperGames(updatedGame.PublisherIdentifier)
            .FirstOrDefault(game => game.Name == updatedGame.Name);
        AssertUtils.AssertAllPropertiesEqual(updatedGame, foundGame);
    }

    [Fact]
    public void ValidateGame()
    {
        var testGame = CreateRandomGame("Pending");
        _subject.ValidateGame(testGame.Identifier);
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherIdentifier)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Equal("Approved", foundGame!.Status);
    }

    [Fact]
    public void RejectGame()
    {
        var testGame = CreateRandomGame("Pending");
        _subject.RejectGame(testGame.Identifier);
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherIdentifier)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Equal("Rejected", foundGame!.Status);
        Assert.Equal(string.Empty, _subject.GetRejectionMessage(testGame.Identifier));
    }

    [Fact]
    public void RejectGameWithMessage()
    {
        var testGame = CreateRandomGame("Pending");
        _subject.RejectGameWithMessage(testGame.Identifier, "TEST");
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherIdentifier)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Equal("Rejected", foundGame!.Status);
        Assert.Equal("TEST", _subject.GetRejectionMessage(testGame.Identifier));
        
    }

    [Fact]
    public void GetAllGames()
    {
        var testGame = CreateRandomGame("Approved");
        var tags = CreateRandomTagsForGame(testGame);

        var foundGame = _subject.GetAllGames().FirstOrDefault(game => game.Name == testGame.Name);
        testGame.Tags = tags.Select(tag => tag.Tag_name).ToArray();
        testGame.TrendingScore = Game.NOTCOMPUTED;
        testGame.TagScore = Game.NOTCOMPUTED;
        AssertUtils.AssertAllPropertiesEqual(testGame, foundGame);
    }
    
    [Fact]
    public void GetUnvalidated()
    {
        var testGame = CreateRandomGame("Pending");
        var foundGame = _subject.GetUnvalidated(2).FirstOrDefault(game => game.Name == testGame.Name);

        AssertUtils.AssertAllPropertiesEqual(testGame, foundGame);
    }    
    
    [Fact]
    public void DeleteGame()
    {
        
        var testGame = CreateRandomGame("Approved");
        CreateRandomTagsForGame(testGame);
        //TODO: also attach reviews, transactions, libraries

        _subject.DeleteGame(testGame.Identifier);
        var notFound = _subject.GetDeveloperGames(testGame.PublisherIdentifier)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Null(notFound);
    }

    [Fact]
    public void IsGameIdUsed()
    {
        var testGame = CreateRandomGame();
        Assert.True(_subject.IsGameIdInUse(testGame.Identifier));
        
    }

    private Tag[] CreateRandomTagsForGame(Game testGame)
    {
        var tags = GameTestUtils.RandomTags();
        foreach (var tag in tags)
        {
            _subject.InsertGameTag(testGame.Identifier, tag.TagId);
        }

        return tags;
    }


    private Game CreateRandomGame(string? status = null)
    {
        var testGame = GameTestUtils.CreateRandomGame();
        testGame.Rating = 0m;
        if (status != null)
        {
            testGame.Status = status;
        }

        _subject.CreateGame(testGame);
        return testGame;
    }
}