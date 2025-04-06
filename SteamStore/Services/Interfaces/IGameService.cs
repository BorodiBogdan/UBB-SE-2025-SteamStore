using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Services.Interfaces
{
    public interface IGameService
    {
        Collection<Game> getAllGames();
        Collection<Tag> getAllTags();
        Collection<Tag> getAllGameTags(Game game);
        Collection<Game> searchGames(String search_query);
        Collection<Game> filterGames(int minRating, int minPrice, int maxPrice, String[] Tags);
        void computeTrendingScores(Collection<Game> games);
        Collection<Game> getTrendingGames();
        Collection<Game> getDiscountedGames();
        List<Game> GetSimilarGames(int gameId);
    }
}
