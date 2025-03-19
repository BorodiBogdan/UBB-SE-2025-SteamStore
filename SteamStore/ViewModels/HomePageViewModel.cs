using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class HomePageViewModel
{
    private ObservableCollection<Game> _allGames;
    public ObservableCollection<Game> searchedGames
    {
        get => _allGames;
        set
        {
            _allGames = value;
            OnPropertyChanged(); // Notify the view when the collection changes
        }
    }

    // Expose GameService so it can be accessed by the view
    public GameService GameService { get; private set; }

    private readonly GameService _gameService;

    public HomePageViewModel(GameService gameService)
    {
        _gameService = gameService;
        GameService = gameService; // Assign to public property
        searchedGames = new ObservableCollection<Game>();
        LoadGames();
    }

    private void LoadGames()
    {
        // Initialize the collection before adding items
        _allGames = new ObservableCollection<Game>();
        
        var games = _gameService.getAllGames();
        foreach (var game in games)
        {
            _allGames.Add(game);
        }
    }

    public void searchGames(string search_query)
    {
        searchedGames = new ObservableCollection<Game>(_gameService.searchGames(search_query));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}