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

        // Collections
        private ObservableCollection<PointShopItem> _shopItems;
        private ObservableCollection<PointShopItem> _userItems;

        // Filter properties
        private string _filterType = "All";
        private string _searchText = "";
        private double _minPrice = 0;
        private double _maxPrice = 10000;

        // Selected item
        private PointShopItem _selectedItem;

        private CancellationTokenSource _searchCancellationTokenSource;

        public PointShopViewModel(User currentUser, DataLink dataLink)
        {
            // Store the current user reference
            _user = currentUser;
            
            // Initialize service and collections
            _pointShopService = new PointShopService(currentUser, dataLink);
            ShopItems = new ObservableCollection<PointShopItem>();
            UserItems = new ObservableCollection<PointShopItem>();
            
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
                // Get all available items (filtering out owned items)
                var allItems = _pointShopService.GetAllItems();
                var userItems = _pointShopService.GetUserItems();
                
                // Filter out items the user already owns
                var availableItems = allItems.Where(item => 
                    !userItems.Any(userItem => userItem.ItemId == item.ItemId)).ToList();
                    
                var filteredItems = availableItems;

                // Apply type filter
                if (!string.IsNullOrEmpty(_filterType) && _filterType != "All")
                {
                    filteredItems = filteredItems.Where(item => 
                        item.ItemType.Equals(_filterType, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Apply price filter
                filteredItems = filteredItems.Where(item => 
                    item.PointPrice >= _minPrice && item.PointPrice <= _maxPrice)
                    .ToList();

                // Apply search filter if there's a search term
                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    filteredItems = filteredItems.Where(item => 
                        item.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) || 
                        item.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Update the observable collection
                ShopItems.Clear();
                foreach (var item in filteredItems)
                {
                    ShopItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error applying filters: {ex.Message}");
            }
        }

        private async void DelayedSearch(CancellationToken cancellationToken)
        {
            try
            {
                // Wait a bit before searching to avoid excessive updates while typing
                await Task.Delay(300, cancellationToken);
                
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 