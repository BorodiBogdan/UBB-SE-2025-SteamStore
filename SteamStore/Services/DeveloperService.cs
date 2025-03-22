using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamStore.Models;

public class DeveloperService
{
    private DeveloperRepository _developerRepository;
    public DeveloperService(DeveloperRepository developerRepository)
    {
        _developerRepository = developerRepository;
    }
    public void ValidateGame(int game_id)
    {
        _developerRepository.ValidateGame(game_id);
    }
    public void CreateGame(Game game)
    {
        try
        {
            _developerRepository.CreateGame(game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void UpdateGame(Game game)
    {
        try
        {
            _developerRepository.UpdateGame(game.Id,game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

    }
    public void DeleteGame(int game_id)
    {
        try
        {
            _developerRepository.DeleteGame(game_id);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public List<Game> GetDeveloperGames()
    {
        return _developerRepository.GetDeveloperGames();
    }
    public List<Game> GetUnvalidated()
    {
        return _developerRepository.GetUnvalidated();
    }
    public void RejectGame(int game_id)
    {
        _developerRepository.RejectGame(game_id);
    }
    public void InsertGameTag(int gameId, int tagId)
    {
        _developerRepository.InsertGameTag(gameId, tagId);
    }
    public Collection<Tag> GetAllTags()
    {
        return _developerRepository.GetAllTags();
    }
    public bool IsGameIdInUse(int gameId)
    {
        return _developerRepository.IsGameIdInUse(gameId);
    }
    public List<Tag> GetGameTags(int gameId)
    {
        return _developerRepository.GetGameTags(gameId);
    }
    public void DeleteGameTags(int gameId)
    {
        _developerRepository.DeleteGameTags(gameId);
    }
}
