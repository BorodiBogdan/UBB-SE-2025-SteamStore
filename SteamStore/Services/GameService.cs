// <copyright file="GameService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;
public class GameService : IGameService
{
    public IGameRepository GameRepository { get; set; }

    public ITagRepository TagRepository { get; set; }

    public Collection<Game> GetAllGames()
    {
        return this.GameRepository.GetAllGames();
    }

    public Collection<Tag> GetAllTags()
    {
        return this.TagRepository.GetAllTags();
    }

    public Collection<Tag> GetAllGameTags(Game game)
    {
        return new Collection<Tag>(this.TagRepository
            .GetAllTags()
            .Where(tag => game.Tags.Contains(tag.Tag_name))
            .ToList());
    }

    public Collection<Game> SearchGames(string searchQuery)
    {
        return new Collection<Game>(this.GameRepository
            .GetAllGames()
            .Where(game => game.Name.ToLower().Contains(searchQuery.ToLower()))
            .ToList());
    }

    public Collection<Game> FilterGames(int minRating, int minPrice, int maxPrice, string[] tags)
    {
        if (tags == null)
        {
            throw new ArgumentNullException(nameof(tags));
        }

        return new Collection<Game>(
            this.GameRepository.GetAllGames()
                .Where(game => game.Rating >= minRating &&
                               game.Price >= minPrice &&
                               game.Price <= maxPrice &&
                               (tags.Length == 0 || tags.ToList().All(tag => game.Tags.Contains(tag)))
                )
                .ToList());
    }

    public void ComputeTrendingScores(Collection<Game> games)
    {
        var maxRecentSales = games.Max(game => game.NumberOfRecentPurchases);
        foreach (var game in games)
        {
            game.TrendingScore = maxRecentSales < 1 ? 0m : Convert.ToDecimal(game.NumberOfRecentPurchases) / maxRecentSales;
        }
    }

    public Collection<Game> GetTrendingGames()
    {
        return this.GetSortedAndFilteredVideoGames(this.GameRepository.GetAllGames());
    }

    

    public Collection<Game> GetDiscountedGames()
    {
        var discountedGames = this.GameRepository.GetAllGames()
            .Where(game => game.Discount > 0).ToList();
        return this.GetSortedAndFilteredVideoGames(new Collection<Game>(discountedGames));
    }

    public List<Game> GetSimilarGames(int gameId)
    {
        var randy = new Random(DateTime.Now.Millisecond);
        var allGames = this.GameRepository.GetAllGames();
        return allGames
            .Where(g => g.Identifier != gameId)
            .OrderBy(_ => randy.Next())
            .Take(3)
            .ToList();
    }

    public Game GetGameById(int gameId)
    {
        const int initialIndex = 0;
        var allGames = this.GetAllGames();
        for (int currentIndexOfGAmeInList = initialIndex; currentIndexOfGAmeInList < allGames.Count; currentIndexOfGAmeInList++)
        {
            if (allGames[currentIndexOfGAmeInList].Identifier == gameId)
            {
                return allGames[currentIndexOfGAmeInList];
            }
        }

        return null;
    }

    private Collection<Game> GetSortedAndFilteredVideoGames(Collection<Game> games)
    {
        this.ComputeTrendingScores(games);
        return new Collection<Game>(games
            .OrderByDescending(game => game.TrendingScore)
            .Take(10)
            .ToList());
    }
}