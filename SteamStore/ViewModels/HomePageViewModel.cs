using System.Collections.ObjectModel;

public class HomePageViewModel
{
    public ObservableCollection<Game> Games { get; set; }

    private readonly GameRepository _gameRepository;

    public HomePageViewModel(GameRepository gameRepository)
    {
        _gameRepository = gameRepository;
        Games = new ObservableCollection<Game>();
        LoadGames();
    }

    private void LoadGames()
    {
        var games = _gameRepository.getAllGames();
        foreach (var game in games)
        {
            Games.Add(game);
        }
    }
}