using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;
using SteamStore.Constants;
using SteamStore.Data;
using SteamStore.Repositories.Interfaces;


public class CartRepository: ICartRepository
{
    private readonly IDataLink _dataLink;
    private readonly User _user;

    public CartRepository(IDataLink dataLink, User user)
    {
        this._dataLink = dataLink;
        this._user = user;
    }
    public List<Game> getCartGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@user_id", _user.UserId)
        };

        DataTable? result = _dataLink.ExecuteReader(SqlConstants.GetAllCartGames, parameters);
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
            new SqlParameter("@user_id", _user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            _dataLink.ExecuteNonQuery(SqlConstants.AddGameToCart, parameters);
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
            new SqlParameter("@user_id", _user.UserId),
            new SqlParameter("@game_id", game.Id)
        };

        try
        {
            _dataLink.ExecuteNonQuery(SqlConstants.RemoveGameFromCart, parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }   
    }
    public float getUserFunds()
    {
        return _user.WalletBalance;
    }
}