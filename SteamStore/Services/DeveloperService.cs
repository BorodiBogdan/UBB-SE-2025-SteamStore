// <copyright file="DeveloperService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Constants;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;
using static SteamStore.Constants.NotificationStrings;

public class DeveloperService : IDeveloperService
{
    private static string pENDINGSTATE = "Pending";

    public IGameRepository GameRepository { get; set; }

    public ITagRepository TagRepository { get; set; }

    public IUserGameRepository UserGameRepository { get; set; }

    public User User { get; set; }

    public void ValidateGame(int game_id)
    {
        this.GameRepository.ValidateGame(game_id);
    }

    public Game ValidateInputForAddingAGame(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string reccommendedRequirement, string discountText, IList<Tag> selectedTags)
    {
        if (string.IsNullOrWhiteSpace(gameIdText) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priceText) ||
            string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(imageUrl) || string.IsNullOrWhiteSpace(minimumRequirement) ||
            string.IsNullOrWhiteSpace(reccommendedRequirement) || string.IsNullOrWhiteSpace(discountText))
        {
            throw new Exception(ExceptionMessages.AllFieldsRequired);
        }

        if (!int.TryParse(gameIdText, out int gameId))
        {
            throw new Exception(ExceptionMessages.InvalidGameId);
        }

        if (!decimal.TryParse(priceText, out var price) || price < 0)
        {
            throw new Exception(ExceptionMessages.InvalidPrice);
        }

        if (!decimal.TryParse(discountText, out var discount) || discount < 0 || discount > 100)
        {
            throw new Exception(ExceptionMessages.InvalidDiscount);
        }

        if (selectedTags == null || selectedTags.Count == 0)
        {
            throw new Exception(ExceptionMessages.NoTagsSelected);
        }

       // var game = new Game(gameId, name,price, description, imageUrl, gameplayUrl, trailerUrl, minReq, recReq, "Pending", discount);
        var game = new Game
        {
            Identifier = gameId,
            Name = name,
            Price = price,
            Description = description,
            ImagePath = imageUrl,
            GameplayPath = gameplayUrl,
            TrailerPath = trailerUrl,
            MinimumRequirements = minimumRequirement,
            RecommendedRequirements = reccommendedRequirement,
            Status = pENDINGSTATE,
            Discount = discount,
        };
        return game;
    }

    public Game FindGameInObservableCollectionById(int gameId, ObservableCollection<Game> gameList)
    {
        foreach (Game game in gameList)
        {
            if (game.Identifier == gameId)
            {
                return game;
            }
        }

        return null;
    }

    public void CreateGame(Game game)
    {
        game.PublisherIdentifier = this.User.UserIdentifier;
        this.GameRepository.CreateGame(game);
    }

    public void CreateGameWithTags(Game game, IList<Tag> selectedTags)
    {
        try
        {
            this.CreateGame(game);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }

        if (selectedTags != null && selectedTags.Count > 0)
        {
            foreach (var tag in selectedTags)
            {
                this.InsertGameTag(game.Identifier, tag.TagId);
            }
        }
    }

    public void UpdateGame(Game game)
    {
        try
        {
            game.PublisherIdentifier = User.UserIdentifier;
            this.GameRepository.UpdateGame(game.Identifier,game);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public void UpdateGameWithTags(Game game, IList<Tag> selectedTags)
    {
        try
        {
            game.PublisherIdentifier = User.UserIdentifier;
            this.GameRepository.UpdateGame(game.Identifier, game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        try
        {
            //System.Diagnostics.Debug.WriteLine("deleting the tags!");
            DeleteGameTags(game.Identifier);
            if (selectedTags != null && selectedTags.Count > 0)
            {
                foreach (var tag in selectedTags)
                {
                    this.InsertGameTag(game.Identifier, tag.TagId);
                }
            }
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public void DeleteGame(int game_id)
    {
        try
        {
            this.GameRepository.DeleteGame(game_id);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public List<Game> GetDeveloperGames()
    {
        return this.GameRepository.GetDeveloperGames(this.User.UserIdentifier);
    }

    public List<Game> GetUnvalidated()
    {
        return this.GameRepository.GetUnvalidated(this.User.UserIdentifier);
    }

    public void RejectGame(int game_id)
    {
        this.GameRepository.RejectGame(game_id);
    }

    public void RejectGameWithMessage(int game_id, string message)
    {
        this.GameRepository.RejectGameWithMessage(game_id, message);
    }

    public string GetRejectionMessage(int game_id)
    {
        return this.GameRepository.GetRejectionMessage(game_id);
    }

    public void InsertGameTag(int gameId, int tagId)
    {
        this.GameRepository.InsertGameTag(gameId, tagId);
    }

    public Collection<Tag> GetAllTags()
    {
        return this.TagRepository.GetAllTags();
    }

    public bool IsGameIdInUse(int gameId)
    {
        return this.GameRepository.IsGameIdInUse(gameId);
    }

    public List<Tag> GetGameTags(int gameId)
    {
        return this.GameRepository.GetGameTags(gameId);
    }

    public void DeleteGameTags(int gameId)
    {
        this.GameRepository.DeleteGameTags(gameId);
    }

    public int GetGameOwnerCount(int gameId)
    {
        return this.UserGameRepository.GetGameOwnerCount(gameId);
    }

    public User GetCurrentUser()
    {
        return this.User;
    }

    public Game CreateValidatedGame(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl, string minimumRequirement, string reccommendedRequirement, string discountText, IList<Tag> selectedTags)
    {
        var game = this.ValidateInputForAddingAGame(gameIdText, name, priceText, description, imageUrl, trailerUrl, gameplayUrl, minimumRequirement, reccommendedRequirement, discountText, selectedTags);

        if (this.IsGameIdInUse(game.Identifier))
        {
            throw new Exception(ExceptionMessages.IdAlreadyInUse);
        }

        this.CreateGameWithTags(game, selectedTags);
        return game;
    }
}
