using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamStore.Pages
{
    public sealed partial class PaypalPaymentPage : Page
    {
        private CartService cartService;
        private UserGameService userGameService;
        private List<Game> purchasedGames;
        private PaypalProcessor paypalProcessor; 

        public PaypalPaymentPage(CartService cartService, UserGameService userGameService)
        {
            this.InitializeComponent();
            this.cartService = cartService;
            this.userGameService = userGameService;
            this.purchasedGames = cartService.getCartGames();
            this.paypalProcessor = new PaypalProcessor();
        }

        private async void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            decimal totalAmount = purchasedGames.Sum(game => (decimal)game.Price);

            bool paymentSuccess = await paypalProcessor.ProcessPaymentAsync(email, password, totalAmount);

            if (paymentSuccess)
            {
                cartService.RemoveGamesFromCart(purchasedGames);
                userGameService.purchaseGames(purchasedGames);

                await ShowNotification("Payment Successful", "Your purchase has been completed successfully.");

                if (this.Parent is Frame frame)
                {
                    frame.Content = new CartPage(cartService, userGameService);
                }

            }
            else
            {
                await ShowNotification("Payment Failed", "Please check your email and password.");
            }
        }

        private async Task ShowNotification(string title, string message)
        {
            NotificationDialog.Title = title;
            NotificationDialog.Content = message;
            await NotificationDialog.ShowAsync();
        }
        private void NotificationDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
        }
    }
}