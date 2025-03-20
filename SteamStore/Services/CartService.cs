using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CartService
{
    private CartRepository _cartRepository;

    public CartService(CartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public List<Game> getCartGames()
    {
        return _cartRepository.getCartGames();
    }
    public void RemoveGameFromCart(Game game)
    {
        _cartRepository.removeGameFromCart(game);
    }
    public void RemoveGamesFromCart(List<Game> games)
    {
        foreach (var game in games)
        {
            _cartRepository.removeGameFromCart(game);
        }
    }   
}

