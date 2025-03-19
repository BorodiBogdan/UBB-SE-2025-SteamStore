using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SteamStore.Pages
{
    public sealed partial class CartPage : Page
    {
        private CartViewModel _viewModel;

        public CartPage(CartService cartService)
        {
            this.InitializeComponent();
            _viewModel = new CartViewModel(cartService);
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
            // Handle the checkout logic here
            // For example, navigate to a checkout page or show a confirmation dialog
        }
    }
}