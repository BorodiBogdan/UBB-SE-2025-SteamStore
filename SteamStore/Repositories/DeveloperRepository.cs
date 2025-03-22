using System.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Pages;
using SteamStore.Models;

public class DeveloperRepository
{
    private DataLink dataLink;
    private User user;

    public DeveloperRepository(DataLink dataLink, User user)
    {
        this.dataLink = dataLink;
        this.user = user;
    }
    public void ValidateGame(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id),
        };
        try
        {
            dataLink.ExecuteNonQuery("validateGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

    }
    public void CreateGame(Game game)
    {
        game.PublisherId = user.UserId;
        
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game.Id),
            new SqlParameter("@name", game.Name),
            new SqlParameter("@price", game.Price),
            new SqlParameter("@publisher_id", game.PublisherId),
            new SqlParameter("@description", game.Description),
            new SqlParameter("@image_url", game.ImagePath),
            new SqlParameter("@trailer_url", game.TrailerPath ?? ""),
            new SqlParameter("@gameplay_url", game.GameplayPath ?? ""),
            new SqlParameter("@minimum_requirements", game.MinimumRequirements),
            new SqlParameter("@recommended_requirements", game.RecommendedRequirements),
            new SqlParameter("@status", game.Status),
            new SqlParameter("@discount", game.Discount)
        };
        try
        {
            dataLink.ExecuteNonQuery("InsertGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public List<Game> GetUnvalidated()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@publisher_id", user.UserId)
        };

        DataTable? result = dataLink.ExecuteReader("GetAllUnvalidated", parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["game_id"],
                    Name = (string)row["name"],
                    Price = Convert.ToDouble(row["price"]),
                    Description = (string)row["description"],
                    ImagePath = (string)row["image_url"],
                    TrailerPath = (string)row["trailer_url"],
                    GameplayPath = (string)row["gameplay_url"],
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Discount = Convert.ToSingle(row["discount"]),
                    PublisherId = (int)row["publisher_id"]
                };
                games.Add(game);
            }
        }
        return games;
    }
    public void DeleteGame(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id)
        };
        try
        {
            dataLink.ExecuteNonQuery("DeleteGameDeveloper", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public List<Game> GetDeveloperGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@publisher_id", user.UserId)
        };
        DataTable? result = dataLink.ExecuteReader("GetDeveloperGames", parameters);
        List<Game> games = new List<Game>();
        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["game_id"],
                    Name = (string)row["name"],
                    Price = Convert.ToDouble(row["price"]),
                    Description = (string)row["description"],
                    ImagePath = (string)row["image_url"],
                    TrailerPath = (string)row["trailer_url"],
                    GameplayPath = (string)row["gameplay_url"],
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Discount = Convert.ToSingle(row["discount"]),
                    PublisherId = (int)row["publisher_id"]
                };
                games.Add(game);
            }
        }
        return games;
    }
    public void UpdateGame(int game_id, Game game)
    {
        game.PublisherId = user.UserId;
        
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id),
            new SqlParameter("@name", game.Name),
            new SqlParameter("@price", game.Price),
            new SqlParameter("@description", game.Description),
            new SqlParameter("@image_url", game.ImagePath),
            new SqlParameter("@trailer_url", game.TrailerPath),
            new SqlParameter("@gameplay_url", game.GameplayPath),
            new SqlParameter("@minimum_requirements", game.MinimumRequirements),
            new SqlParameter("@recommended_requirements", game.RecommendedRequirements),
            new SqlParameter("@status", game.Status),
            new SqlParameter("@discount", game.Discount),
            new SqlParameter("@publisher_id", game.PublisherId)
        };
        try
        {
            dataLink.ExecuteNonQuery("UpdateGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void RejectGame(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id)
        };
        try
        {
            dataLink.ExecuteNonQuery("RejectGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}