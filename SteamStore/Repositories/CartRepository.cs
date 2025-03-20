using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;


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

        DataTable? result = dataLink.ExecuteReader("GetAllCartGames", parameters);
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
                    Status = "Available"
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
            dataLink.ExecuteNonQuery("AddGameToCart", parameters);
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
            dataLink.ExecuteNonQuery("RemoveGameFromCart", parameters);
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