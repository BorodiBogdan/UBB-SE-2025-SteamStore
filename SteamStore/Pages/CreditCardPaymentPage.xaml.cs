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
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreditCardPaymentPage : Page
    {
        private CartService cartService;
        private UserGameService userGameService;
        private List<Game> purchasedGames;

        private CreditCardProcessor creditCardProcessor; 

        public CreditCardPaymentPage(CartService cartService, UserGameService userGameService)
        {
            this.InitializeComponent();
            this.cartService = cartService;
            this.userGameService = userGameService;
            this.purchasedGames = cartService.getCartGames();
            this.creditCardProcessor = new CreditCardProcessor(); 
        }

        private async void ProcessPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string cardNumber = CardNumberTextBox.Text;
            string expirationDate = ExpirationDateTextBox.Text;
            string cvv = CvvTextBox.Text;
            string ownerName = OwnerNameTextBox.Text;
            decimal totalAmount = purchasedGames.Sum(game => (decimal)game.Price);

            bool paymentSuccess = await creditCardProcessor.ProcessPaymentAsync(cardNumber, expirationDate, cvv, ownerName);

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
                await ShowNotification("Payment Failed", "Please check your credit card details.");
            }
        }

        private async Task ShowNotification(string title, string message)
        {
            NotificationDialog.Title = title;
            NotificationMessageTextBlock.Text = message;
            await NotificationDialog.ShowAsync();
        }

        private void NotificationDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
        }
    }
}
