// <copyright file="DeveloperViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SteamStore;
using SteamStore.Models;
using SteamStore.Services.Interfaces;
using Windows.Gaming.Input;

public class DeveloperViewModel : INotifyPropertyChanged
{
    private readonly IDeveloperService developerService;

    public DeveloperViewModel(IDeveloperService developerService)
    {
        this.developerService = developerService;
        this.DeveloperGames = new ObservableCollection<Game>();
        this.UnvalidatedGames = new ObservableCollection<Game>();
        this.Tags = new ObservableCollection<Tag>();
        this.LoadGames();
        this.LoadTags();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<Game> DeveloperGames { get; set; }

    public ObservableCollection<Game> UnvalidatedGames { get; set; }

    public ObservableCollection<Tag> Tags { get; set; }

    public Game GetGameByIdInDeveloperGameList(int gameId)
    {
        return this.developerService.FindGameInObservableCollectionById(gameId, this.DeveloperGames);
    }

    public void LoadGames()
    {
        this.DeveloperGames.Clear();
        var games = this.developerService.GetDeveloperGames();
        foreach (var game in games)
        {
            this.DeveloperGames.Add(game);
        }

        this.OnPropertyChanged();
    }

    public void ValidateGame(int game_id)
    {
        this.developerService.ValidateGame(game_id);
    }

    public bool CheckIfUserIsADeveloper()
    {
        return this.developerService.GetCurrentUser().UserRole == User.Role.Developer;
    }

    public void CreateGame(Game game, IList<Tag> selectedTags)
    {
        this.developerService.CreateGameWithTags(game, selectedTags);
        this.DeveloperGames.Add(game);
    }

    public void UpdateGame(Game game)
    {
        // this.DeveloperGames.Remove(this.DeveloperGames.FirstOrDefault(g => g.Identifier == game.Identifier));
        // this.developerService.UpdateGame(game);
        // this.DeveloperGames.Add(game);
        this.developerService.UpdateGameAndRefreshList(game, this.DeveloperGames);
    }

    public void UpdateGameWithTags(Game game, IList<Tag> selectedTags)
    {
    }

    public void DeleteGame(int game_id)
    {
        // var game = DeveloperGames.FirstOrDefault(g => g.Identifier == game_id);
        // DeveloperGames.Remove(game);
        // developerService.DeleteGame(game_id);
        this.developerService.DeleteGame(game_id, this.DeveloperGames);
    }

    public void RejectGame(int game_id)
    {
        // developerService.RejectGame(game_id);
        // var game = DeveloperGames.FirstOrDefault(x => x.Identifier == game_id);
        // UnvalidatedGames.Remove(game);
        this.developerService.RejectGameAndRemoveFromUnvalidated(game_id, this.UnvalidatedGames);
    }

    public void LoadUnvalidated()
    {
        this.UnvalidatedGames.Clear();
        var games = this.developerService.GetUnvalidated();
        foreach (var game in games)
        {
            this.UnvalidatedGames.Add(game);
        }
    }

    public bool IsGameIdInUse(int gameId)
    {
        // Check in the developer's own games first
        // if (this.DeveloperGames.Any(g => g.Identifier == gameId))
        // {
        //    return true;
        // }

        //// Check in unvalidated games
        // if (this.UnvalidatedGames.Any(g => g.Identifier == gameId))
        // {
        //    return true;
        // }

        // return this.developerService.IsGameIdInUse(gameId);
        return this.developerService.IsGameIdInUse(gameId, this.DeveloperGames, this.UnvalidatedGames);
    }

    public int GetGameOwnerCount(int game_id)
    {
        return this.developerService.GetGameOwnerCount(game_id);
    }

    public void RejectGameWithMessage(int game_id, string rejectionMessage)
    {
    }

    public async Task HandleRejectGameAsync(int gameId, string rejectionReason)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(rejectionReason))
            {
                this.developerService.RejectGameWithMessage(gameId, rejectionReason);
            }
            else
            {
                this.RejectGame(gameId);
            }

            this.LoadUnvalidated();
        }
        catch (Exception exception)
        {
            await this.ShowErrorMessage("Error", $"Failed to reject game: {exception.Message}");
        }
    }

    public async Task CreateGameAsync(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string recommendedRequirements, string discountText, IList<Tag> selectedTags)
    {
        // This can throw if any validation fails – and that’s okay
        Game game = this.developerService.CreateValidatedGame(
            gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl, minimumRequirement, recommendedRequirements, discountText, selectedTags);
        this.DeveloperGames.Add(game);
        this.OnPropertyChanged(nameof(this.DeveloperGames));
    }

    public async Task UpdateGameAsync(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string recommendedRequirements, string discountText, IList<Tag> selectedTags)
    {
        Game game = this.developerService.ValidateInputForAddingAGame(gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl, minimumRequirement, recommendedRequirements, discountText, selectedTags);

        // System.Diagnostics.Debug.WriteLine("VALID input");
        this.developerService.UpdateGameWithTags(game, selectedTags);
    }

    public string GetRejectionMessage(int gameId)
    {
        return this.developerService.GetRejectionMessage(gameId);
    }

    public List<Tag> GetGameTags(int gameId)
    {
        return this.developerService.GetGameTags(gameId);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void LoadTags()
    {
        this.Tags.Clear();
        var tags = this.developerService.GetAllTags();
        foreach (var tag in tags)
        {
            this.Tags.Add(tag);
        }

        this.OnPropertyChanged();
    }

    private async Task ShowErrorMessage(string title, string message)
    {
        ContentDialog errorDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot,
        };
        await errorDialog.ShowAsync();
    }

    public IList<Tag> GetMatchingTags(int gameId, IList<Tag> allTags)
    {
        return this.developerService.GetMatchingTagsForGame(gameId, allTags);
    }
}