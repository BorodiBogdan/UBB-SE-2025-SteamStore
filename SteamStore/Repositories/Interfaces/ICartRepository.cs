using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Repositories.Interfaces
{
    public interface ICartRepository
    {
        List<Game> getCartGames();
        void addGameToCart(Game game);
        void removeGameFromCart(Game game);

        float getUserFunds();
    }
}
