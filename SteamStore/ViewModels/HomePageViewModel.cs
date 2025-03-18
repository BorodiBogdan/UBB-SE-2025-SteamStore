using System.Collections.ObjectModel;

public class HomePageViewModel
{
    public ObservableCollection<Game> allGames { get; set; }

    private readonly GameService _gameService;

    public HomePageViewModel(GameService gameService)
    {
        _gameService = gameService;
        allGames = new ObservableCollection<Game>();
        LoadGames();
    }

    private void LoadGames()
    {
        var games = _gameService.getAllGames();
        foreach (var game in games)
        {
            allGames.Add(game);
        }
    }
}