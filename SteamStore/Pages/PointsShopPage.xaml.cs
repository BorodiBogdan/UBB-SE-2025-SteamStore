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
                CheckForEarnedPoints();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing PointsShopPage: {ex.Message}");
                ShowErrorDialog("Failed to initialize Points Shop", ex.Message);
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
                    Debug.WriteLine($"Failed to load image: {ex.Message}");
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
                double pointPrice = ViewModel.SelectedItem.PointPrice;
                string itemType = ViewModel.SelectedItem.ItemType;
                
                bool success = await ViewModel.PurchaseSelectedItem();
                
                if (success)
                {
                    // Make sure the transaction was added (fallback)
                    if (ViewModel.TransactionHistory == null)
                    {
                        ViewModel.TransactionHistory = new ObservableCollection<PointShopTransaction>();
                    }
                    
                    // Check if we need to manually add a transaction (should already be added in the ViewModel)
                    bool transactionExists = ViewModel.TransactionHistory.Any(t => t.ItemName == itemName && Math.Abs(t.PointsSpent - pointPrice) < 0.01);
                    
                    if (!transactionExists)
                    {
                        Debug.WriteLine("Transaction wasn't added in ViewModel - adding it manually");
                        var transaction = new PointShopTransaction(
                            ViewModel.TransactionHistory.Count + 1,
                            itemName,
                            pointPrice,
                            itemType);
                        ViewModel.TransactionHistory.Add(transaction);
                    }
                    
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

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationBar.Visibility = Visibility.Collapsed;
        }

        public void ShowPointsEarnedNotification(int pointsEarned)
        {
            try
            {
                // Update notification text
                NotificationText.Text = $"You earned {pointsEarned} points from your recent purchase!";
                
                // Show notification
                NotificationBar.Visibility = Visibility.Visible;
                
                // Auto-hide after 15 seconds using DispatcherQueue
                DispatcherQueue.TryEnqueue(() => 
                {
                    try 
                    {
                        var timer = this.DispatcherQueue.CreateTimer();
                        timer.Interval = TimeSpan.FromSeconds(15);
                        timer.Tick += (s, e) => 
                        {
                            NotificationBar.Visibility = Visibility.Collapsed;
                            timer.Stop();
                        };
                        timer.Start();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error setting up timer: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing notification: {ex.Message}");
            }
        }

        private void CheckForEarnedPoints()
        {
            try
            {
                if (Application.Current.Resources.TryGetValue("RecentEarnedPoints", out object pointsObj) && 
                    pointsObj is int earnedPoints && 
                    earnedPoints > 0)
                {
                    // Show notification about recently earned points
                    ShowPointsEarnedNotification(earnedPoints);
                    
                    // Reset the value so it doesn't show again
                    Application.Current.Resources["RecentEarnedPoints"] = 0;
                }
            }
            catch (Exception ex)
            {
                // Silent catch - don't let notification issues break the page
                Debug.WriteLine($"Error checking for earned points: {ex.Message}");
            }
        }

        private void ViewTransactionHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Debug the transaction history
            int transactionCount = ViewModel?.TransactionHistory?.Count ?? 0;
            Debug.WriteLine($"Transaction history count: {transactionCount}");
            
            if (ViewModel?.TransactionHistory != null)
            {
                foreach (var transaction in ViewModel.TransactionHistory)
                {
                    Debug.WriteLine($"Transaction: {transaction.ItemName}, {transaction.PointsSpent} points, {transaction.PurchaseDate}");
                }
            }
            
            TransactionHistoryPanel.Visibility = Visibility.Visible;
        }

        private void CloseTransactionHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionHistoryPanel.Visibility = Visibility.Collapsed;
        }
    }
}
