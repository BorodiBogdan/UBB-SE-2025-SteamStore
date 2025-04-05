using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamStore.Pages;

namespace SteamStore.ViewModels
{
    class PaypalPaymentViewModel:INotifyPropertyChanged
    {
        private CartService _cartService;
        private UserGameService _userGameService;
        private List<Game> _purchasedGames;
        private PaypalProcessor _paypalProcessor;
        private decimal _amountToPay;
        private string _email;
        private string _password;


        public event PropertyChangedEventHandler PropertyChanged;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public PaypalPaymentViewModel(CartService cartService, UserGameService userGameService)
        {
            this._cartService = cartService;
            this._userGameService = userGameService;
            _purchasedGames = cartService.getCartGames();
            _paypalProcessor = new PaypalProcessor();
            _amountToPay = cartService.GetTotalSumToBePaid();
        }

        public async Task ValidatePayment(Frame frame)
        {
            bool paymentSuccess = await _paypalProcessor.ProcessPaymentAsync(Email, Password, _amountToPay);
            if (paymentSuccess) 
            {
                _cartService.RemoveGamesFromCart(_purchasedGames);
                _userGameService.purchaseGames(_purchasedGames);

                // Get points earned from the purchase
                int pointsEarned = _userGameService.LastEarnedPoints;

                // Store points in App resources for PointsShopPage to access
                try
                {
                    Application.Current.Resources["RecentEarnedPoints"] = pointsEarned;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error storing points: {ex.Message}");
                }

                // Show points earned notification if points were earned
                if (pointsEarned > 0)
                {
                    await ShowNotification("Payment Successful", $"Your purchase has been completed successfully. You earned {pointsEarned} points!");
                }
                else
                {
                    await ShowNotification("Payment Successful", "Your purchase has been completed successfully.");
                }
                frame.Content = new CartPage(_cartService, _userGameService);
            }
            else
            {
                await ShowNotification("Payment Failed", "Please check your email and password.");
            }
        }

        private async Task ShowNotification(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
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
