using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Repositories.Interfaces
{
    public interface IUserGameRepository
    {
        bool isGamePurchased(Game game);
        void removeGameFromWishlist(Game game);
        void addGameToPurchased(Game game);
        void addGameToWishlist(Game game);
        string[] GetGameTags(int gameId);
        Collection<Game> getAllUserGames();
        void AddPointsForPurchase(float purchaseAmount);
        float GetUserPointsBalance();
        Collection<Game> getWishlistGames();

        int GetGameOwnerCount(int gameId);
    }
}
