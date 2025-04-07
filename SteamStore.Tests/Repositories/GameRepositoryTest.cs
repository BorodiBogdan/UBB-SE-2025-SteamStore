using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Tests.TestUtils;

namespace SteamStore.Tests.Repositories;

public class GameRepositoryTest
{
    private readonly GameRepository _subject = new(TestDataLink.GetDataLink());


    [Fact]
    public void CreateGame()
    {
        var testGame = CreateRandomGame();
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherId)
            .FirstOrDefault(game => game.Name == testGame.Name);
        AssertUtils.AssertAllPropertiesEqual(testGame, foundGame);
    }

    [Fact]
    public void UpdateGame()
    {
        var insertedGame = CreateRandomGame();
        var updatedGame = GameTestUtils.CreateRandomGame();
        updatedGame.Rating = 0m;
        updatedGame.Id = insertedGame.Id;
        _subject.UpdateGame(updatedGame.Id, updatedGame);
        var foundGame = _subject.GetDeveloperGames(updatedGame.PublisherId)
            .FirstOrDefault(game => game.Name == updatedGame.Name);
        AssertUtils.AssertAllPropertiesEqual(updatedGame, foundGame);
    }

    [Fact]
    public void ValidateGame()
    {
        var testGame = CreateRandomGame("Pending");
        _subject.ValidateGame(testGame.Id);
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherId)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Equal("Approved", foundGame!.Status);
    }

    [Fact]
    public void RejectGame()
    {
        var testGame = CreateRandomGame("Pending");
        _subject.RejectGame(testGame.Id);
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherId)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Equal("Rejected", foundGame!.Status);
        Assert.Equal(string.Empty, _subject.GetRejectionMessage(testGame.Id));
    }

    [Fact]
    public void RejectGameWithMessage()
    {
        var testGame = CreateRandomGame("Pending");
        _subject.RejectGameWithMessage(testGame.Id, "TEST");
        var foundGame = _subject.GetDeveloperGames(testGame.PublisherId)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Equal("Rejected", foundGame!.Status);
        Assert.Equal("TEST", _subject.GetRejectionMessage(testGame.Id));
        
    }

    [Fact]
    public void GetAllGames()
    {
        var testGame = CreateRandomGame("Approved");
        var tags = CreateRandomTagsForGame(testGame);

        var foundGame = _subject.GetAllGames().FirstOrDefault(game => game.Name == testGame.Name);
        testGame.Tags = tags.Select(tag => tag.tag_name).ToArray();
        testGame.trendingScore = Game.NOT_COMPUTED;
        testGame.tagScore = Game.NOT_COMPUTED;
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

        _subject.DeleteGame(testGame.Id);
        var notFound = _subject.GetDeveloperGames(testGame.PublisherId)
            .FirstOrDefault(game => game.Name == testGame.Name);

        Assert.Null(notFound);
    }

    [Fact]
    public void IsGameIdUsed()
    {
        var testGame = CreateRandomGame();
        Assert.True(_subject.IsGameIdInUse(testGame.Id));
        
    }

    private Tag[] CreateRandomTagsForGame(Game testGame)
    {
        var tags = GameTestUtils.RandomTags();
        foreach (var tag in tags)
        {
            _subject.InsertGameTag(testGame.Id, tag.tag_id);
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