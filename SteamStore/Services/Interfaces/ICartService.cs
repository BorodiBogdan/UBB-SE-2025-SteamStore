using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Services.Interfaces
{
    public interface ICartService
    {
        List<Game> getCartGames();
        void RemoveGameFromCart(Game game);
        void AddGameToCart(Game game);
        void RemoveGamesFromCart(List<Game> games);
        float getUserFunds();
        decimal GetTotalSumToBePaid();
    }
}
