using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UserGameService
{
    private UserGameRepository _userGameRepository;

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
}