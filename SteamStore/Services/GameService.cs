using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SteamStore.Models;
using SteamStore.Repositories;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;

namespace SteamStore.Services;

public class GameService : IGameService
{
    public IGameRepository GameRepository { get; set; }
    public ITagRepository TagRepository { get; set; }


    public Collection<Game> GetAllGames()
    {
        return GameRepository.GetAllGames();
    }

    public Collection<Tag> GetAllTags()
    {
        return TagRepository.GetAllTags();
    }

    public Collection<Tag> GetAllGameTags(Game game)
    {
        return new Collection<Tag>(TagRepository
            .GetAllTags()
            .Where(tag => game.Tags.Contains(tag.Tag_name))
            .ToList());
    }

    public Collection<Game> SearchGames(string searchQuery)
    {
        return new Collection<Game>(GameRepository
            .GetAllGames()
            .Where(game => game.Name.ToLower().Contains(searchQuery.ToLower()))
            .ToList());
    }

    public Collection<Game> FilterGames(int minRating, int minPrice, int maxPrice, string[] tags)
    {
        if (tags == null) throw new ArgumentNullException(nameof(tags));
        return new Collection<Game>(
            GameRepository.GetAllGames()
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
        return GetSortedAndFilteredVideoGames(GameRepository.GetAllGames());
    }

    private Collection<Game> GetSortedAndFilteredVideoGames(Collection<Game> games)
    {
        ComputeTrendingScores(games);
        return new Collection<Game>(games
            .OrderByDescending(game => game.TrendingScore)
            .Take(10)
            .ToList());
    }

    public Collection<Game> GetDiscountedGames()
    {
        var discountedGames = GameRepository.GetAllGames()
            .Where(game => game.Discount > 0).ToList();
        return GetSortedAndFilteredVideoGames(new Collection<Game>(discountedGames));
    }

    public List<Game> GetSimilarGames(int gameId)
    {
        var randy = new Random(DateTime.Now.Millisecond);
        var allGames = GameRepository.GetAllGames();
        return allGames
            .Where(g => g.Identifier != gameId)
            .OrderBy(_ => randy.Next())
            .Take(3)
            .ToList();
    }
}