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


    public Collection<Game> getAllGames()
    {
        return GameRepository.GetAllGames();
    }

    public Collection<Tag> getAllTags()
    {
        return TagRepository.GetAllTags();
    }

    public Collection<Tag> getAllGameTags(Game game)
    {
        return new Collection<Tag>(TagRepository
            .GetAllTags()
            .Where(tag => game.Tags.Contains(tag.tag_name))
            .ToList());
    }

    public Collection<Game> searchGames(string searchQuery)
    {
        return new Collection<Game>(GameRepository
            .GetAllGames()
            .Where(game => game.Name.ToLower().Contains(searchQuery.ToLower()))
            .ToList());
    }

    public Collection<Game> filterGames(int minRating, int minPrice, int maxPrice, string[] tags)
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

    public void computeTrendingScores(Collection<Game> games)
    {
        var maxRecentSales = games.Max(game => game.noOfRecentPurchases);
        foreach (var game in games)
        {
            game.trendingScore = maxRecentSales < 1 ? 0m : Convert.ToDecimal(game.noOfRecentPurchases) / maxRecentSales;
        }
    }

    public Collection<Game> getTrendingGames()
    {
        return GetSortedAndFilteredVideoGames(GameRepository.GetAllGames());
    }

    private Collection<Game> GetSortedAndFilteredVideoGames(Collection<Game> games)
    {
        computeTrendingScores(games);
        return new Collection<Game>(games
            .OrderByDescending(game => game.trendingScore)
            .Take(10)
            .ToList());
    }

    public Collection<Game> getDiscountedGames()
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
            .Where(g => g.Id != gameId)
            .OrderBy(_ => randy.Next())
            .Take(3)
            .ToList();
    }
}