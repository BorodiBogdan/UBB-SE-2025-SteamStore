using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Repositories.Interfaces
{
    public interface IDeveloperRepository
    {
        void ValidateGame(int game_id);
        void CreateGame(Game game);
        List<Game> GetUnvalidated();
        void DeleteGameTags(int game_id);
        void DeleteGame(int game_id);
        List<Game> GetDeveloperGames();
        void UpdateGame(int game_id, Game game);
        void RejectGame(int game_id);
        void RejectGameWithMessage(int game_id, string message);
        string GetRejectionMessage(int game_id);
        Collection<Tag> GetAllTags();
        void InsertGameTag(int gameId, int tagId);
        bool IsGameIdInUse(int gameId);
        List<Tag> GetGameTags(int gameId);
        int GetGameOwnerCount(int game_id);
        User GetCurrentUser();
    }
}
