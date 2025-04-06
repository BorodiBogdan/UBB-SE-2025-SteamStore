using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Services.Interfaces
{
    public interface IUserGameService
    {
        int LastEarnedPoints { get; }
        void removeGameFromWishlist(Game game);
        void addGameToWishlist(Game game);
        void purchaseGames(List<Game> games);
        void computeNoOfUserGamesForEachTag(Collection<Tag> all_tags);
        Collection<Tag> getFavoriteUserTags();
        void computeTagScoreForGames(Collection<Game> games);
        void computeTrendingScores(Collection<Game> games);
        Collection<Game> getRecommendedGames();
        Collection<Game> getWishListGames();
        Collection<Game> searchWishListByName(string searchText);
        Collection<Game> filterWishListGames(string criteria);
        bool isGamePurchased(Game game);
        Collection<Game> sortWishListGames(string criteria, bool ascending);

    }
}
