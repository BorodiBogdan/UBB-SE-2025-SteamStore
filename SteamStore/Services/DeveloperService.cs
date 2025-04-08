using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;
using SteamStore.Constants;
using static SteamStore.Constants.NotificationStrings;

public class DeveloperService : IDeveloperService
{
    private static string PENDING_STATE = "Pending";

    public IGameRepository GameRepository{ get; set; }
    public ITagRepository TagRepository { get; set; }
    
    public IUserGameRepository UserGameRepository { get; set; }
    
    public User User { get; set; }
    
    public void ValidateGame(int game_id)
    {
        GameRepository.ValidateGame(game_id);
    }
    public Game ValidateInputForAddingAGame(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl,string gameplayUrl,
                                string minReq, string recReq, string discountText, IList<Tag> selectedTags)
    {
        if (string.IsNullOrWhiteSpace(gameIdText) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText) ||
            string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(imageUrl) || string.IsNullOrWhiteSpace(minReq) ||
            string.IsNullOrWhiteSpace(recReq) || string.IsNullOrWhiteSpace(discountText))
        {
            throw new Exception(ExceptionMessages.AllFieldsRequired);
        }

        if (!int.TryParse(gameIdText, out int gameId))
        {
            throw new Exception(ExceptionMessages.InvalidGameId);
            
        }

        if (!decimal.TryParse(priceText, out var price) || price < 0)
        {
            throw new Exception(ExceptionMessages.InvalidPrice);
        }

        if (!decimal.TryParse(discountText, out var discount) || discount < 0 || discount > 100)
        {
            throw new Exception(ExceptionMessages.InvalidDiscount);
        }

        if (selectedTags == null || selectedTags.Count == 0)
        {
            throw new Exception(ExceptionMessages.NoTagsSelected);
        }

       // var game = new Game(gameId, name,price, description, imageUrl, gameplayUrl, trailerUrl, minReq, recReq, "Pending", discount);
        var game = new Game
        {
            Identifier = gameId,
            Name = name,
            Price = price,
            Description = description,
            ImagePath = imageUrl,
            GameplayPath = gameplayUrl,
            TrailerPath = trailerUrl,
            MinimumRequirements = minReq,
            RecommendedRequirements = recReq,
            Status = PENDING_STATE,
            Discount = discount
        };
        return game;
    }

    public Game FindGameInObservableCollectionById(int gameId, ObservableCollection<Game> gameList)
    {
        foreach (Game game in gameList)
        {
            if (game.Identifier == gameId)
            {
                return game;
            }
        }

        return null; 
    }


    

    public void CreateGame(Game game)
    {
        game.PublisherIdentifier = User.UserIdentifier;
        GameRepository.CreateGame(game);
    }

    public void CreateGameWithTags(Game game, IList<Tag> selectedTags)
    {   try
        {
            CreateGame(game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        if (selectedTags != null && selectedTags.Count > 0)
        {
            foreach (var tag in selectedTags)
            {
                InsertGameTag(game.Identifier, tag.TagId);
            }
        }
    }
    public void UpdateGame(Game game)
    {
        try
        {
            game.PublisherIdentifier = User.UserIdentifier;
            GameRepository.UpdateGame(game.Identifier,game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

    }

    public void UpdateGameWithTags(Game game, IList<Tag> selectedTags)
    {
        try
        {
            game.PublisherIdentifier = User.UserIdentifier;
            GameRepository.UpdateGame(game.Identifier, game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        try
        {
            //System.Diagnostics.Debug.WriteLine("deleting the tags!");
            DeleteGameTags(game.Identifier);
            if (selectedTags != null && selectedTags.Count > 0)
                foreach (var tag in selectedTags)
                    InsertGameTag(game.Identifier, tag.TagId);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }
    public void DeleteGame(int game_id)
    {
        try
        {
            GameRepository.DeleteGame(game_id);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public List<Game> GetDeveloperGames()
    {
        return GameRepository.GetDeveloperGames(User.UserIdentifier);
    }
    public List<Game> GetUnvalidated()
    {
        return GameRepository.GetUnvalidated(User.UserIdentifier);
    }
    public void RejectGame(int game_id)
    {
        GameRepository.RejectGame(game_id);
    }
    public void RejectGameWithMessage(int game_id, string message)
    {
        GameRepository.RejectGameWithMessage(game_id, message);
    }
    public string GetRejectionMessage(int game_id)
    {
        return GameRepository.GetRejectionMessage(game_id);
    }
    public void InsertGameTag(int gameId, int tagId)
    {
        GameRepository.InsertGameTag(gameId, tagId);
    }

   
    public Collection<Tag> GetAllTags()
    {
        return TagRepository.GetAllTags();
    }
    public bool IsGameIdInUse(int gameId)
    {
        return GameRepository.IsGameIdInUse(gameId);
    }
    public List<Tag> GetGameTags(int gameId)
    {
        return GameRepository.GetGameTags(gameId);
    }
    public void DeleteGameTags(int gameId)
    {
        GameRepository.DeleteGameTags(gameId);
    }
    public int GetGameOwnerCount(int gameId)
    {
        return UserGameRepository.GetGameOwnerCount(gameId);
    }
    public User GetCurrentUser()
    {
        return User;
    }
    public Game CreateValidatedGame(string gameIdText, string name, string priceText, string description, string imageUrl,
                                string trailerUrl, string gameplayUrl, string minReq, string recReq,
                                string discountText, IList<Tag> selectedTags)
    {
        var game = ValidateInputForAddingAGame(gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl, minReq, recReq, discountText, selectedTags);

        if (IsGameIdInUse(game.Identifier))
            throw new Exception(ExceptionMessages.IdAlreadyInUse);

        CreateGameWithTags(game, selectedTags);
        return game;
    }


}
