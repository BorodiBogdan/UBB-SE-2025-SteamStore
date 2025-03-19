using System;
using System.Collections.ObjectModel;
using System.Linq;

public class CartViewModel
{
    public ObservableCollection<Game> CartGames { get; set; }

    public decimal TotalPrice => (decimal)CartGames.Sum(game => (double)game.Price);

    private readonly CartService _cartService;
    private readonly UserGameService _userGameService;

    public CartViewModel(CartService cartService, UserGameService userGameService)
    {
        _cartService = cartService;
        _userGameService = userGameService;
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
    public void PurchaseGames()
    {
        _userGameService.purchaseGames(CartGames.ToList());
        _cartService.RemoveGamesFromCart(CartGames.ToList());
        CartGames.Clear();
    }
}