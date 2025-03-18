using System;
using System.Collections.ObjectModel;
using System.Linq;

public class CartViewModel
{
    public ObservableCollection<Game> CartGames { get; set; }

    public decimal TotalPrice => (decimal)CartGames.Sum(game => (double)game.Price);

    private readonly CartService _cartService;

    public CartViewModel(CartService cartService)
    {
        _cartService = cartService;
        CartGames = new ObservableCollection<Game>();
        LoadGames();
    }

    private void LoadGames()
    {
        var games = _cartService.getCartGames();
        foreach (var game in games)
        {
            CartGames.Add(game);
        }
    }
    public void RemoveGameFromCart(Game game)
    {
        _cartService.RemoveGameFromCart(game);
        CartGames.Remove(game);
    }
}