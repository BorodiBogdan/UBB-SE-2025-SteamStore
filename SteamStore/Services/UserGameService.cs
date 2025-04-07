using SteamStore.Models;
using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamStore.Constants;
using SteamStore.Constants.SteamStore.Constants;
using static SteamStore.Constants.NotificationStrings;
public class UserGameService : IUserGameService
{
    private IUserGameRepository _userGameRepository;
    private IGameRepository _gameRepository;
    
    // Property to track points earned in the last purchase
    public int LastEarnedPoints { get; private set; }

    public UserGameService(IUserGameRepository userGameRepository,IGameRepository gameRepository)
    {
        _userGameRepository = userGameRepository;
        _gameRepository = gameRepository;
        LastEarnedPoints = 0;
    }
    public void removeGameFromWishlist(Game game)
    {
        _userGameRepository.removeGameFromWishlist(game);
    }

    public void addGameToWishlist(Game game)
    {
        try
        {
            // Check if game is already purchased
            if (isGamePurchased(game))
            {
                throw new Exception(string.Format(ExceptionMessages.GameAlreadyOwned, game.Name));

                //throw new Exception($"Failed to add {game.Name} to your wishlist: Game already owned");
            }
            
            _userGameRepository.addGameToWishlist(game);
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
    public void purchaseGames(List<Game> games)
    {
        // Reset points counter
        LastEarnedPoints = 0;
        
        // Track user's points before purchase
        float pointsBalanceBefore = _userGameRepository.GetUserPointsBalance();
        
        // Purchase games
        foreach (var game in games)
        {
            _userGameRepository.addGameToPurchased(game);
            _userGameRepository.removeGameFromWishlist(game);
        }
        
        // Calculate earned points by comparing balances
        float pointsBalanceAfter = _userGameRepository.GetUserPointsBalance();
        LastEarnedPoints = (int)(pointsBalanceAfter - pointsBalanceBefore);
    }

    public void computeNoOfUserGamesForEachTag(Collection<Tag> all_tags)
    {
        var user_games = _userGameRepository.getAllUserGames();
        Dictionary<string, Tag> tagsDictionary = all_tags
            .ToDictionary(tag => tag.tag_name);
        foreach (var tag in tagsDictionary.Values)
        {
            tag.no_of_user_games_with_tag = 0;
        }
        foreach(var user_game in user_games)
        {
            foreach(string tag_name in user_game.Tags)
            {
                tagsDictionary[tag_name].no_of_user_games_with_tag++;
            }
        }
    }

    public Collection<Tag> getFavoriteUserTags()
    {
        var all_tags = _gameRepository.getAllTags();
        computeNoOfUserGamesForEachTag(all_tags);
        return new Collection<Tag>(all_tags
            .OrderByDescending(tag => tag.no_of_user_games_with_tag)
            .Take(3)
            .ToList());
    }

    public void computeTagScoreForGames(Collection<Game> games)
    {
        var favorite_tags = getFavoriteUserTags();
        foreach (var game in games)
        {
            game.tagScore = 0;
            foreach (var tag in favorite_tags)
            {
                if (game.Tags.Contains(tag.tag_name))
                {
                    game.tagScore += tag.no_of_user_games_with_tag;
                }
                game.tagScore = game.tagScore * ((float) 1/3);
            }
        }
    }

    public void computeTrendingScores(Collection<Game> games)
    {
        int maxRecentSales = games.Max(game => game.noOfRecentPurchases);
        foreach (var game in games)
        {
            game.trendingScore = (((float)game.noOfRecentPurchases) / maxRecentSales);
        }
    }

    public Collection<Game> getRecommendedGames()
    {
        var games = _gameRepository.getAllGames();
        computeTrendingScores(games);
        computeTagScoreForGames(games);
        return new Collection<Game>(games
                .OrderByDescending(game => game.tagScore *0.5 + game.trendingScore *0.5)
                .Take(10)
                .ToList());
    }

    public Collection<Game> getWishListGames()
    {
        return _userGameRepository.getWishlistGames();
    }

    public Collection<Game> searchWishListByName(string searchText)
    {
        return new Collection<Game>(_userGameRepository.getWishlistGames()
            .Where(game => game.Name.ToLower().Contains(searchText.ToLower()))
            .ToList());
    }

    public Collection<Game> filterWishListGames(string criteria)
    {
        var games = _userGameRepository.getWishlistGames();
        switch (criteria)
        {
            case FilterCriteria.OverwhelminglyPositive:
                return new Collection<Game>(games.Where(g => g.Rating >= 4.5).ToList());
            case FilterCriteria.VeryPositive:
                return new Collection<Game>(games.Where(g => g.Rating >= 4 && g.Rating < 4.5).ToList());
            case FilterCriteria.Mixed:
                return new Collection<Game>(games.Where(g => g.Rating >= 2 && g.Rating < 4).ToList());
            case FilterCriteria.Negative:
                return new Collection<Game>(games.Where(g => g.Rating < 2).ToList());
            default:
                return games;
        }
    }

    public bool isGamePurchased(Game game)
    {
        return _userGameRepository.isGamePurchased(game);
    }
    public Collection<Game> sortWishListGames(string criteria, bool ascending)
    {
        var games = _userGameRepository.getWishlistGames();
        IOrderedEnumerable<Game> orderedGames = criteria switch
        {
            FilterCriteria.Price => ascending ? games.OrderBy(g => g.Price) : games.OrderByDescending(g => g.Price),
            FilterCriteria.Rating => ascending ? games.OrderBy(g => g.Rating) : games.OrderByDescending(g => g.Rating),
            FilterCriteria.Discount => ascending ? games.OrderBy(g => g.Discount) : games.OrderByDescending(g => g.Discount),
            _ => ascending ? games.OrderBy(g => g.Name) : games.OrderByDescending(g => g.Name)
        };
        return new Collection<Game>(orderedGames.ToList());
    }
}