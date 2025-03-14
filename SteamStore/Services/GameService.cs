using System;
using System.Collections.ObjectModel;

public class GameService
{
    private GameRepository _gameRepository;
    public GameService(GameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    public Collection<Game> getAllGames()
    {
        return _gameRepository.getAllGames();
    }
}