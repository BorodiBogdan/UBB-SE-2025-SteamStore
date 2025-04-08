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
        return _cartRepository.GetCartGames();
    }
    public void RemoveGameFromCart(Game game)
    {
        _cartRepository.RemoveGameFromCart(game);
    }
    public void AddGameToCart(Game game)
    {
        try
        {
            _cartRepository.AddGameToCart(game);
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
            _cartRepository.RemoveGameFromCart(game);
        }
    }   

    public float getUserFunds()
    {
        return _cartRepository.GetUserFunds();
    }

    public decimal GetTotalSumToBePaid()
    {
        return _cartRepository.GetCartGames().Sum(game => (decimal)game.Price);
    }
}

