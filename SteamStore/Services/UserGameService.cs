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
using SteamStore.Constants;
using static SteamStore.Constants.NotificationStrings;
using SteamStore.Repositories;

public class UserGameService : IUserGameService
{
    public IUserGameRepository UserGameRepository { get; set; }
    public IGameRepository GameRepository { get; set; }
    
    public ITagRepository TagRepository { get; set; }
    
    // Property to track points earned in the last purchase
    public int LastEarnedPoints { get; private set; }
    
    public void RemoveGameFromWishlist(Game game)
    {
        UserGameRepository.removeGameFromWishlist(game);
    }

    public void AddGameToWishlist(Game game)
    {
        try
        {
            // Check if game is already purchased
            if (IsGamePurchased(game))
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
    public void PurchaseGames(List<Game> games)
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

    public void ComputeNoOfUserGamesForEachTag(Collection<Tag> all_tags)
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

    public Collection<Tag> GetFavoriteUserTags()
    {
        var allTags = TagRepository.GetAllTags();
        ComputeNoOfUserGamesForEachTag(allTags);
        return new Collection<Tag>(allTags
            .OrderByDescending(tag => tag.no_of_user_games_with_tag)
            .Take(3)
            .ToList());
    }

    public void ComputeTagScoreForGames(Collection<Game> games)
    {
        var favorite_tags = GetFavoriteUserTags();
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

    public void ComputeTrendingScores(Collection<Game> games)
    {
        var maxRecentSales = games.Max(game => game.noOfRecentPurchases);
        foreach (var game in games)
        {
            game.trendingScore = Convert.ToDecimal(game.noOfRecentPurchases) / maxRecentSales;
        }
    }

    public Collection<Game> GetRecommendedGames()
    {
        var games = GameRepository.GetAllGames();
        ComputeTrendingScores(games);
        ComputeTagScoreForGames(games);
        return new Collection<Game>(games
                .OrderByDescending(game => game.tagScore *0.5m + game.trendingScore *0.5m)
                .Take(10)
                .ToList());
    }

    public Collection<Game> GetWishListGames()
    {
        return UserGameRepository.getWishlistGames();
    }

    public Collection<Game> SearchWishListByName(string searchText)
    {
        return new Collection<Game>(UserGameRepository.getWishlistGames()
            .Where(game => game.Name.ToLower().Contains(searchText.ToLower()))
            .ToList());
    }

    public Collection<Game> FilterWishListGames(string criteria)
    {
        var games = UserGameRepository.getWishlistGames();
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
        return UserGameRepository.isGamePurchased(game);
    }
    public Collection<Game> SortWishListGames(string criteria, bool ascending)
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