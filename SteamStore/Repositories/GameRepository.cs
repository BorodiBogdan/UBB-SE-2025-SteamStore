using System.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Pages;
using SteamStore.Models;


public class GameRepository
{
    private readonly DataLink dataLink;
    public GameRepository(DataLink dataLink)
    {
        this.dataLink = dataLink;
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
            //test on main
        };

        try
        {
            int? result = dataLink.ExecuteScalar<int>("CreateGame", parameters);
            return result ?? 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 0;
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
            DataTable result = dataLink.ExecuteReader("getGameTags", parameters);
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

    private float GetGameRating(int gameId)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@gid", gameId)
        };

        try
        {
            float? result = dataLink.ExecuteScalar<float>("getGameRating", parameters);
            return result ?? 0f;
        }
        catch (Exception e) { 
            throw new Exception($"Error getting rating for game {gameId}: {e.Message}");
        }
    }
    public Collection<Game> getAllGames()
    {

        DataTable? result = dataLink.ExecuteReader("GetAllGames");
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
                    Tags = GetGameTags((int)row["game_id"]),
                    Rating = GetGameRating((int)row["game_id"]) // trb fixata asta????
                };
                games.Add(game);
            }
        }
        
        return new Collection<Game>(games);
    }

    public Collection<Tag> getAllTags() {
        DataTable? result = dataLink.ExecuteReader("GetAllTags");
        List<Tag> tags = new List<Tag>();
           if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Tag tag = new Tag
                {
                    tag_id = (int)row["tag_id"],
                    tag_name = (string)row["tag_name"]
                };
                tags.Add(tag);
            }
        }

        return new Collection<Tag>(tags);
    }
}