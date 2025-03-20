using System;
using System.Collections.ObjectModel;
using System.Linq;

public class DeveloperViewModel
{
    public ObservableCollection<Game> DeveloperGames { get; set; }
    public DeveloperService _developerService;
    public UserGameService _userGameService;

    public DeveloperViewModel(DeveloperService developerService, UserGameService userGameService)
    {
        _developerService = developerService;
        _userGameService = userGameService;
        DeveloperGames = new ObservableCollection<Game>();
        LoadGames();
    }
    public void LoadGames()
    {
        var games = _developerService.GetDeveloperGames();
        foreach (var game in games)
        {
            DeveloperGames.Add(game);
        }
    }
    public void ValidateGame(int game_id, bool isValid)
    {
        _developerService.ValidateGame(game_id, isValid);
    }
    public void CreateGame(Game game)
    {
        _developerService.CreateGame(game);
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
    }
    public void LoadUnvalidated()
    {
        DeveloperGames.Clear();
        var games = _developerService.GetUnvalidated();
        foreach (var game in games)
        {
            DeveloperGames.Add(game);
        }
    }


}