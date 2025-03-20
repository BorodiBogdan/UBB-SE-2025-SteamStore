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
using SteamStore.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PointsShopPage : Page
    {
        private PointShopViewModel ViewModel { get; set; }
        private User _currentUser;
        private DataLink _dataLink;

        public PointsShopPage()
        {
            this.InitializeComponent();
            
            // Configure the DataLink using ConfigurationBuilder
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            var configuration = configBuilder.Build();
            
            try
            {
                // Create a temporary user for testing - this would normally come from a login service
                _currentUser = new User(
                    userId: 1,
                    name: "TestUser",
                    email: "test@example.com",
                    walletBalance: 1000,
                    pointsBalance: 5000,
                    userRole: User.Role.User
                );
                
                // Initialize the DataLink with the entire configuration object
                _dataLink = new DataLink(configuration);
                
                // Initialize the ViewModel with the current user
                ViewModel = new PointShopViewModel(_currentUser, _dataLink);
                this.DataContext = ViewModel;
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Failed to initialize PointsShopPage", ex.Message);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                // If a user is passed through navigation, use it
                if (e.Parameter is User user)
                {
                    _currentUser = user;
                    ViewModel = new PointShopViewModel(_currentUser, _dataLink);
                    this.DataContext = ViewModel;
                }
                
                // Refresh data when navigating to the page
                ViewModel.LoadItems();
                ViewModel.LoadUserItems();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Failed to load data", ex.Message);
            }
        }

        private async void ShowErrorDialog(string title, string message)
        {
            ContentDialog errorDialog = new ContentDialog();
            errorDialog.Title = title;
            errorDialog.Content = message;
            errorDialog.CloseButtonText = "OK";
            errorDialog.XamlRoot = this.XamlRoot;

            await errorDialog.ShowAsync();
        }

        private void ItemsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                // Update the selected item details panel
                SelectedItemName.Text = ViewModel.SelectedItem.Name;
                SelectedItemType.Text = ViewModel.SelectedItem.ItemType;
                SelectedItemDescription.Text = ViewModel.SelectedItem.Description;
                SelectedItemPrice.Text = $"{ViewModel.SelectedItem.PointPrice} Points";
                
                // Try to load the image
                try
                {
                    // Support both local and web images
                    SelectedItemImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(ViewModel.SelectedItem.ImagePath));
                }
                catch (Exception ex)
                {
                    // If image fails to load, handle it
                    System.Diagnostics.Debug.WriteLine($"Failed to load image: {ex.Message}");
                }
                
                // Show the detail panel
                ItemDetailPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide the detail panel if no item is selected
                ItemDetailPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseDetailButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the item detail panel and clear the selection
            ItemDetailPanel.Visibility = Visibility.Collapsed;
            ItemsGridView.SelectedItem = null;
            ViewModel.SelectedItem = null;
        }

        private async void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Store the item name before the purchase
                if (ViewModel.SelectedItem == null)
                {
                    ShowErrorDialog("Purchase Failed", "No item selected");
                    return;
                }
                
                string itemName = ViewModel.SelectedItem.Name;
                
                bool success = await ViewModel.PurchaseSelectedItem();
                
                if (success)
                {
                    ContentDialog successDialog = new ContentDialog();
                    successDialog.Title = "Purchase Successful";
                    successDialog.Content = $"You have successfully purchased {itemName}. Check your inventory to view it.";
                    successDialog.CloseButtonText = "OK";
                    successDialog.XamlRoot = this.XamlRoot;
                    
                    await successDialog.ShowAsync();
                    
                    // Close the detail panel
                    ItemDetailPanel.Visibility = Visibility.Collapsed;
                    
                    // Refresh the shop and inventory
                    ViewModel.LoadItems();
                    ViewModel.LoadUserItems();
                    
                    // Reset selection
                    ItemsGridView.SelectedItem = null;
                    ViewModel.SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Purchase Failed", ex.Message);
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
            Button button = sender as Button;
            if (button == null) return;

            try
            {
                // Get the item from the button's tag 
                int itemId = Convert.ToInt32(button.Tag);
                var item = ViewModel.UserItems.FirstOrDefault(i => i.ItemId == itemId);

                if (item != null)
                {
                    // Check if item is active
                    if (item.IsActive)
                    {
                        // Deactivate the item
                        await ViewModel.DeactivateItem(item);

                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Item Deactivated";
                        dialog.Content = $"{item.Name} has been deactivated.";
                        dialog.CloseButtonText = "OK";
                        dialog.XamlRoot = this.XamlRoot;
                        
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        // Activate the item
                        await ViewModel.ActivateItem(item);

                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Item Activated";
                        dialog.Content = $"{item.Name} has been activated.";
                        dialog.CloseButtonText = "OK";
                        dialog.XamlRoot = this.XamlRoot;
                        
                        await dialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", ex.Message);
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
    }
}
