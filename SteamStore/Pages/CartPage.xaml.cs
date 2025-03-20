using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SteamStore.Pages
{
    public sealed partial class CartPage : Page
    {
        private CartViewModel _viewModel;

        public CartPage(CartService cartService, UserGameService userGameService)
        {
            this.InitializeComponent();
            _viewModel = new CartViewModel(cartService, userGameService);
            this.DataContext = _viewModel;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Game game)
            {
                _viewModel.RemoveGameFromCart(game);

            }
        }

        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CartGames.Count > 0)
            {
                var selectedPaymentMethod = (PaymentMethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (this.Parent is Frame frame)
                {

                    if (selectedPaymentMethod == "PayPal")
                    {
                        PaypalPaymentPage paypalPaymentPage = new PaypalPaymentPage(_viewModel._cartService, _viewModel._userGameService);
                        frame.Content = paypalPaymentPage;

                    }
                    else
                        if(selectedPaymentMethod == "Credit Card")
                        {
                            CreditCardPaymentPage creditCardPaymentPage = new CreditCardPaymentPage(_viewModel._cartService, _viewModel._userGameService);
                            frame.Content = creditCardPaymentPage;
                        }
                    else
                    {
                        float totalPrice = _viewModel.CartGames.Sum(game => (float)game.Price);
                        float userFunds = _viewModel.showUserFunds();

                        if (userFunds < totalPrice)
                        {
                            await ShowErrorMessageAsync("Insufficient Funds", "You do not have enough funds in your Steam Wallet to complete this purchase.");
                            return; 
                        }

                        bool isConfirmed = await ShowConfirmationDialogAsync();
                        if (!isConfirmed)
                            return;
                        _viewModel.PurchaseGames();
                    }
                }
            }
        }
        private async Task<bool> ShowConfirmationDialogAsync()
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Confirm Purchase",
                Content = "Are you sure you want to proceed with the purchase using your Steam Wallet?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            confirmDialog.XamlRoot = this.Content.XamlRoot;

            ContentDialogResult result = await confirmDialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }
        private async Task ShowErrorMessageAsync(string title, string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };

            await errorDialog.ShowAsync();
        }
    }
}