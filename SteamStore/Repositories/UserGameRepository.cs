// <copyright file="UserGameRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using SteamStore.Data;
using SteamStore.Repositories.Interfaces;
using Windows.Gaming.Input;

public class UserGameRepository : IUserGameRepository
{
    private User user;
    private IDataLink dataLink;

    public UserGameRepository(IDataLink data, User user)
    {
        this.user = user;
        this.dataLink = data;
    }

    public bool IsGamePurchased(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game.Identifier),
            new SqlParameter("@user_id", this.user.UserIdentifier),
        };
        try
        {
            return this.dataLink.ExecuteScalar<int>("IsGamePurchased", parameters) > 0;
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public void RemoveGameFromWishlist(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
            new SqlParameter("@game_id", game.Identifier),
        };

        try
        {
            this.dataLink.ExecuteNonQuery("RemoveGameFromWishlist", parameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void AddGameToPurchased(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
            new SqlParameter("@game_id", game.Identifier),
        };

        try
        {
            if (Convert.ToDecimal(this.user.WalletBalance) < game.Price)
            {
                throw new Exception("Insufficient funds");
            }

            this.dataLink.ExecuteNonQuery("AddGameToPurchased", parameters);
            this.user.WalletBalance -= (float)game.Price;

            // Calculate and add points (121 points for every $1 spent)
            this.AddPointsForPurchase((float)game.Price);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public void AddGameToWishlist(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
            new SqlParameter("@game_id", game.Identifier),
        };

        try
        {
            this.dataLink.ExecuteNonQuery("AddGameToWishlist", parameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public string[] GetGameTags(int gameId)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@gid", gameId),
        };

        try
        {
            DataTable result = this.dataLink.ExecuteReader("getGameTags", parameters);
            List<string> tags = new List<string>();

            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    tags.Add((string)row["tag_name"]);
                }
            }

            return tags.ToArray();
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting tags for game {gameId}: {e.Message}");
        }
    }

    public int GetGameOwnerCount(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new ("@game_id", gameId) };
        var result = this.dataLink.ExecuteReader("GetGameOwnerCount", sqlParameters);

        return result is { Rows.Count: > 0 } ? Convert.ToInt32(result.Rows[0]["OwnerCount"]) : 0;
    }

    public Collection<Game> GetAllUserGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@uid", this.user.UserIdentifier),
        };

        var result = this.dataLink.ExecuteReader("getUserGames", parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Identifier = (int)row["game_id"],
                    Name = (string)row["name"],
                    Description = (string)row["Description"],
                    ImagePath = (string)row["image_url"],
                    Price = Convert.ToDecimal(row["price"]),
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Tags = this.GetGameTags((int)row["game_id"]),
                    TrendingScore = Game.NOTCOMPUTED,
                };
                games.Add(game);
            }
        }

        return new Collection<Game>(games);
    }

    public void AddPointsForPurchase(float purchaseAmount)
    {
        try
        {
            // Calculate points (121 points for every $1 spent)
            int earnedPoints = (int)(purchaseAmount * 121);

            // Update user's point balance
            this.user.PointsBalance += earnedPoints;

            // Update in database
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", this.user.UserIdentifier),
                new SqlParameter("@PointBalance", this.user.PointsBalance),
            };

            this.dataLink.ExecuteNonQuery("UpdateUserPointBalance", parameters);
        }
        catch (Exception example)
        {
            throw new Exception($"Failed to add points for purchase: {example.Message}");
        }
    }

    public float GetUserPointsBalance()
    {
        // Simply return the user's current points balance from the model
        return this.user.PointsBalance;
    }

    public Collection<Game> GetWishlistGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", this.user.UserIdentifier),
        };

        var result = this.dataLink.ExecuteReader("GetWishlistGames", parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Identifier = (int)row["game_id"],
                    Name = (string)row["name"],
                    Description = (string)row["Description"],
                    ImagePath = (string)row["image_url"],
                    Price = Convert.ToDecimal(row["price"]),
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Rating = Convert.ToDecimal(row["rating"]),
                    Discount = Convert.ToDecimal(row["discount"]),
                    Tags = this.GetGameTags((int)row["game_id"]),
                };
                games.Add(game);
            }
        }

        return new Collection<Game>(games);
    }
}