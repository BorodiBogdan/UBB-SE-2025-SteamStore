using System;
using System.Collections.ObjectModel;
using System.Linq;

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

    public Collection<Game> searchGames(String search_query)
    {
        return new Collection<Game>(_gameRepository
       .getAllGames()
       .Where(game => game.Name.ToLower().Contains(search_query.ToLower()))
       .ToList());
    }
}