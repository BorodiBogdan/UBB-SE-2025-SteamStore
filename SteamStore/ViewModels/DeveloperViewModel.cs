using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SteamStore.Models;

public class DeveloperViewModel
{
    public ObservableCollection<Game> DeveloperGames { get; set; }
    public ObservableCollection<Game> UnvalidatedGames {  get; set; }
    public ObservableCollection<Tag> Tags { get; set; }

    public DeveloperService _developerService;
    public UserGameService _userGameService;

    public DeveloperViewModel(DeveloperService developerService, UserGameService userGameService)
    {
        _developerService = developerService;
        _userGameService = userGameService;
        DeveloperGames = new ObservableCollection<Game>();
        UnvalidatedGames = new ObservableCollection<Game>();
        Tags = new ObservableCollection<Tag>();
        LoadGames();
        LoadTags();
    }

    private void LoadTags()
    {
        Tags.Clear();
        var tags = _developerService.GetAllTags();
        foreach (var tag in tags)
        {
            Tags.Add(tag);
        }
    }

    public void LoadGames()
    {
        DeveloperGames.Clear();
        var games = _developerService.GetDeveloperGames();
        foreach (var game in games)
        {
            DeveloperGames.Add(game);
        }
    }

    public void ValidateGame(int game_id)
    {
        _developerService.ValidateGame(game_id);
    }

    public void CreateGame(Game game, IList<Tag> selectedTags)
    {
        _developerService.CreateGame(game);
        
        if (selectedTags != null && selectedTags.Count > 0)
        {
            foreach (var tag in selectedTags)
            {
                _developerService.InsertGameTag(game.Id, tag.tag_id);
            }
        }
        
        DeveloperGames.Add(game);
    }

    public void UpdateGame(Game game)
    {
        DeveloperGames.Remove(DeveloperGames.FirstOrDefault(g => g.Id == game.Id));
        _developerService.UpdateGame(game);
        DeveloperGames.Add(game);
    }

    public void DeleteGame(int game_id)
    {
        var game = DeveloperGames.FirstOrDefault(g => g.Id == game_id);
        DeveloperGames.Remove(game);
        _developerService.DeleteGame(game_id);
    }

    public void RejectGame(int game_id)
    {
        _developerService.RejectGame(game_id);
        var game = DeveloperGames.FirstOrDefault(x => x.Id == game_id);
        UnvalidatedGames.Remove(game);
    }


    public void LoadUnvalidated()
    {
        UnvalidatedGames.Clear();
        var games = _developerService.GetUnvalidated();
        foreach (var game in games)
        {
            UnvalidatedGames.Add(game);
        }
    }

    public bool IsGameIdInUse(int gameId)
    {
        // Check in the developer's own games first
        if (DeveloperGames.Any(g => g.Id == gameId))
        {
            return true;
        }
        
        // Check in unvalidated games
        if (UnvalidatedGames.Any(g => g.Id == gameId))
        {
            return true;
        }
        
        return _developerService.IsGameIdInUse(gameId);
    }

    public int GetGameOwnerCount(int game_id)
    {
        return _developerService.GetGameOwnerCount(game_id);
    }
}