using SteamStore.Repositories.Interfaces;
using SteamStore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CartService : ICartService
{
    private ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
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
    public void AddGameToCart(Game game)
    {
        try
        {
            _cartRepository.addGameToCart(game);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }   
    }
    public void RemoveGamesFromCart(List<Game> games)
    {
        foreach (var game in games)
        {
            _cartRepository.removeGameFromCart(game);
        }
    }   

    public float getUserFunds()
    {
        return _cartRepository.getUserFunds();
    }

    public decimal GetTotalSumToBePaid()
    {
        return _cartRepository.getCartGames().Sum(game => (decimal)game.Price);
    }
}

