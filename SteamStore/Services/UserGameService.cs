// <copyright file="UserGameService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SteamStore.Constants;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;

public class UserGameService : IUserGameService
{
    public IUserGameRepository UserGameRepository { get; set; }

    public IGameRepository GameRepository { get; set; }

    public ITagRepository TagRepository { get; set; }

    // Property to track points earned in the last purchase
    public int LastEarnedPoints { get; private set; }

    public void RemoveGameFromWishlist(Game game)
    {
        this.UserGameRepository.RemoveGameFromWishlist(game);
    }

    public void AddGameToWishlist(Game game)
    {
        try
        {
            // Check if game is already purchased
            if (this.IsGamePurchased(game))
            {
                throw new Exception(string.Format(ExceptionMessages.GameAlreadyOwned, game.Name));
            }

            this.UserGameRepository.AddGameToWishlist(game);
        }
        catch (Exception e)
        {
            // Clean up the error message
            string message = e.Message;
            if (message.Contains("ExecuteNonQuery"))
            {
                message = string.Format(ExceptionMessages.GameAlreadyInWishlist, game.Name);
            }

            throw new Exception(message);
        }
    }

    public void PurchaseGames(List<Game> games)
    {
        // Reset points counter
        this.LastEarnedPoints = 0;

        // Track user's points before purchase
        float pointsBalanceBefore = this.UserGameRepository.GetUserPointsBalance();

        // Purchase games
        foreach (var game in games)
        {
            this.UserGameRepository.AddGameToPurchased(game);
            this.UserGameRepository.RemoveGameFromWishlist(game);
        }

        // Calculate earned points by comparing balances
        float pointsBalanceAfter = this.UserGameRepository.GetUserPointsBalance();
        this.LastEarnedPoints = (int)(pointsBalanceAfter - pointsBalanceBefore);
    }

    public void ComputeNoOfUserGamesForEachTag(Collection<Tag> all_tags)
    {
        var user_games = this.UserGameRepository.GetAllUserGames();
        Dictionary<string, Tag> tagsDictionary = all_tags
            .ToDictionary(tag => tag.Tag_name);
        foreach (var tag in tagsDictionary.Values)
        {
            tag.NumberOfUserGamesWithTag = 0;
        }

        foreach (var user_game in user_games)
        {
            foreach (string tag_name in user_game.Tags)
            {
                tagsDictionary[tag_name].NumberOfUserGamesWithTag++;
            }
        }
    }

    public Collection<Tag> GetFavoriteUserTags()
    {
        var allTags = this.TagRepository.GetAllTags();
        this.ComputeNoOfUserGamesForEachTag(allTags);
        return new Collection<Tag>(allTags
            .OrderByDescending(tag => tag.NumberOfUserGamesWithTag)
            .Take(3)
            .ToList());
    }

    public void ComputeTagScoreForGames(Collection<Game> games)
    {
        var favorite_tags = this.GetFavoriteUserTags();
        foreach (var game in games)
        {
            game.TagScore = 0;
            foreach (var tag in favorite_tags)
            {
                if (game.Tags.Contains(tag.Tag_name))
                {
                    game.TagScore += tag.NumberOfUserGamesWithTag;
                }
            }
            game.TagScore = game.TagScore * (1 / 3m);
        }
    }

    public void ComputeTrendingScores(Collection<Game> games)
    {
        var maxRecentSales = games.Max(game => game.NumberOfRecentPurchases);
        foreach (var game in games)
        {
            game.TrendingScore = Convert.ToDecimal(game.NumberOfRecentPurchases) / maxRecentSales;
        }
    }

    public Collection<Game> GetRecommendedGames()
    {
        var games = this.GameRepository.GetAllGames();
        this.ComputeTrendingScores(games);
        this.ComputeTagScoreForGames(games);
        return new Collection<Game>(games
                .OrderByDescending(game => (game.TagScore * 0.5m) + (game.TrendingScore * 0.5m))
                .Take(10)
                .ToList());
    }

    public Collection<Game> GetWishListGames()
    {
        return this.UserGameRepository.GetWishlistGames();
    }

    public Collection<Game> SearchWishListByName(string searchText)
    {
        return new Collection<Game>(this.UserGameRepository.GetWishlistGames()
            .Where(game => game.Name.ToLower().Contains(searchText.ToLower()))
            .ToList());
    }

    public Collection<Game> FilterWishListGames(string criteria)
    {
        var games = this.UserGameRepository.GetWishlistGames();
        switch (criteria)
        {
            case FilterCriteria.OVERWHELMINGLYPOSITIVE:
                return new Collection<Game>(games.Where(g => g.Rating >= 4.5m).ToList());
            case FilterCriteria.VERYPOSITIVE:
                return new Collection<Game>(games.Where(g => g.Rating >= 4 && g.Rating < 4.5m).ToList());
            case FilterCriteria.MIXED:
                return new Collection<Game>(games.Where(g => g.Rating >= 2 && g.Rating < 4m).ToList());
            case FilterCriteria.NEGATIVE:
                return new Collection<Game>(games.Where(g => g.Rating < 2).ToList());
            default:
                return games;
        }
    }

    public bool IsGamePurchased(Game game)
    {
        return this.UserGameRepository.IsGamePurchased(game);
    }

    public Collection<Game> SortWishListGames(string criteria, bool ascending)
    {
        var games = this.UserGameRepository.GetWishlistGames();
        IOrderedEnumerable<Game> orderedGames = criteria switch
        {
            FilterCriteria.PRICE => ascending ? games.OrderBy(g => g.Price) : games.OrderByDescending(g => g.Price),
            FilterCriteria.RATING => ascending ? games.OrderBy(g => g.Rating) : games.OrderByDescending(g => g.Rating),
            FilterCriteria.DISCOUNT => ascending ? games.OrderBy(g => g.Discount) : games.OrderByDescending(g => g.Discount),
            _ => ascending ? games.OrderBy(g => g.Name) : games.OrderByDescending(g => g.Name)
        };
        return new Collection<Game>(orderedGames.ToList());
    }
}