using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CartGames.Count > 0)
            {
                // Check the selected payment method
                var selectedPaymentMethod = (PaymentMethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (this.Parent is Frame frame)
                {

                    if (selectedPaymentMethod == "PayPal")
                    {
                        // Navigate to the PayPal payment page
                        PaypalPaymentPage paypalPaymentPage = new PaypalPaymentPage(_viewModel._cartService, _viewModel._userGameService);
                        frame.Content = paypalPaymentPage;

                    }
                    else
                    {
                        // Handle other payment methods (e.g., Steam Wallet or Credit Card)
                        _viewModel.PurchaseGames();
                    }
                }
            }
        }
    }
}