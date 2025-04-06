using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SteamStore.Models;
using SteamStore.Services.Interfaces;
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
        private const int MIN_PRICE_FILTER_VALUE = 0;
        private const int MAX_PRICE_FILTER_VALUE = 200;
        private const int RATING_FILTER_VALUE = 0;

        private HomePageViewModel HomePageViewModel { get; set; }
        public HomePage(IGameService _gameService, ICartService _cartService, IUserGameService _userGameService)
        {
            this.InitializeComponent();
            HomePageViewModel = new HomePageViewModel(_gameService, _userGameService, _cartService);
            this.DataContext = HomePageViewModel;

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
            PopupRatingSlider.Value = RATING_FILTER_VALUE;
            MinPriceSlider.Value = MIN_PRICE_FILTER_VALUE;
            MaxPriceSlider.Value = MAX_PRICE_FILTER_VALUE;
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
                    viewModel.SwitchToGamePage(this.Parent, selectedGame);
                }
                
                // Clear selection
                listView.SelectedItem = null;
            }
        }

    }
}
