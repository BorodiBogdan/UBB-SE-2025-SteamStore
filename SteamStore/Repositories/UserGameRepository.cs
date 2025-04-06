using SteamStore.Data;
using SteamStore.Repositories.Interfaces;
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

public class UserGameRepository : IUserGameRepository
{
    private User _user;
    private IDataLink _data;

    public UserGameRepository(IDataLink data, User user)
    {
        this._user = user;
        this._data = data;
    }

    public bool isGamePurchased(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game.Id),
            new SqlParameter("@user_id", _user.UserId),
        };
        try
        {
            return _data.ExecuteScalar<int>("IsGamePurchased", parameters) > 0;
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
            new SqlParameter("@user_id", _user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            _data.ExecuteNonQuery("RemoveGameFromWishlist", parameters);
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
            new SqlParameter("@user_id", _user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            if(_user.WalletBalance < game.Price)
            {
                throw new Exception("Insufficient funds");
            }

            _data.ExecuteNonQuery("AddGameToPurchased", parameters);
            _user.WalletBalance -= (float)game.Price;
            
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
            new SqlParameter("@user_id", _user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            _data.ExecuteNonQuery("AddGameToWishlist", parameters);
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
            new SqlParameter("@gid", gameId)
        };

        try
        {
            DataTable result = _data.ExecuteReader("getGameTags", parameters);
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
            new SqlParameter("@uid", _user.UserId)
        };

        DataTable? result = _data.ExecuteReader("getUserGames",parameters);
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

    public void AddPointsForPurchase(float purchaseAmount)
    {
        try
        {
            // Calculate points (121 points for every $1 spent)
            int earnedPoints = (int)(purchaseAmount * 121);
            
            // Update user's point balance
            _user.PointsBalance += earnedPoints;
            
            // Update in database
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", _user.UserId),
                new SqlParameter("@PointBalance", _user.PointsBalance)
            };

            _data.ExecuteNonQuery("UpdateUserPointBalance", parameters);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to add points for purchase: {ex.Message}");
        }
    }

    public float GetUserPointsBalance()
    {
        // Simply return the user's current points balance from the model
        return _user.PointsBalance;
    }

    public Collection<Game> getWishlistGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", _user.UserId)
        };

        DataTable? result = _data.ExecuteReader("GetWishlistGames", parameters);
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