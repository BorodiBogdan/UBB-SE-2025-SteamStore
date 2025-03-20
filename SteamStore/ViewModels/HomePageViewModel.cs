using Microsoft.UI.Xaml.Automation.Peers;
using SteamStore.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class HomePageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Game> searchedOrFilteredGames{get;set;}
    public ObservableCollection<Game> trendingGames { get; set; }

    public string _search_filter_text;
    public ObservableCollection<Tag> tags { get; set; }
    private readonly GameService _gameService;

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
    public GameService GameService { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public HomePageViewModel(GameService gameService)
    {
        _gameService = gameService;
        GameService = gameService; // Assign to public property
        searchedOrFilteredGames = new ObservableCollection<Game>();
        trendingGames = new ObservableCollection<Game>();
        LoadAllGames();
        LoadTrendingGames();
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

}