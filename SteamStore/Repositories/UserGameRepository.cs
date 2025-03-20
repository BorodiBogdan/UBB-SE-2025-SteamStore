using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

public class UserGameRepository
{
    private User user;
    private DataLink data;

    public UserGameRepository(DataLink data, User user)
    {
        this.user = user;
        this.data = data;
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
            Console.WriteLine(e.Message);
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}