using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Models;

namespace SteamStore.Repositories.Interfaces
{
    public interface IGameRepository
    {
        void CreateGame(Game game);
        Collection<Game> GetAllGames();

        List<Game> GetUnvalidated(int userUserId);
        void DeleteGameTags(int gameId);
        List<Game> GetDeveloperGames(int userUserId);
        void UpdateGame(int gameId, Game game);
        void RejectGame(int gameId);
        void RejectGameWithMessage(int gameId, string message);
        string GetRejectionMessage(int gameId);
        void InsertGameTag(int gameId, int tagId);
        bool IsGameIdInUse(int gameId);
        List<Tag> GetGameTags(int gameId);
        void ValidateGame(int gameId);
        void DeleteGame(int gameId);
    }
}