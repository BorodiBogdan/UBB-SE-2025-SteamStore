using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

public class GamePageViewModel : INotifyPropertyChanged
{
    // Make services accessible to the view
    internal readonly CartService _cartService;
    internal readonly UserGameService _userGameService;
    internal readonly GameService _gameService;

    private Game _game;
    private ObservableCollection<Game> _similarGames;
    
    // Game properties
    public Game Game
    {
        get => _game;
        set
        {
            _game = value;
            OnPropertyChanged();
        }
    }
    
    // Similar games collection
    public ObservableCollection<Game> SimilarGames
    {
        get => _similarGames;
        set
        {
            _similarGames = value;
            OnPropertyChanged();
        }
    }
    
    // Constructor - inject services
    public GamePageViewModel(GameService gameService, CartService cartService, UserGameService userGameService)
    {
        _cartService = cartService;
        _userGameService = userGameService;
        _gameService = gameService; 
        SimilarGames = new ObservableCollection<Game>();
    }
    
    // Load game and related data
    public void LoadGame(Game game)
    {
        Game = game;
        LoadSimilarGames();
    }
    
    // Load game by ID
    public void LoadGameById(int gameId)
    {
        if (_gameService == null) return;
        
        var allGames = _gameService.getAllGames();
        Game = allGames.FirstOrDefault(g => g.Id == gameId);
        
        if (Game != null)
        {
            LoadSimilarGames();
        }
    }
    
    // Load similar games based on current game
    private void LoadSimilarGames()
    {
        if (Game == null || _gameService == null) return;
        
        var allGames = _gameService.getAllGames();
        
        // In a real implementation, we would filter by genre
        // For now we'll just exclude the current game
        var similarGames = allGames
            .Where(g => g.Id != Game.Id)
            .OrderBy(g => Guid.NewGuid()) // Random order for demonstration
            .Take(3) // Take exactly 3 as required
            .ToList();
        
        SimilarGames.Clear();
        foreach (var game in similarGames)
        {
            SimilarGames.Add(game);
        }
    }
    
    // Add game to cart - safely handle null CartService
    public void AddToCart()
    {
        if (Game != null && _cartService != null)
        {
            try
            {
                _cartService.AddGameToCart(Game);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
    
    // Add game to wishlist - this will be implemented later
    public void AddToWishlist()
    {
        if(Game != null && _userGameService != null)
        {
            try
            {
                _userGameService.addGameToWishlist(Game);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
    
    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}