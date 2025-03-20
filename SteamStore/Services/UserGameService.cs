using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UserGameService
{
    private UserGameRepository _userGameRepository;
    private GameRepository _gameRepository;

    public UserGameService(UserGameRepository userGameRepository)
    {
        _userGameRepository = userGameRepository;
    }
    public void removeGameFromWishlist(Game game)
    {
        _userGameRepository.removeGameFromWishlist(game);
    }

    public void addGameToWishlist(Game game)
    {
        try
        {
            _userGameRepository.addGameToWishlist(game);
        }
        catch (Exception e) { throw new Exception(e.Message); }
    }
    public void purchaseGames(List<Game> games)
    {
        {
            foreach (var game in games)
            {
                _userGameRepository.addGameToPurchased(game);
                _userGameRepository.removeGameFromWishlist(game);
            }
        }
    }

    public void computeNoOfUserGamesWithTag()
    {
        var user_games = _userGameRepository.getAllUserGames();
        Dictionary<string, Tag> tagsDictionary = _gameRepository.getAllTags()
            .ToDictionary(tag => tag.tag_name);
        foreach (var tag in tagsDictionary.Values)
        {
            tag.no_of_user_games_with_tag = 0;
        }
        foreach(var user_game in user_games)
        {
            foreach(string tag_name in user_game.Tags)
            {
                tagsDictionary[tag_name].no_of_user_games_with_tag++;
            }
        }
    }
}