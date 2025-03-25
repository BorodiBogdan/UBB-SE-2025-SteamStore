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

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedItem == null) return;

            var selectedItem = ((ComboBoxItem)FilterComboBox.SelectedItem).Content.ToString();
            string criteria = selectedItem switch
            {
                "All Games" => "all",
                "Overwhelmingly Positive (4.5+★)" => "overwhelmingly_positive",
                "Very Positive (4-4.5★)" => "very_positive",
                "Mixed (2-4★)" => "mixed",
                "Negative (<2★)" => "negative",
                _ => "all"
            };

            _viewModel.FilterWishListGames(criteria);
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem == null) return;

            var selectedItem = ((ComboBoxItem)SortComboBox.SelectedItem).Content.ToString();
            switch (selectedItem)
            {
                case "Price (Low to High)":
                    _viewModel.SortWishListGames("price", true);
                    break;
                case "Price (High to Low)":
                    _viewModel.SortWishListGames("price", false);
                    break;
                case "Rating (High to Low)":
                    _viewModel.SortWishListGames("rating", false);
                    break;
                case "Discount (High to Low)":
                    _viewModel.SortWishListGames("discount", false);
                    break;
            }
        }

        private async void RemoveFromWishlist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var game = (Game)button.DataContext;
                
                var dialog = new ContentDialog
                {
                    Title = "Confirm Removal",
                    Content = $"Are you sure you want to remove {game.Name} from your wishlist?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    XamlRoot = Content.XamlRoot,
                    DefaultButton = ContentDialogButton.Primary
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    _viewModel.RemoveFromWishlist(game);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur during dialog display
                Console.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Game game)
            {
                if (this.Parent is Frame frame)
                {
                    var gamePage = new GamePage(_viewModel.GameService, _viewModel.CartService, _viewModel.UserGameService, game);
                    frame.Content = gamePage;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Frame frame)
            {
                frame.Content = new HomePage(_viewModel.GameService, _viewModel.CartService, _viewModel.UserGameService);
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