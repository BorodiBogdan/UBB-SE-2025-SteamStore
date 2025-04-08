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
using SteamStore.Repositories;

public class UserGameService : IUserGameService
{
    public IUserGameRepository UserGameRepository { get; set; }
    public IGameRepository GameRepository { get; set; }
    
    public ITagRepository TagRepository { get; set; }
    
    // Property to track points earned in the last purchase
    public int LastEarnedPoints { get; private set; }
    
    public void removeGameFromWishlist(Game game)
    {
        UserGameRepository.removeGameFromWishlist(game);
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
            
            UserGameRepository.addGameToWishlist(game);
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
        float pointsBalanceBefore = UserGameRepository.GetUserPointsBalance();
        
        // Purchase games
        foreach (var game in games)
        {
            UserGameRepository.addGameToPurchased(game);
            UserGameRepository.removeGameFromWishlist(game);
        }
        
        // Calculate earned points by comparing balances
        float pointsBalanceAfter = UserGameRepository.GetUserPointsBalance();
        LastEarnedPoints = (int)(pointsBalanceAfter - pointsBalanceBefore);
    }

    public void computeNoOfUserGamesForEachTag(Collection<Tag> all_tags)
    {
        var user_games = UserGameRepository.getAllUserGames();
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
        var allTags = TagRepository.GetAllTags();
        computeNoOfUserGamesForEachTag(allTags);
        return new Collection<Tag>(allTags
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
                game.tagScore = game.tagScore * ( 1/3m);
            }
        }
    }

    public void computeTrendingScores(Collection<Game> games)
    {
        var maxRecentSales = games.Max(game => game.noOfRecentPurchases);
        foreach (var game in games)
        {
            game.trendingScore = Convert.ToDecimal(game.noOfRecentPurchases) / maxRecentSales;
        }
    }

    public Collection<Game> getRecommendedGames()
    {
        var games = GameRepository.GetAllGames();
        computeTrendingScores(games);
        computeTagScoreForGames(games);
        return new Collection<Game>(games
                .OrderByDescending(game => game.tagScore *0.5m + game.trendingScore *0.5m)
                .Take(10)
                .ToList());
    }

    public Collection<Game> getWishListGames()
    {
        return UserGameRepository.getWishlistGames();
    }

    public Collection<Game> searchWishListByName(string searchText)
    {
        return new Collection<Game>(UserGameRepository.getWishlistGames()
            .Where(game => game.Name.ToLower().Contains(searchText.ToLower()))
            .ToList());
    }

    public Collection<Game> filterWishListGames(string criteria)
    {
        var games = UserGameRepository.getWishlistGames();
        switch (criteria)
        {
            case FilterCriteria.OVERWHELMINGLY_POSITIVE:
                return new Collection<Game>(games.Where(g => g.Rating >= 4.5m).ToList());
            case FilterCriteria.VERY_POSITIVE:
                return new Collection<Game>(games.Where(g => g.Rating >= 4 && g.Rating < 4.5m).ToList());
            case FilterCriteria.MIXED:
                return new Collection<Game>(games.Where(g => g.Rating >= 2 && g.Rating < 4m).ToList());
            case FilterCriteria.NEGATIVE:
                return new Collection<Game>(games.Where(g => g.Rating < 2).ToList());
            default:
                return games;
        }
    }

    public bool isGamePurchased(Game game)
    {
        return UserGameRepository.isGamePurchased(game);
    }
    public Collection<Game> sortWishListGames(string criteria, bool ascending)
    {
        var games = UserGameRepository.getWishlistGames();
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