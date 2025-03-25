using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class CartViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Game> _cartGames;
    private decimal _totalPrice;

    public ObservableCollection<Game> CartGames
    {
        get => _cartGames;
        set
        {
            _cartGames = value;
            OnPropertyChanged();
            UpdateTotalPrice();
        }
    }

    public decimal TotalPrice
    {
        get => _totalPrice;
        private set
        {
            if (_totalPrice != value)
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }
    }

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
        UpdateTotalPrice();
    }

    private void UpdateTotalPrice()
    {
        TotalPrice = (decimal)CartGames.Sum(game => (double)game.Price);
    }

    public void RemoveGameFromCart(Game game)
    {
        _cartService.RemoveGameFromCart(game);
        CartGames.Remove(game);
        UpdateTotalPrice();
    }

    public void PurchaseGames()
    {
        _userGameService.purchaseGames(CartGames.ToList());

        // Get the points earned from the user game service
        LastEarnedPoints = _userGameService.LastEarnedPoints;

        _cartService.RemoveGamesFromCart(CartGames.ToList());
        CartGames.Clear();
        UpdateTotalPrice();
    }

    public float showUserFunds()
    {
        return _cartService.getUserFunds();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}