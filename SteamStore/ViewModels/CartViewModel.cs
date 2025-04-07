using System;
using System.Collections.ObjectModel;
using System.Linq; 
using System.ComponentModel;
using System.Runtime.CompilerServices;

using System.Windows.Input;
using SteamStore.Pages;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using SteamStore;
using SteamStore.Constants;

using System.Threading.Tasks;
using SteamStore.ViewModels;
using SteamStore.Services.Interfaces;


public class CartViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Game> _cartGames;
    private decimal _totalPrice;
    private string _selectedPaymentMethod;


    private const int ThresholdForNotEarningPoints = 0;


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

    public string SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set
        {
            _selectedPaymentMethod = value;
            OnPropertyChanged();
        }
    }

    // Property to track points earned in the last purchase
    public int LastEarnedPoints { get; private set; }

    public ICartService _cartService;
    public IUserGameService _userGameService;
    public ICommand RemoveGameCommand { get; }
    public ICommand CheckoutCommand { get; }

    public CartViewModel(ICartService cartService, IUserGameService userGameService)
    {
        _cartService = cartService;
        _userGameService = userGameService;
        CartGames = new ObservableCollection<Game>();
        LastEarnedPoints = 0;
        LoadGames();
        //Initialize commands
        RemoveGameCommand = new RelayCommand<Game>(RemoveGameFromCart);
       // CheckoutCommand = new RelayCommand<XamlRoot>(async (xamlRoot) => await CheckoutAsync(xamlRoot));

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
        OnPropertyChanged(nameof(CartGames));
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

    public async void ChangeToPaymentPage(Frame frame)
    {
        if (SelectedPaymentMethod == PaymentMethods.PAYPAL_PAYMENT_METHOD)
        {
            PaypalPaymentPage paypalPaymentPage = new PaypalPaymentPage(_cartService, _userGameService);
            frame.Content = paypalPaymentPage;
        }
        else if (SelectedPaymentMethod == PaymentMethods.CREDIT_CARD_PAYMENT_METHOD)
        {
            CreditCardPaymentPage creditCardPaymentPage = new CreditCardPaymentPage(_cartService, _userGameService);
            frame.Content = creditCardPaymentPage;
        }
        else if(SelectedPaymentMethod == PaymentMethods.STEAM_WALLET_PAYMENT_METHOD)
        {
            float totalPrice = CartGames.Sum(game => (float)game.Price);
            float userFunds = showUserFunds();
            if ( userFunds < totalPrice)
            {
                await ShowDialog(InsufficientFundsErrors.INSUFFICIENT_FUNDS_ERROR_TITLE, InsufficientFundsErrors.INSUFFICIENT_FUNDS_ERROR_MESSAGE);
            }
            bool isConfirmed = await ShowConfirmationDialogAsync();
            if (!isConfirmed)
                return;

            PurchaseGames();
            if (LastEarnedPoints > ThresholdForNotEarningPoints)
            {
                // Store the points in App resources for PointsShopPage to access
                try
                {
                    Application.Current.Resources[ResourceKeys.RecentEarnedPoints] = LastEarnedPoints;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error storing points: {ex.Message}");
                }

                await ShowPointsEarnedDialogAsync(LastEarnedPoints);
            }
        }



    }

    
    private async System.Threading.Tasks.Task ShowDialog(string title, string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = DialogStrings.OK_BUTTON_TEXT,
            XamlRoot = App.m_window.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
    private async Task<bool> ShowConfirmationDialogAsync()
    {
        ContentDialog confirmDialog = new ContentDialog
        {
            Title = DialogStrings.CONFIRM_PURCHASE_TITLE,
            Content = DialogStrings.CONFIRM_PURCHASE_MESSAGE,
            PrimaryButtonText = DialogStrings.YES_BUTTON_TEXT,
            CloseButtonText = DialogStrings.NO_BUTTON_TEXT,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = App.m_window.Content.XamlRoot
        };

        

        ContentDialogResult result = await confirmDialog.ShowAsync();

        return result == ContentDialogResult.Primary;
    }

    private async Task ShowPointsEarnedDialogAsync(int pointsEarned)
    {
        ContentDialog pointsDialog = new ContentDialog
        {
            Title = DialogStrings.POINTS_EARNED_TITLE,
            Content = string.Format(DialogStrings.POINTS_EARNED_MESSAGE, pointsEarned),
            CloseButtonText = DialogStrings.OK_BUTTON_TEXT,
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.m_window.Content.XamlRoot
        };

        await pointsDialog.ShowAsync();
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

    //public async Task CheckoutAsync(XamlRoot xamlRoot)
    //{
    //    if (CartGames.Count == 0)
    //        return;

    //    if (SelectedPaymentMethod == "PayPal")
    //    {
    //        NavigateToPaymentPage(new PaypalPaymentPage(_cartService, _userGameService));
    //    }
    //    else if (SelectedPaymentMethod == "Credit Card")
    //    {
    //        NavigateToPaymentPage(new CreditCardPaymentPage(_cartService, _userGameService));
    //    }
    //    else
    //    {
    //        float totalPrice = CartGames.Sum(game => (float)game.Price);
    //        float userFunds = showUserFunds();

    //        if (userFunds < totalPrice)
    //        {
    //            await ShowErrorMessageAsync("Insufficient Funds", "You do not have enough funds in your Steam Wallet to complete this purchase.");
    //            return;
    //        }

    //        bool isConfirmed = await ShowConfirmationDialogAsync(xamlRoot);
    //        if (!isConfirmed)
    //            return;

    //        PurchaseGames();

    //        if (LastEarnedPoints > 0)
    //        {
    //            try
    //            {
    //                Application.Current.Resources["RecentEarnedPoints"] = LastEarnedPoints;
    //            }
    //            catch (Exception ex)
    //            {
    //                System.Diagnostics.Debug.WriteLine($"Error storing points: {ex.Message}");
    //            }

    //            await ShowPointsEarnedDialogAsync(LastEarnedPoints,xamlRoot);
    //        }
    //    }
    //}
    //public async Task<string> CheckoutAsync(string selectedPaymentMethod, XamlRoot xamlRoot)
    //{
    //    if (CartGames.Count == 0)
    //        return "Your cart is empty."; // ✅ Return error message to the View

    //    if (selectedPaymentMethod == "PayPal")
    //    {
    //        NavigateToPaymentPage(new PaypalPaymentPage(_cartService, _userGameService));
    //        return null; // ✅ No error
    //    }
    //    else if (selectedPaymentMethod == "Credit Card")
    //    {
    //        NavigateToPaymentPage(new CreditCardPaymentPage(_cartService, _userGameService));
    //        return null;
    //    }

    //    float totalPrice = CartGames.Sum(game => (float)game.Price);
    //    float userFunds = showUserFunds();

    //    if (userFunds < totalPrice)
    //        return "Insufficient Funds: You do not have enough funds in your Steam Wallet to complete this purchase.";

    //    bool isConfirmed = await ShowConfirmationDialogAsync(xamlRoot);
    //    if (!isConfirmed)
    //        return "Purchase was canceled.";

    //    PurchaseGames();

    //    if (LastEarnedPoints > 0)
    //    {
    //        try
    //        {
    //            Application.Current.Resources["RecentEarnedPoints"] = LastEarnedPoints;
    //        }
    //        catch (Exception ex)
    //        {
    //            System.Diagnostics.Debug.WriteLine($"Error storing points: {ex.Message}");
    //        }

    //        await ShowPointsEarnedDialogAsync(LastEarnedPoints, xamlRoot);
    //    }

    //    return null; 
    //}




    //private void NavigateToPaymentPage(Page paymentPage)
    //{
    //   if (App.MainWindow.Content is Frame rootFrame)
    //{
    //    rootFrame.Navigate(paymentPage.GetType(), null);
    //        PrimaryButtonText = "Yes",
    //        CloseButtonText = "No",
    //        DefaultButton = ContentDialogButton.Primary,
    //        XamlRoot = xamlRoot
    //    };

    //    return await confirmDialog.ShowAsync() == ContentDialogResult.Primary;
    //}

    //private async Task ShowErrorMessageAsync(string title, string message)
    //{
    //    ContentDialog errorDialog = new ContentDialog
    //    {
    //        Title = title,
    //        Content = message,
    //        CloseButtonText = "OK",
    //        DefaultButton = ContentDialogButton.Close
    //    };

    //    await errorDialog.ShowAsync();
    //}

    //private async Task ShowPointsEarnedDialogAsync(int pointsEarned,XamlRoot xamlRoot)
    //{
    //    ContentDialog pointsDialog = new ContentDialog
    //    {
    //        Title = "Points Earned!",
    //        Content = $"You earned {pointsEarned} points for your purchase!\nVisit the Points Shop to spend your points on exclusive items.",
    //        CloseButtonText = "OK",
    //        DefaultButton = ContentDialogButton.Close,
    //        XamlRoot = xamlRoot
    //    };

    //    await pointsDialog.ShowAsync();
    //}


}