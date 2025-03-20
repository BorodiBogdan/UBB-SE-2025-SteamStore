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
                _viewModel.PurchaseGames();
            }
        }
    }
}