using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using SteamStore.Models;
using SteamStore.Pages;
using SteamStore.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class HomePageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Game> searchedOrFilteredGames{get;set;}
    public ObservableCollection<Game> trendingGames { get; set; }
    public ObservableCollection<Game> recommendedGames { get; set; }
    public ObservableCollection<Game> discountedGames { get; set;}
    public string _search_filter_text;
    public ObservableCollection<Tag> tags { get; set; }
    private readonly IGameService _gameService;
    private readonly IUserGameService _userGameService;
    private readonly ICartService _cartService;

    public string search_filter_text
    {
        get => _search_filter_text;
        set
        {
            if (_search_filter_text != value)
            {
                _search_filter_text = value;
                OnPropertyChanged();
            }
        }
    }

    // Expose GameService so it can be accessed by the view
    public IGameService GameService { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public HomePageViewModel(IGameService gameService, IUserGameService userGameService, ICartService cartService)
    {
        _gameService = gameService;
        _userGameService = userGameService;
        _cartService = cartService;
        GameService = gameService; // Assign to public property
        searchedOrFilteredGames = new ObservableCollection<Game>();
        trendingGames = new ObservableCollection<Game>();
        recommendedGames = new ObservableCollection<Game>();
        discountedGames = new ObservableCollection<Game>();
        LoadAllGames();
        LoadTrendingGames();
        LoadRecommendedGames();
        LoadDiscountedGames();
        tags = new ObservableCollection<Tag>();
        LoadTags();
    }

    
    private void LoadTrendingGames()
    {
        trendingGames.Clear();
        var games = _gameService.getTrendingGames();
        foreach (var game in games)
        {
            trendingGames.Add(game);
        }
    }
    private void LoadTags()
    {
        var tagsList = _gameService.getAllTags();
        foreach (var tag in tagsList)
        {
            tags.Add(tag);
        }
    }

    private void LoadRecommendedGames()
    {
        recommendedGames.Clear();
        var games = _userGameService.getRecommendedGames();
        foreach (var game in games)
        {
            recommendedGames.Add(game);
        }
    }
    
    private void LoadDiscountedGames()
    {
        discountedGames.Clear();
        var games = _gameService.getDiscountedGames();
        foreach (var game in games)
        {
            discountedGames.Add(game);
        }
    }



    public void LoadAllGames()
    {
        searchedOrFilteredGames.Clear();
        search_filter_text = "All Games";
        var games = _gameService.getAllGames();
        foreach (var game in games)
        {
            searchedOrFilteredGames.Add(game);
        }
    }

    public void SearchGames(string search_query)
    {
        searchedOrFilteredGames.Clear();
        var games = _gameService.searchGames(search_query);
        foreach (var game in games)
        {
            searchedOrFilteredGames.Add(game);
        }
        if(search_query == "")
        {
            search_filter_text = "All Games";
            return;
        }
        if(games.Count == 0)
        {
            search_filter_text = "No games found for search: " + search_query;
            return;
        }
        search_filter_text = "Search results for: " + search_query;
    }

    public void FilterGames(int minRating, int minPrice, int maxPrice, String[] Tags)
    {
        searchedOrFilteredGames.Clear();
        var games = _gameService.filterGames(minRating, minPrice, maxPrice, Tags);
        foreach (var game in games)
        {
            searchedOrFilteredGames.Add(game);
        }
        if (games.Count == 0)
        {
            search_filter_text = "No games found for the filter";
            return;
        }
        search_filter_text = "Filtered games ";
    }

    public void SwitchToGamePage(Microsoft.UI.Xaml.DependencyObject parent, Game selectedGame)
    {
        if (parent is Frame frame)
        {
            var gamePage = new GamePage(GameService, _cartService, _userGameService, selectedGame);
            frame.Content = gamePage;
        }
    }
}