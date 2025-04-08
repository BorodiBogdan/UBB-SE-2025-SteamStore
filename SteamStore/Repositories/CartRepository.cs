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

        DataTable? result = _dataLink.ExecuteReader(SqlConstants.GETALLCARTGAMES, parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row[SqlConstants.GAMEIDCOLUMN],
                    Name = (string)row[SqlConstants.NAMECOLUMN ],
                    Description = (string)row[SqlConstants.DESCRIPTIONCOLUMN],
                    ImagePath = (string)row[SqlConstants.IMAGEURLCOLUMN],
                    Price = Convert.ToDecimal(row[SqlConstants.PRICECOLUMN]),
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
            _dataLink.ExecuteNonQuery(SqlConstants.ADDGAMETOCART, parameters);
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
            _dataLink.ExecuteNonQuery(SqlConstants.REMOVEGAMEFROMCART, parameters);
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