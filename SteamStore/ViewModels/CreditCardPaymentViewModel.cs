using Microsoft.UI.Xaml.Controls;
using SteamStore.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


using Microsoft.UI.Xaml;
using System.Runtime.CompilerServices;
using SteamStore.Constants;
using SteamStore.Services.Interfaces;

namespace SteamStore.ViewModels
{
    class CreditCardPaymentViewModel : INotifyPropertyChanged
    {
        private readonly ICartService _cartService;
        private readonly IUserGameService _userGameService;
        private readonly CreditCardProcessor _creditCardProcessor;
        private string _cardNumber;
        private string _expirationDate;
        private string _cvv;
        private string _ownerName;
        private decimal _totalAmount;
        private int _lastEarnedPoints;

        private const int ThresholdForNotEarningPoints = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        public string CardNumber
        {
            get => _cardNumber;
            set { _cardNumber = value; OnPropertyChanged(); }
        }

        public string ExpirationDate
        {
            get => _expirationDate;
            set { _expirationDate = value; OnPropertyChanged(); }
        }

        public string CVV
        {
            get => _cvv;
            set { _cvv = value; OnPropertyChanged(); }
        }

        public string OwnerName
        {
            get => _ownerName;
            set { _ownerName = value; OnPropertyChanged(); }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            private set { _totalAmount = value; OnPropertyChanged(); }
        }

        public int LastEarnedPoints
        {
            get => _lastEarnedPoints;
            private set { _lastEarnedPoints = value; OnPropertyChanged(); }
        }

        public ICommand ProcessPaymentCommand
        {
            get;
        }
        public CreditCardPaymentViewModel(ICartService cartService, IUserGameService userGameService)
        {
            _cartService = cartService;
            _userGameService = userGameService;
            _creditCardProcessor = new CreditCardProcessor();
            TotalAmount = _cartService.getCartGames().Sum(game => (decimal)game.Price);
           
        }
        public async Task ProcessPaymentAsync(Frame frame)
        {
            bool paymentSuccess = await _creditCardProcessor.ProcessPaymentAsync(_cardNumber, _expirationDate, _cvv, _ownerName);
            if (paymentSuccess)
            {
                List<Game> purchasedGames = _cartService.getCartGames();
                _cartService.RemoveGamesFromCart(purchasedGames);
                _userGameService.purchaseGames(purchasedGames);
                LastEarnedPoints = _userGameService.LastEarnedPoints;

            
             
                try
                {
                    Application.Current.Resources[ResourceKeys.RecentEarnedPoints] = LastEarnedPoints;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error storing points: {ex.Message}");
                }

                
                if (LastEarnedPoints > ThresholdForNotEarningPoints)
                {
                    await ShowNotification(PaymentDialogStrings.PAYMENTSUCCESSTITLE,
                        string.Format(PaymentDialogStrings.PAYMENTSUCCESSWITHPOINTSMESSAGE, LastEarnedPoints));
                }
                else
                {
                    await ShowNotification(PaymentDialogStrings.PAYMENTSUCCESSTITLE, PaymentDialogStrings.PAYMENTSUCCESSMESSAGE);
                }

                CartPage cartPage = new CartPage(_cartService, _userGameService);
                frame.Content = cartPage;
            }
            else
            {
                await ShowNotification(PaymentDialogStrings.PAYMENTFAILEDTITLE, PaymentDialogStrings.PAYMENTFAILEDMESSAGE);
            }

        }
        private async Task ShowNotification(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = PaymentDialogStrings.OKBUTTONTEXT,
                XamlRoot = App.m_window.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
       
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}

