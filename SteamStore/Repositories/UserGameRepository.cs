using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;

public class UserGameRepository
{
    private User user;
    private DataLink data;

    public UserGameRepository(DataLink data, User user)
    {
        this.user = user;
        this.data = data;
    }

    public bool isGamePurchased(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game.Id),
            new SqlParameter("@user_id", user.UserId),
        };
        try
        {
            return data.ExecuteScalar<int>("IsGamePurchased", parameters) > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void removeGameFromWishlist(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            data.ExecuteNonQuery("RemoveGameFromWishlist", parameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void addGameToPurchased(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            if(user.WalletBalance < game.Price)
            {
                throw new Exception("Insufficient funds");
            }

            data.ExecuteNonQuery("AddGameToPurchased", parameters);
            user.WalletBalance -= (float)game.Price;
            
            // Calculate and add points (121 points for every $1 spent)
            AddPointsForPurchase((float)game.Price);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void addGameToWishlist(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            data.ExecuteNonQuery("AddGameToWishlist", parameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    private string[] GetGameTags(int gameId)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@gid", gameId)
        };

        try
        {
            DataTable result = data.ExecuteReader("getGameTags", parameters);
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

    public Collection<Game> getAllUserGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@uid", user.UserId)
        };

        DataTable? result = data.ExecuteReader("getUserGames",parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["game_id"],
                    Name = (string)row["name"],
                    Description = (string)row["Description"],
                    ImagePath = (string)row["image_url"],
                    Price = Convert.ToDouble(row["price"]),
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Tags = GetGameTags((int)row["game_id"]),
                    trendingScore = Game.NOT_COMPUTED
                };
                games.Add(game);
            }
        }

        return new Collection<Game>(games);
    }

    private void AddPointsForPurchase(float purchaseAmount)
    {
        try
        {
            // Calculate points (121 points for every $1 spent)
            int earnedPoints = (int)(purchaseAmount * 121);
            
            // Update user's point balance
            user.PointsBalance += earnedPoints;
            
            // Update in database
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", user.UserId),
                new SqlParameter("@PointBalance", user.PointsBalance)
            };

            data.ExecuteNonQuery("UpdateUserPointBalance", parameters);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add points for purchase: {ex.Message}");
        }
    }

    public float GetUserPointsBalance()
    {
        // Simply return the user's current points balance from the model
        return user.PointsBalance;
    }

    public Collection<Game> getWishlistGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId)
        };

        DataTable? result = data.ExecuteReader("GetWishlistGames", parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["game_id"],
                    Name = (string)row["name"],
                    Description = (string)row["Description"],
                    ImagePath = (string)row["image_url"],
                    Price = Convert.ToDouble(row["price"]),
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Rating = Convert.ToSingle(row["rating"]),
                    Discount = Convert.ToSingle(row["discount"]),
                    Tags = GetGameTags((int)row["game_id"])
                };
                games.Add(game);
            }
        }

        return new Collection<Game>(games);
    }

}