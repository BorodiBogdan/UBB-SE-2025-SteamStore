using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using SteamStore.Models;
using SteamStore;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Gaming.Input;
using SteamStore.Services.Interfaces;

public class DeveloperViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Game> DeveloperGames { get; set; }
    public ObservableCollection<Game> UnvalidatedGames {  get; set; }
    public ObservableCollection<Tag> Tags { get; set; }

    private readonly IDeveloperService _developerService;

    public DeveloperViewModel(IDeveloperService developerService)
    {
        _developerService = developerService;
        DeveloperGames = new ObservableCollection<Game>();
        UnvalidatedGames = new ObservableCollection<Game>();
        Tags = new ObservableCollection<Tag>();
        LoadGames();
        LoadTags();
    }
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Game GetGameByIdInDeveloperGameList(int gameId)
    {
        return _developerService.FindGameInObservableCollectionById(gameId,DeveloperGames);

    }


    private void LoadTags()
    {
        Tags.Clear();
        var tags = _developerService.GetAllTags();
        foreach (var tag in tags)
        {
            Tags.Add(tag);
        }
        OnPropertyChanged();
    }

    public void LoadGames()
    {
        DeveloperGames.Clear();
        var games = _developerService.GetDeveloperGames();
        foreach (var game in games)
        {
            DeveloperGames.Add(game);
        }
        OnPropertyChanged();
    }

    public void ValidateGame(int game_id)
    {
        _developerService.ValidateGame(game_id);
    }

    public bool CheckIfUserIsADeveloper()
    {
        return _developerService.GetCurrentUser().UserRole == User.Role.Developer;
    }

    public void CreateGame(Game game, IList<Tag> selectedTags)
    {
      
        _developerService.CreateGameWithTags(game, selectedTags);
        DeveloperGames.Add(game);
    }

    public void UpdateGame(Game game)
    {
        DeveloperGames.Remove(DeveloperGames.FirstOrDefault(g => g.Identifier == game.Identifier));
        _developerService.UpdateGame(game);
        DeveloperGames.Add(game);
    }
    public void UpdateGameWithTags(Game game, IList<Tag> selectedTags)
    {

    }

    public void DeleteGame(int game_id)
    {
        var game = DeveloperGames.FirstOrDefault(g => g.Identifier == game_id);
        DeveloperGames.Remove(game);
        _developerService.DeleteGame(game_id);
    }

    public void RejectGame(int game_id)
    {
        _developerService.RejectGame(game_id);
        var game = DeveloperGames.FirstOrDefault(x => x.Identifier == game_id);
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
        if (DeveloperGames.Any(g => g.Identifier == gameId))
        {
            return true;
        }
        
        // Check in unvalidated games
        if (UnvalidatedGames.Any(g => g.Identifier == gameId))
        {
            return true;
        }
        
        return _developerService.IsGameIdInUse(gameId);
    }

    public int GetGameOwnerCount(int game_id)
    {
        return _developerService.GetGameOwnerCount(game_id);
    }
    public void RejectGameWithMessage(int game_id, string rejectionMessage)
    {

    }

    public async Task HandleRejectGameAsync(int gameId,string rejectionReason)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(rejectionReason))
            {
                _developerService.RejectGameWithMessage(gameId, rejectionReason);
            }
            else
            {
                RejectGame(gameId);
            }
            LoadUnvalidated();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage("Error", $"Failed to reject game: {ex.Message}");
        }

    }

    private async Task ShowErrorMessage(string title, string message)
    {
        ContentDialog errorDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.m_window.Content.XamlRoot
        };
        await errorDialog.ShowAsync();
    }

    
    public async Task CreateGameAsync(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string recommendedRequirements, string discountText, IList<Tag> selectedTags)
    {
        // This can throw if any validation fails – and that’s okay
        Game game = _developerService.CreateValidatedGame(
            gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl,
            minimumRequirement, recommendedRequirements, discountText, selectedTags);

       
        DeveloperGames.Add(game);
        OnPropertyChanged(nameof(DeveloperGames));
    }


    public async Task UpdateGameAsync(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string recommendedRequirements, string discountText, IList<Tag> selectedTags)
    {


        Game game = _developerService.ValidateInputForAddingAGame(gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl, minimumRequirement, recommendedRequirements, discountText, selectedTags);
        //System.Diagnostics.Debug.WriteLine("VALID input");
        _developerService.UpdateGameWithTags(game, selectedTags);

    }

    public string GetRejectionMessage(int gameId)
    {
        return _developerService.GetRejectionMessage(gameId);
    }

    public List<Tag> GetGameTags(int gameId)
    {
        return _developerService.GetGameTags(gameId);
    }

}