// <copyright file="PointsShopPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Microsoft.UI.Dispatching;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Media.Imaging;
    using Microsoft.UI.Xaml.Navigation;
    using SteamStore.Models;
    using SteamStore.Services;
    using SteamStore.Services.Interfaces;
    using SteamStore.ViewModels;
    using Windows.Foundation;
    using Windows.Foundation.Collections;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PointsShopPage : Page
    {
        public PointsShopPage(IPointShopService pointShopService)
        {
            this.InitializeComponent();

            try
            {
                // Initialize the ViewModel with the PointShopService
                this.ViewModel = new PointShopViewModel(pointShopService);
                this.DataContext = this.ViewModel;

                // Check for earned points
                if (this.ViewModel.ShouldShowPointsEarnedNotification())
                {
                    this.ShowPointsEarnedNotification(this.ViewModel.GetPointsEarnedMessage());
                    this.ViewModel.ResetEarnedPoints();
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error initializing PointsShopPage: {exception.Message}");
            }
        }

        private PointShopViewModel ViewModel { get; set; }

        public void ShowPointsEarnedNotification(string message)
        {
            this.NotificationText.Text = message;
            this.NotificationBar.Visibility = Visibility.Visible;

            var timer = this.DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(15);
            timer.Tick += (s, e) =>
            {
                this.NotificationBar.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        private void ItemsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel.HandleItemSelection())
            {
                this.ItemDetailPanel.Visibility = Visibility.Visible;
                var details = this.ViewModel.GetSelectedItemDetails();

                this.SelectedItemName.Text = details.Name;
                this.SelectedItemType.Text = details.Type;
                this.SelectedItemDescription.Text = details.Description;
                this.SelectedItemPrice.Text = details.Price;

                try
                {
                    if (!string.IsNullOrEmpty(details.ImageUri))
                    {
                        this.SelectedItemImage.Source = new BitmapImage(new Uri(details.ImageUri));
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Error loading image: {exception.Message}");
                }
            }
            else
            {
                this.ItemDetailPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseDetailButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the item detail panel and clear the selection
            this.ViewModel.ClearSelection();
        }

        private async void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = await this.ViewModel.TryPurchaseSelectedItemAsync();

            if (success)
            {
                this.ViewModel.ClearSelection();
                this.ItemDetailPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ViewInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.InventoryPanel.Visibility = Visibility.Visible;
        }

        private void CloseInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.InventoryPanel.Visibility = Visibility.Collapsed;
        }

        private async void RemoveButtons_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int itemId))
            {
                await this.ViewModel.ToggleActivationForItemWithMessage(itemId);
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
                if (this.ViewModel != null)
                {
                    this.ViewModel.FilterType = filterType;
                }
            }
        }

        private void CloseNotification_Click(object sender, RoutedEventArgs e)
        {
            this.NotificationBar.Visibility = Visibility.Collapsed;
        }

        private void ViewTransactionHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.TransactionHistoryPanel.Visibility = Visibility.Visible;
        }

        private void CloseTransactionHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.TransactionHistoryPanel.Visibility = Visibility.Collapsed;
        }
    }
}
