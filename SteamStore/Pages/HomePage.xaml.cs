using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private UserGameService _userGameService;
        private CartService _cartService;

        public HomePage(GameService _gameService, CartService _cartService, UserGameService _userGameService)
        {
            this.InitializeComponent();

            this.DataContext = new HomePageViewModel(_gameService);
            this._userGameService = _userGameService;
            this._cartService = _cartService;

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string user_input = SearchBox.Text;
            if (this.DataContext is HomePageViewModel viewModel)
                viewModel.SearchGames(user_input);
            GameListView.UpdateLayout();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = true;
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            // You can access the filter values from PopupRatingSlider, MinPriceSlider, MaxPriceSlider here.
            int ratingFilter = ((int)PopupRatingSlider.Value);
            int minPrice = ((int)MinPriceSlider.Value);
            int maxPrice = ((int)MaxPriceSlider.Value);
            var selectedTags = TagListView.SelectedItems
            .Cast<Tag>() 
            .Select(tag => tag.tag_name)
            .ToList();

            if (this.DataContext is HomePageViewModel viewModel)
                viewModel.FilterGames(ratingFilter,minPrice,maxPrice,selectedTags.ToArray());

            // Close the popup
            FilterPopup.IsOpen = false;
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            PopupRatingSlider.Value = 0;
            MinPriceSlider.Value = 0;
            MaxPriceSlider.Value = 200;
            TagListView.SelectedItems.Clear();
            if (this.DataContext is HomePageViewModel viewModel)
                viewModel.LoadAllGames();
            FilterPopup.IsOpen = false;
        }


        //Navigation to GamePage
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is Game selectedGame)
            {
                // Get the services from DataContext
                if (this.DataContext is HomePageViewModel viewModel)
                {
                    // Get game service from viewModel
                    var gameService = viewModel.GameService;
                    var cartService = _cartService;
                    var userGameService = _userGameService;
                    
                    // Instead of trying to find MainWindow, navigate directly
                    // We'll pass the game as navigation parameter
                    if (this.Parent is Frame frame)
                    {
                        // Create the GamePage with just GameService (no CartService yet)
                        var gamePage = new GamePage(gameService, cartService, userGameService, selectedGame);
                        
                        // Set it as content
                        frame.Content = gamePage;

                    }
                }
                
                // Clear selection
                listView.SelectedItem = null;
            }
        }

    }
}
