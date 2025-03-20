using System.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Pages;
using SteamStore.Models;

public class GameValidatorRepository
{
    private DataLink dataLink;
    private User user;

    public GameValidatorRepository(DataLink dataLink, User user)
    {
        this.dataLink = dataLink;
        this.user = user;
    }
    public void ValidateGame(int game_id, bool isValid)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id),
        };
        try
        {
            dataLink.ExecuteNonQuery("ValidateGame", sqlParameters);
            isValid = true;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

    }
    public void CreateGame(Game game)
    {
        
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game.Id),
            new SqlParameter("@name", game.Name),
            new SqlParameter("@price", game.Price),
            new SqlParameter("@publisher_id", user.UserId),
            new SqlParameter("@description", game.Description),
            new SqlParameter("@image_url", game.ImagePath),
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
}