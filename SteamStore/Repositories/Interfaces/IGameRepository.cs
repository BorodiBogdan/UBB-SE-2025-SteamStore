using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Repositories.Interfaces
{
    public interface IGameRepository
    {
        int CreateGame(Game game);
        string[] GetGameTags(int gameId);
        float GetGameRating(int gameId);
        int GetNoOfRecentSalesForGame(int gameId);
        Collection<Game> getAllGames();
        Collection<Tag> getAllTags();
    }
}
