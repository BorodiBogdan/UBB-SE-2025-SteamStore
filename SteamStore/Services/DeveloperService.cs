using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamStore.Models;

public class DeveloperService
{
    private DeveloperRepository _developerRepository;
    public DeveloperService(DeveloperRepository developerRepository)
    {
        _developerRepository = developerRepository;
    }
    public void ValidateGame(int game_id)
    {
        _developerRepository.ValidateGame(game_id);
    }
    public Game ValidateInputForAddingAGame(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl,string gameplayUrl,
                                string minReq, string recReq, string discountText, IList<Tag> selectedTags)
    {
        if (string.IsNullOrWhiteSpace(gameIdText) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText) ||
            string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(imageUrl) || string.IsNullOrWhiteSpace(minReq) ||
            string.IsNullOrWhiteSpace(recReq) || string.IsNullOrWhiteSpace(discountText))
        {
            throw new Exception("All fields Are required!");
        }

        if (!int.TryParse(gameIdText, out int gameId))
        {
            throw new Exception("GameId must be a valid integer!");
            
        }

        if (!double.TryParse(priceText, out double price) || price < 0)
        {
            throw new Exception("Price must be a positive number.");
        }

        if (!float.TryParse(discountText, out float discount) || discount < 0 || discount > 100)
        {
            throw new Exception("Discount must be between 0 and 100.");
        }

        if (selectedTags == null || selectedTags.Count == 0)
        {
            throw new Exception("Please select at least one tag for the game.");
        }

       // var game = new Game(gameId, name,price, description, imageUrl, gameplayUrl, trailerUrl, minReq, recReq, "Pending", discount);
        var game = new Game
        {
            Id = gameId,
            Name = name,
            Price = price,
            Description = description,
            ImagePath = imageUrl,
            GameplayPath = gameplayUrl,
            TrailerPath = trailerUrl,
            MinimumRequirements = minReq,
            RecommendedRequirements = recReq,
            Status = "Pending",
            Discount = discount
        };
        return game;
    }

    public Game FindGameInObservableCollectionById(int gameId, ObservableCollection<Game> gameList)
    {
        foreach (Game game in gameList)
        {
            if (game.Id == gameId)
            {
                return game;
            }
        }

        return null; 
    }


    

    public void CreateGame(Game game)
    {
        try
        {
            _developerRepository.CreateGame(game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
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
                InsertGameTag(game.Id, tag.tag_id);
            }
        }
    }
    public void UpdateGame(Game game)
    {
        try
        {
            _developerRepository.UpdateGame(game.Id,game);
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
            _developerRepository.UpdateGame(game.Id, game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        try
        {
            //System.Diagnostics.Debug.WriteLine("deleting the tags!");
            DeleteGameTags(game.Id);
            if (selectedTags != null && selectedTags.Count > 0)
                foreach (var tag in selectedTags)
                    InsertGameTag(game.Id, tag.tag_id);
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
            _developerRepository.DeleteGame(game_id);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public List<Game> GetDeveloperGames()
    {
        return _developerRepository.GetDeveloperGames();
    }
    public List<Game> GetUnvalidated()
    {
        return _developerRepository.GetUnvalidated();
    }
    public void RejectGame(int game_id)
    {
        _developerRepository.RejectGame(game_id);
    }
    public void RejectGameWithMessage(int game_id, string message)
    {
        _developerRepository.RejectGameWithMessage(game_id, message);
    }
    public string GetRejectionMessage(int game_id)
    {
        return _developerRepository.GetRejectionMessage(game_id);
    }
    public void InsertGameTag(int gameId, int tagId)
    {
        _developerRepository.InsertGameTag(gameId, tagId);
    }

   
    public Collection<Tag> GetAllTags()
    {
        return _developerRepository.GetAllTags();
    }
    public bool IsGameIdInUse(int gameId)
    {
        return _developerRepository.IsGameIdInUse(gameId);
    }
    public List<Tag> GetGameTags(int gameId)
    {
        return _developerRepository.GetGameTags(gameId);
    }
    public void DeleteGameTags(int gameId)
    {
        _developerRepository.DeleteGameTags(gameId);
    }
    public int GetGameOwnerCount(int game_id)
    {
        return _developerRepository.GetGameOwnerCount(game_id);
    }
    public User GetCurrentUser()
    {
        return _developerRepository.GetCurrentUser();
    }
    public Game CreateValidatedGame(string gameIdText, string name, string priceText, string description, string imageUrl,
                                string trailerUrl, string gameplayUrl, string minReq, string recReq,
                                string discountText, IList<Tag> selectedTags)
    {
        var game = ValidateInputForAddingAGame(gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl, minReq, recReq, discountText, selectedTags);

        if (IsGameIdInUse(game.Id))
            throw new Exception("Game ID is already in use.");

        CreateGameWithTags(game, selectedTags);
        return game;
    }

}
