using System.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Pages;
using SteamStore.Models;
using SteamStore.Data;
using SteamStore.Repositories.Interfaces;

public class GameRepository : IGameRepository
{
    private readonly IDataLink _dataLink;
    public GameRepository(IDataLink dataLink)
    {
        this._dataLink = dataLink;
    }

    public int CreateGame(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
                    new SqlParameter("@Name", game.Name),
                    new SqlParameter("@Description", game.Description),
                    new SqlParameter("@ImagePath", game.ImagePath),
                    new SqlParameter("@Price", game.Price),
                    new SqlParameter("@TrailerPath", game.TrailerPath),
                    new SqlParameter("GameplayPath", game.GameplayPath),
                    new SqlParameter("@MinimumRequirements", game.MinimumRequirements),
                    new SqlParameter("@RecommendedRequirements", game.RecommendedRequirements),
                    new SqlParameter("@Status", game.Status),
                    new SqlParameter("@Discount", game.Discount)
        };

        try
        {
            int? result = _dataLink.ExecuteScalar<int>("CreateGame", parameters);
            return result ?? 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 0;
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
            DataTable result = _dataLink.ExecuteReader("getGameTags", parameters);
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

    public float GetGameRating(int gameId)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
                    new SqlParameter("@gid", gameId)
        };

        try
        {
            float? result = _dataLink.ExecuteScalar<float>("getGameRating", parameters);
            return result ?? 0f;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting rating for game {gameId}: {e.Message}");
        }
    }

    public int GetNoOfRecentSalesForGame(int gameId)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
                    new SqlParameter("@gid", gameId)
        };

        try
        {
            int? result = _dataLink.ExecuteScalar<int>("getNoOfRecentSalesForGame", parameters);
            return result ?? 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting no. of recent purchases for game {gameId}: {e.Message}");
        }
    }

    public Collection<Game> getAllGames()
    {
        DataTable? result = _dataLink.ExecuteReader("GetAllGames");
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
                    TrailerPath = (string)row["trailer_url"],
                    GameplayPath = (string)row["gameplay_url"],
                    Price = Convert.ToDouble(row["price"]),
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Discount = (int)row["discount"],
                    Tags = GetGameTags((int)row["game_id"]),
                    Rating = GetGameRating((int)row["game_id"]),
                    noOfRecentPurchases = GetNoOfRecentSalesForGame((int)row["game_id"]),
                    trendingScore = Game.NOT_COMPUTED,
                    tagScore = Game.NOT_COMPUTED
                };
                games.Add(game);
            }
        }

        return new Collection<Game>(games);
    }

    public Collection<Tag> getAllTags()
    {
        DataTable? result = _dataLink.ExecuteReader("GetAllTags");
        List<Tag> tags = new List<Tag>();
        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Tag tag = new Tag
                {
                    tag_id = (int)row["tag_id"],
                    tag_name = (string)row["tag_name"],
                    no_of_user_games_with_tag = Tag.NOT_COMPUTED
                };
                tags.Add(tag);
            }
        }

        return new Collection<Tag>(tags);
    }
}
