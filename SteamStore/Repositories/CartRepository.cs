using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;
using SteamStore.Constants;


public class CartRepository
{
    private readonly DataLink dataLink;
    private readonly User user;

    public CartRepository(DataLink dataLink, User user)
    {
        this.dataLink = dataLink;
        this.user = user;
    }
    public List<Game> getCartGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId)
        };

        DataTable? result = dataLink.ExecuteReader(SqlConstants.GetAllCartGames, parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row[SqlConstants.GameIdColumn],
                    Name = (string)row[SqlConstants.NameColumn],
                    Description = (string)row[SqlConstants.DescriptionColumn],
                    ImagePath = (string)row[SqlConstants.ImageUrlColumn],
                    Price = Convert.ToDouble(row[SqlConstants.PriceColumn]),
                    Status = "Approved"
                };
                games.Add(game);
            }
        }
        return games;
    }   
    public void addGameToCart(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            dataLink.ExecuteNonQuery(SqlConstants.AddGameToCart, parameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void removeGameFromCart(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            dataLink.ExecuteNonQuery(SqlConstants.RemoveGameFromCart, parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }   
    }
    public float getUserFunds()
    {
        return user.WalletBalance;
    }
}