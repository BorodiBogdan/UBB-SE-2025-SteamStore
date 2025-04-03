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
using SteamStore.Models;
using SteamStore.Services;
using SteamStore.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;


namespace SteamStore.Pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PointsShopPage : Page
    {
        private PointShopViewModel ViewModel { get; set; }
        private PointShopService _pointShopService;

        public PointsShopPage(PointShopService pointShopService)
        {
            this.InitializeComponent();
            
            try
            {
                _pointShopService = pointShopService;
                
                // Initialize the ViewModel with the PointShopService
                ViewModel = new PointShopViewModel(_pointShopService);
                this.DataContext = ViewModel;

                // Check for earned points
                if (ViewModel.ShouldShowPointsEarnedNotification())
                {
                    ShowPointsEarnedNotification(ViewModel.GetPointsEarnedMessage());
                    ViewModel.ResetEarnedPoints();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing PointsShopPage: {ex.Message}");
                //ShowErrorDialog("Failed to initialize Points Shop", ex.Message);
            }
        }

        //private async void ShowErrorDialog(string title, string message)
        //{
        //    ContentDialog errorDialog = new ContentDialog();
        //    errorDialog.Title = title;
        //    errorDialog.Content = message;
        //    errorDialog.CloseButtonText = "OK";
        //    errorDialog.XamlRoot = this.XamlRoot;

        //    await errorDialog.ShowAsync();
        //}

        private void ItemsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.HandleItemSelection())
            {
                ItemDetailPanel.Visibility = Visibility.Visible;
                var details = ViewModel.GetSelectedItemDetails();

                SelectedItemName.Text = details.Name;
                SelectedItemType.Text = details.Type;
                SelectedItemDescription.Text = details.Description;
                SelectedItemPrice.Text = details.Price;

                try
                {
                    if (!string.IsNullOrEmpty(details.ImageUri))
                    {
                        SelectedItemImage.Source = new BitmapImage(new Uri(details.ImageUri));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading image: {ex.Message}");
                }
            }
            else
                ItemDetailPanel.Visibility = Visibility.Collapsed;
        }

        private void CloseDetailButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the item detail panel and clear the selection
            ViewModel.ClearSelection();
        }

        private async void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = await ViewModel.TryPurchaseSelectedItemAsync();

            if (success)
            {
                ViewModel.ClearSelection();
                ItemDetailPanel.Visibility = Visibility.Collapsed;
            }

        }

        private void ViewInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            InventoryPanel.Visibility = Visibility.Visible;
        }

        private void CloseInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            InventoryPanel.Visibility = Visibility.Collapsed;
        }

        private async void RemoveButtons_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int itemId))
            {
                await ViewModel.ToggleActivationForItemWithMessage(itemId);
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Set a default image when loading fails
            if (sender is Image img)
            {
                // Set a placeholder or default image
                img.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri("https://via.placeholder.com/200x200?text=Image+Not+Found"));
            }
        }

        private void ItemTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string filterType = selectedItem.Content.ToString();
                if (ViewModel != null)
                {
                    ViewModel.FilterType = filterType;
                }
            }
        }

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationBar.Visibility = Visibility.Collapsed;
        }

        public void ShowPointsEarnedNotification(string message)
        {
            NotificationText.Text = message;
            NotificationBar.Visibility = Visibility.Visible;

            var timer = DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(15);
            timer.Tick += (s, e) =>
            {
                NotificationBar.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        //private void CheckForEarnedPoints()
        //{
        //    try
        //    {
        //        if (Application.Current.Resources.TryGetValue("RecentEarnedPoints", out object pointsObj) && 
        //            pointsObj is int earnedPoints && 
        //            earnedPoints > 0)
        //        {
        //            // Show notification about recently earned points
        //            ShowPointsEarnedNotification(earnedPoints);
                    
        //            // Reset the value so it doesn't show again
        //            Application.Current.Resources["RecentEarnedPoints"] = 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Silent catch - don't let notification issues break the page
        //        Debug.WriteLine($"Error checking for earned points: {ex.Message}");
        //    }
        //}

        private void ViewTransactionHistoryButton_Click(object sender, RoutedEventArgs e)
        {

            TransactionHistoryPanel.Visibility = Visibility.Visible;
        }
         
        private void CloseTransactionHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionHistoryPanel.Visibility = Visibility.Collapsed;
        }
    }
}
