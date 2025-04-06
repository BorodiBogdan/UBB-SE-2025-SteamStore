using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Services.Interfaces
{
    public interface IDeveloperService
    {
        void ValidateGame(int game_id);
        Game ValidateInputForAddingAGame(string gameIdText, string name, string priceText, string description, string imageUrl, string trailerUrl, string gameplayUrl,
                                string minReq, string recReq, string discountText, IList<Tag> selectedTags);
        Game FindGameInObservableCollectionById(int gameId, ObservableCollection<Game> gameList);
        void CreateGame(Game game);
        void CreateGameWithTags(Game game, IList<Tag> selectedTags);
        void UpdateGame(Game game);
        void UpdateGameWithTags(Game game, IList<Tag> selectedTags);
        void DeleteGame(int game_id);
        List<Game> GetDeveloperGames();
        List<Game> GetUnvalidated();
        void RejectGame(int game_id);
        void RejectGameWithMessage(int game_id, string message);
        string GetRejectionMessage(int game_id);
        void InsertGameTag(int gameId, int tagId);
        Collection<Tag> GetAllTags();
        bool IsGameIdInUse(int gameId);
        List<Tag> GetGameTags(int gameId);
        void DeleteGameTags(int gameId);
        int GetGameOwnerCount(int game_id);
        User GetCurrentUser();
    }
}
