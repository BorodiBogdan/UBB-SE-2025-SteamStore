using SteamStore.Models;
using SteamStore.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Microsoft.UI.Xaml.Controls;

namespace SteamStore.ViewModels
{
    public class PointShopViewModel : INotifyPropertyChanged
    {
        private readonly PointShopService _pointShopService;
        private User _user;
        private const string FILTER_TYPE_ALL = "All";
        private const string INITIAL_SEARCH_STRING = "";
        private const int MIN_PRICE = 0;
        private const int MAX_PRICE = 10000;
        private const int TRANSACTION_ID = 1;
        private const double MINMAL_DIFFERENCE_VALUE_COMPARISON = 0.01;
        private const int DELAY_TIME_SEARCH = 300;

        // Collections
        private ObservableCollection<PointShopItem> _shopItems;
        private ObservableCollection<PointShopItem> _userItems;
        private ObservableCollection<PointShopTransaction> _transactionHistory;

        // Filter properties
        private string _filterType = FILTER_TYPE_ALL;
        private string _searchText = INITIAL_SEARCH_STRING;
        private double _minPrice = MIN_PRICE;
        private double _maxPrice = MAX_PRICE;

        // Selected item
        private PointShopItem _selectedItem;

        private CancellationTokenSource _searchCancellationTokenSource;
        private int _nextTransactionId = TRANSACTION_ID;

        public PointShopViewModel(User currentUser, DataLink dataLink)
        {
            // Store the current user reference
            _user = currentUser;
            
            // Initialize service and collections
            _pointShopService = new PointShopService(currentUser, dataLink);
            ShopItems = new ObservableCollection<PointShopItem>();
            UserItems = new ObservableCollection<PointShopItem>();
            TransactionHistory = new ObservableCollection<PointShopTransaction>();
            
            // Load initial data
            LoadItems();
            LoadUserItems();
        }

        public PointShopViewModel(PointShopService pointShopService)
        {
            // Initialize with existing service
            _pointShopService = pointShopService;
            
            // Get the user reference from the service's internal repository
            _user = _pointShopService.GetCurrentUser();
            
            // Initialize collections
            ShopItems = new ObservableCollection<PointShopItem>();
            UserItems = new ObservableCollection<PointShopItem>();
            TransactionHistory = new ObservableCollection<PointShopTransaction>();
            
            // Load initial data
            LoadItems();
            LoadUserItems();
        }

        public ObservableCollection<PointShopItem> ShopItems
        {
            get => _shopItems;
            set
            {
                _shopItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PointShopItem> UserItems
        {
            get => _userItems;
            set
            {
                _userItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PointShopTransaction> TransactionHistory
        {
            get => _transactionHistory;
            set
            {
                _transactionHistory = value;
                OnPropertyChanged();
            }
        }

        public string FilterType
        {
            get => _filterType;
            set
            {
                if (_filterType != value)
                {
                    _filterType = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    
                    // Cancel any existing search operation
                    _searchCancellationTokenSource?.Cancel();
                    _searchCancellationTokenSource = new CancellationTokenSource();
                    
                    // Apply search with a small delay to avoid too many updates
                    DelayedSearch(_searchCancellationTokenSource.Token);
                }
            }
        }

        public double MinPrice
        {
            get => _minPrice;
            set
            {
                if (_minPrice != value)
                {
                    _minPrice = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public double MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (_maxPrice != value)
                {
                    _maxPrice = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public PointShopItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanPurchase));
            }
        }

        public float UserPointBalance => _user?.PointsBalance ?? 0;

        public bool CanPurchase
        {
            get
            {
                if (SelectedItem == null || _user == null)
                    return false;

                // Check if user already owns this item
                bool alreadyOwns = UserItems.Any(item => item.ItemId == SelectedItem.ItemId);
                
                // Check if user has enough points
                bool hasEnoughPoints = _user.PointsBalance >= SelectedItem.PointPrice;
                
                return !alreadyOwns && hasEnoughPoints;
            }
        }

        public void LoadItems()
        {
            try
            {
                // Get all available items
                var allItems = _pointShopService.GetAllItems();
                
                // Get user's items to filter them out
                var userItems = _pointShopService.GetUserItems();
                
                // Filter out items the user already owns
                var availableItems = allItems.Where(item => 
                    !userItems.Any(userItem => userItem.ItemId == item.ItemId)).ToList();
                
                // Update the shop items
                ShopItems.Clear();
                foreach (var item in availableItems)
                {
                    ShopItems.Add(item);
                }
                
                ApplyFilters();
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error loading items: {ex.Message}");
            }
        }

        public void LoadUserItems()
        {
            try
            {
                var items = _pointShopService.GetUserItems();
                UserItems.Clear();
                foreach (var item in items)
                {
                    UserItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error loading user items: {ex.Message}");
            }
        }

        public async Task<bool> PurchaseSelectedItem()
        {
            if (SelectedItem == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot purchase: SelectedItem is null");
                return false;
            }

            try
            {
                // Store a local copy of the item to prevent issues after state changes
                var itemToPurchase = SelectedItem;
                
                _pointShopService.PurchaseItem(itemToPurchase);
                
                // Add transaction to history
                var transaction = new PointShopTransaction(
                    _nextTransactionId++, 
                    itemToPurchase.Name, 
                    itemToPurchase.PointPrice, 
                    itemToPurchase.ItemType);
                TransactionHistory.Add(transaction);
                
                // Point balance is updated in the repository
                OnPropertyChanged(nameof(UserPointBalance));
                OnPropertyChanged(nameof(CanPurchase));
                
                // Reload user items to show the new purchase
                LoadUserItems();
                
                // Reload shop items to remove the purchased item
                LoadItems();
                
                return true;
            }
            catch (Exception ex)
            {
                // Rethrow for handling in the UI
                throw new Exception($"Failed to purchase item: {ex.Message}", ex);
            }
        }

        public async Task<bool> ActivateItem(PointShopItem item)
        {
            if (item == null)
                return false;

            try
            {
                _pointShopService.ActivateItem(item);
                
                // Refresh user items to reflect the activation change
                LoadUserItems();
                
                return true;
            }
            catch (Exception ex)
            {
                // Rethrow for handling in the UI
                throw new Exception($"Failed to activate item: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeactivateItem(PointShopItem item)
        {
            if (item == null)
                return false;

            try
            {
                _pointShopService.DeactivateItem(item);
                
                // Refresh user items to reflect the deactivation change
                LoadUserItems();
                
                return true;
            }
            catch (Exception ex)
            {
                // Rethrow for handling in the UI
                throw new Exception($"Failed to deactivate item: {ex.Message}", ex);
            }
        }
        private void ApplyFilters()
        {
            try
            {
                var filteredItems = _pointShopService.GetFilteredItems(_filterType, _searchText, _minPrice, _maxPrice);
                ShopItems.Clear();
                foreach (var item in filteredItems) ShopItems.Add(item);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error applying filters: {ex.Message}"); }
        }

        private async void DelayedSearch(CancellationToken cancellationToken)
        {
            try
            {
                // Wait a bit before searching to avoid excessive updates while typing
                await Task.Delay(DELAY_TIME_SEARCH, cancellationToken);
                
                // Only apply if not cancelled
                if (!cancellationToken.IsCancellationRequested)
                {
                    ApplyFilters();
                }
            }
            catch (TaskCanceledException)
            {
                // Search was cancelled, do nothing
            }
        }
        public bool HandleItemSelection()
        {
            if (SelectedItem != null)
            {
                SelectedItemImageUri = SelectedItem.ImagePath;
                IsDetailPanelVisible = true;
            }
            else
            {
                IsDetailPanelVisible = false;
            }

            OnPropertyChanged(nameof(SelectedItemImageUri));
            OnPropertyChanged(nameof(IsDetailPanelVisible));
            return IsDetailPanelVisible;
        }
        public void ClearSelection()
        {
            SelectedItem = null;
            IsDetailPanelVisible = false;

            OnPropertyChanged(nameof(IsDetailPanelVisible));
        }
        public string SelectedItemImageUri { get; private set; }
        private bool _isDetailPanelVisible;
        public bool IsDetailPanelVisible
        {
            get => _isDetailPanelVisible;
            set
            {
                _isDetailPanelVisible = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public (string Name, string Type, string Description, string Price, string ImageUri) GetSelectedItemDetails()
        {
            try
            {
                if (SelectedItem == null)
                    return (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

                return (
                    Name: SelectedItem.Name,
                    Type: SelectedItem.ItemType,
                    Description: SelectedItem.Description,
                    Price: $"{SelectedItem.PointPrice} Points",
                    ImageUri: SelectedItem.ImagePath
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preparing item details: {ex.Message}");
                return (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            }
        }
        private async System.Threading.Tasks.Task ShowDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.m_window.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
        public async Task<bool> TryPurchaseSelectedItemAsync()
        {
            if (SelectedItem == null)
            {
                await ShowDialog("Error","No item selected");
                return false;
            }
                

            string itemName = SelectedItem.Name;
            double pointPrice = SelectedItem.PointPrice;
            string itemType = SelectedItem.ItemType;

            try
            {
                bool success = await PurchaseSelectedItem();

                if (success)
                {
                    TransactionHistory ??= new ObservableCollection<PointShopTransaction>();

                    bool transactionExists = TransactionHistory.Any(t =>
                        t.ItemName == itemName &&
                        Math.Abs(t.PointsSpent - pointPrice) < MINMAL_DIFFERENCE_VALUE_COMPARISON);

                    if (!transactionExists)
                    {
                        var transaction = new PointShopTransaction(
                            TransactionHistory.Count + 1,
                            itemName,
                            pointPrice,
                            itemType);
                        TransactionHistory.Add(transaction);
                    }

                    LoadUserItems();
                    LoadItems();

                    await ShowDialog("Congrats!", $"You have successfully purchased {itemName}. Check your inventory to view it.");
                    return true;
                }
                await ShowDialog("Error", "Purchase failed. Please try again.");
                return false;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Purchase Failed: {ex.Message}");
                return false;
            }
        }

        public async Task ToggleActivationForItemWithMessage(int itemId)
        {
            try
            {
                var item = UserItems.FirstOrDefault(i => i.ItemId == itemId);
                if (item == null)
                {
                    await ShowDialog("Item Not Found", "The selected item could not be found.");
                }

                if (item.IsActive)
                {
                    await DeactivateItem(item);
                    await ShowDialog("Item Deactivated", $"{item.Name} has been deactivated.");
                }
                else
                {
                    await ActivateItem(item);
                    await ShowDialog("Item Activated", $"{item.Name} has been activated.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"An error occurred while updating the item: {ex.Message}");
            }
        }
        public bool ShouldShowPointsEarnedNotification()
        {
            return Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue("RecentEarnedPoints", out object pointsObj)
                && pointsObj is int earnedPoints && earnedPoints > 0;
        }

        public string GetPointsEarnedMessage()
        {
            if (Microsoft.UI.Xaml.Application.Current.Resources["RecentEarnedPoints"] is int earnedPoints && earnedPoints > 0)
            {
                return $"You earned {earnedPoints} points from your recent purchase!";
            }
            return string.Empty;
        }

        public void ResetEarnedPoints()
        {
            Microsoft.UI.Xaml.Application.Current.Resources["RecentEarnedPoints"] = 0;
        }



        #endregion
    }
} 