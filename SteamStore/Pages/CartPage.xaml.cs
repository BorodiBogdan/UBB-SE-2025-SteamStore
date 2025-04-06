using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.Linq;
using SteamStore.Services.Interfaces;

namespace SteamStore.Pages
{
    public sealed partial class CartPage : Page
    {
        private CartViewModel _viewModel;

        public CartPage(ICartService cartService, IUserGameService userGameService)
        {
            this.InitializeComponent();
            _viewModel = new CartViewModel(cartService, userGameService);
            this.DataContext = _viewModel;
        }

        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CartGames.Count > 0)
            {
                if (this.Parent is Frame frame)
                {
                    _viewModel.ChangeToPaymentPage(frame);
                }
            }
        }
    }
}