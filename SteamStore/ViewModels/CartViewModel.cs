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
        var games = _cartService.GetCartGames();
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
        _userGameService.PurchaseGames(CartGames.ToList());

        // Get the points earned from the user game service
        LastEarnedPoints = _userGameService.LastEarnedPoints;

        _cartService.RemoveGamesFromCart(CartGames.ToList());
        CartGames.Clear();
        UpdateTotalPrice();
    }

    public async void ChangeToPaymentPage(Frame frame)
    {
        if (SelectedPaymentMethod == PaymentMethods.PAYPALPAYMENTMETHOD)
        {
            PaypalPaymentPage paypalPaymentPage = new PaypalPaymentPage(_cartService, _userGameService);
            frame.Content = paypalPaymentPage;
        }
        else if (SelectedPaymentMethod == PaymentMethods.CREDITCARDPAYMENTMETHOD)
        {
            CreditCardPaymentPage creditCardPaymentPage = new CreditCardPaymentPage(_cartService, _userGameService);
            frame.Content = creditCardPaymentPage;
        }
        else if(SelectedPaymentMethod == PaymentMethods.STEAMWALLETPAYMENTMETHOD)
        {
            float totalPrice = CartGames.Sum(game => (float)game.Price);
            float userFunds = showUserFunds();
            if ( userFunds < totalPrice)
            {
                await ShowDialog(InsufficientFundsErrors.INSUFFICIENTFUNDSERRORTITLE, InsufficientFundsErrors.INSUFFICIENTFUNDSERRORMESSAGE);
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
            CloseButtonText = DialogStrings.OKBUTTONTEXT,
            XamlRoot = App.m_window.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
    private async Task<bool> ShowConfirmationDialogAsync()
    {
        ContentDialog confirmDialog = new ContentDialog
        {
            Title = DialogStrings.CONFIRMPURCHASETITLE,
            Content = DialogStrings.CONFIRMPURCHASEMESSAGE,
            PrimaryButtonText = DialogStrings.YESBUTTONTEXT,
            CloseButtonText = DialogStrings.NOBUTTONTEXT,
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
            Title = DialogStrings.POINTSEARNEDTITLE,
            Content = string.Format(DialogStrings.POINTSEARNEDMESSAGE, pointsEarned),
            CloseButtonText = DialogStrings.OKBUTTONTEXT,
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.m_window.Content.XamlRoot
        };

        await pointsDialog.ShowAsync();
    }



    public float showUserFunds()
    {
        return _cartService.GetUserFunds();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    


}