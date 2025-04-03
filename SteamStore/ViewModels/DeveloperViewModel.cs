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

public class DeveloperViewModel : INotifyPropertyChanged
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
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    public async Task<string> CreateGameAsync(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minReq, string recReq, string discountText, IList<Tag> selectedTags)
    {
        int gameId;
        double price;
        float discount;

        if (string.IsNullOrWhiteSpace(gameIdText) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText) ||
            string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(imageUrl) || string.IsNullOrWhiteSpace(minReq) ||
            string.IsNullOrWhiteSpace(recReq) || string.IsNullOrWhiteSpace(discountText))
        {
            return "All fields are required.";
        }

        if (!int.TryParse(gameIdText, out gameId))
        {
            return "Game ID must be a valid integer.";
        }

        if (!double.TryParse(priceText, out price) || price < 0)
        {
            return "Price must be a positive number.";
        }
        if (!float.TryParse(discountText, out discount) || discount < 0 || discount > 100)
        {
            return "Discount must be between 0 and 100.";
        }

        if (selectedTags == null || selectedTags.Count == 0)
        {
            return "Please select at least one tag for the game.";
        }

        var game = new Game
        {
            Id = gameId,
            Name = name,
            Price = price,
            Description = description,
            ImagePath = imageUrl,
            GameplayPath = gameplayUrl,
            TrailerPath = trailerUrl,
            MinimumRequirements = minReq,
            RecommendedRequirements = recReq,
            Status = "Pending",
            Discount = discount
        };

       
        try
        {
          
            if (IsGameIdInUse(gameId))
            {
                return "Game ID is already in use. Please choose another ID.";
            }

            CreateGame(game,selectedTags);
            OnPropertyChanged(nameof(DeveloperGames));

            return null; // Success
        }
        catch (Exception ex)
        {
            return $"Failed to add game: {ex.Message}";
        }
    }
}