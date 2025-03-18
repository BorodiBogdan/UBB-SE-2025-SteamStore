using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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
        public HomePage(GameService _gameService)
        {
            this.InitializeComponent();

            // Resolve dependencies (e.g., GameRepository and DataLink)


            this.DataContext = new HomePageViewModel(_gameService);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string user_input = SearchBox.Text.ToLower();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = true;
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            // You can access the filter values from PopupRatingSlider, MinPriceSlider, MaxPriceSlider here.
            double ratingFilter = PopupRatingSlider.Value;
            double minPrice = MinPriceSlider.Value;
            double maxPrice = MaxPriceSlider.Value;

            // Implement your filtering logic, e.g.,
            // viewModel.FilterGamesByRatingAndPrice(ratingFilter, minPrice, maxPrice);

            // Close the popup
            FilterPopup.IsOpen = false;
        }

        //Navigation to GamePage
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Frame.Navigate(typeof(GamePage));
        }
    }
}
