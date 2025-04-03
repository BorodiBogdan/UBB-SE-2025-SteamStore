using SteamStore.Models;
using SteamStore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Xaml.Media;
using SteamStore.Pages;
using SteamStore;

namespace SteamStore.Pages
{
    public sealed partial class WishListView : Page
    {
        private WishListViewModel _viewModel;

        public WishListView(UserGameService userGameService, GameService gameService, CartService cartService)
        {
            this.InitializeComponent();
            _viewModel = new WishListViewModel(userGameService, gameService, cartService);
            this.DataContext = _viewModel;
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Game game)
            {
                if (this.Parent is Frame frame)
                {
                    _viewModel.ViewGameDetails(frame, game);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Frame frame)
            {
                _viewModel.BackToHomePage(frame);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.HandleSearchWishListGames();
            }
        }
    }
} 