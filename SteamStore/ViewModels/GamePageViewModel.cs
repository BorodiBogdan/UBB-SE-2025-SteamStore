using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using SteamStore.Pages;
using SteamStore.Services.Interfaces;

public class GamePageViewModel : INotifyPropertyChanged
{
    internal readonly ICartService _cartService;
    internal readonly IUserGameService _userGameService;
    internal readonly IGameService _gameService;

    private Game _game;
    private ObservableCollection<Game> _similarGames;
    private bool _isOwned;
    private ObservableCollection<string> _gameTags;

    private const int MaxSimilarGamesToDisplay = 3;

    public Game Game
    {
        get => _game;
        set
        {
            _game = value;
            OnPropertyChanged();
            UpdateIsOwnedStatus();
            UpdateGameTags();
        }
    }
    
    public ObservableCollection<string> GameTags
    {
        get => _gameTags;
        private set
        {
            _gameTags = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsOwned
    {
        get => _isOwned;
        private set
        {
            if (_isOwned != value)
            {
                _isOwned = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<Game> SimilarGames
    {
        get => _similarGames;
        set
        {
            _similarGames = value;
            OnPropertyChanged();
        }
    }
    
    public GamePageViewModel(IGameService gameService, ICartService cartService, IUserGameService userGameService)
    {
        _cartService = cartService;
        _userGameService = userGameService;
        _gameService = gameService; 
        SimilarGames = new ObservableCollection<Game>();
        GameTags = new ObservableCollection<string>();
    }
    
    public void LoadGame(Game game)
    {
        Game = game;
        LoadSimilarGames();
    }
    
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
    
    private void UpdateIsOwnedStatus()
    {
        if (Game == null || _userGameService == null)
        {
            IsOwned = false;
            return;
        }

        try
        {
            IsOwned = _userGameService.isGamePurchased(Game);
        }
        catch (Exception)
        {
            IsOwned = false;
        }
    }
    
    private void UpdateGameTags()
    {
        if (Game == null || _gameService == null)
        {
            GameTags.Clear();
            return;
        }

        try
        {
            var tags = _gameService.getAllGameTags(Game);
            
            GameTags.Clear();
            foreach (var tag in tags)
            {
                GameTags.Add(tag.tag_name);
            }
        }
        catch (Exception ex)
        {
            GameTags.Clear();
        }
    }
    
    // Load similar games based on current game
    private void LoadSimilarGames()
    {
        if (Game == null || _gameService == null) return;
        var similarGames = _gameService.GetSimilarGames(Game.Id);
        SimilarGames = new ObservableCollection<Game>(similarGames.Take(MaxSimilarGamesToDisplay));
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
    
    public event PropertyChangedEventHandler PropertyChanged;
    public void GetSimilarGames(Game game,Frame frame)
    {
        if (frame != null)
        {
            var gamePage = new GamePage(_gameService, _cartService,_userGameService, game);

            frame.Content = gamePage;

        }
        else
        {
            frame.Navigate(typeof(GamePage), game);
        }

    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}