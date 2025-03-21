using SteamStore.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

public class GameService
{
    private GameRepository _gameRepository;
    public GameService(GameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    public Collection<Game> getAllGames()
    {
        return _gameRepository.getAllGames();
    }

    public Collection<Tag> getAllTags()
    {
        return _gameRepository.getAllTags();
    }

    public Collection<Game> searchGames(String search_query)
    {
        return new Collection<Game>(_gameRepository
       .getAllGames()
       .Where(game => game.Name.ToLower().Contains(search_query.ToLower()))
       .ToList());
    }

    public Collection<Game> filterGames(int minRating,int minPrice,int maxPrice, String[] Tags) {
        return new Collection<Game>(
            _gameRepository.getAllGames()
            .Where(game => game.Rating >= minRating &&
                game.Price >= minPrice &&
                game.Price <= maxPrice && 
                (Tags.Length == 0 || Tags.ToList().All(tag => game.Tags.Contains(tag)))
            )
            .ToList());
    }

    public void computeTrendingScores(Collection<Game> games)
    {
        int maxRecentSales = games.Max(game => game.noOfRecentPurchases);
        foreach (var game in games)
        {
            game.trendingScore = (((float)game.noOfRecentPurchases) / maxRecentSales);
        }
    }

    public Collection<Game> getTrendingGames()
    {
        var games = _gameRepository.getAllGames();
        computeTrendingScores(games);
        return new Collection<Game>(games
                       .OrderByDescending(game => game.trendingScore)
                       .Take(10)
                       .ToList());
    }

    public Collection<Game> getDiscountedGames()
    {
        var games = _gameRepository.getAllGames();
        computeTrendingScores(games);
        return new Collection<Game>(games
                        .Where(game => game.Discount>0)
                        .OrderByDescending(game => game.trendingScore)
                        .Take(10)
                        .ToList());
    }
}