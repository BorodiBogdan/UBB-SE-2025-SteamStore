using System;
using System.Collections.ObjectModel;
using System.Linq;

public class CartViewModel
{
    public ObservableCollection<Game> CartGames { get; set; }

    public decimal TotalPrice => (decimal)CartGames.Sum(game => (double)game.Price);
    
    // Property to track points earned in the last purchase
    public int LastEarnedPoints { get; private set; }

    public CartService _cartService;
    public UserGameService _userGameService;

    public CartViewModel(CartService cartService, UserGameService userGameService)
    {
        _cartService = cartService;
        _userGameService = userGameService;
        CartGames = new ObservableCollection<Game>();
        LastEarnedPoints = 0;
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
        
        // Get the points earned from the user game service
        LastEarnedPoints = _userGameService.LastEarnedPoints;
        
        _cartService.RemoveGamesFromCart(CartGames.ToList());
        CartGames.Clear();
    }
    public float showUserFunds()
    {
        return _cartService.getUserFunds();
    }   
}