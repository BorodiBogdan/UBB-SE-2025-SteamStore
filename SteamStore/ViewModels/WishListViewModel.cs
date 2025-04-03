using Microsoft.UI.Xaml.Controls;
using SteamStore.Models;
using SteamStore.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SteamStore.ViewModels
{
    public class WishListViewModel : INotifyPropertyChanged
    {
        private readonly UserGameService _userGameService;
        private readonly GameService _gameService;
        private readonly CartService _cartService;
        private ObservableCollection<Game> _wishListGames = new ObservableCollection<Game>();
        private string _searchText = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Game> WishListGames
        {
            get => _wishListGames;
            set
            {
                _wishListGames = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                HandleSearchWishListGames();
            }
        }

        // Expose services for navigation
        public GameService GameService => _gameService;
        public CartService CartService => _cartService;
        public UserGameService UserGameService => _userGameService;

        public ICommand RemoveFromWishlistCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand BackCommand { get; }

        public List<string> FilterOptions { get; } = new()
    {
        "All Games", "Overwhelmingly Positive (4.5+★)", "Very Positive (4-4.5★)",
        "Mixed (2-4★)", "Negative (<2★)"
    };

        public List<string> SortOptions { get; } = new()
    {
        "Price (Low to High)", "Price (High to Low)", "Rating (High to Low)", "Discount (High to Low)"
    };

        private string _selectedFilter;
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                _selectedFilter = value;
                OnPropertyChanged();
                HandleFilterChange();
            }
        }

        private string _selectedSort;
        public string SelectedSort
        {
            get => _selectedSort;
            set
            {
                _selectedSort = value;
                OnPropertyChanged();
                HandleSortChange();
            }
        }

        public WishListViewModel(UserGameService userGameService, GameService gameService, CartService cartService)
        {
            _userGameService = userGameService;
            _gameService = gameService;
            _cartService = cartService;
            _wishListGames = new ObservableCollection<Game>();
            LoadWishListGames();
            RemoveFromWishlistCommand = new RelayCommand<Game>(async (game) => await ConfirmAndRemoveFromWishlist(game));
        }
        private void HandleFilterChange()
        {
            string criteria = SelectedFilter switch
            {
                "Overwhelmingly Positive (4.5+★)" => "overwhelmingly_positive",
                "Very Positive (4-4.5★)" => "very_positive",
                "Mixed (2-4★)" => "mixed",
                "Negative (<2★)" => "negative",
                _ => "all"
            };

            FilterWishListGames(criteria);
        }

        private void HandleSortChange()
        {
            switch (SelectedSort)
            {
                case "Price (Low to High)":
                    SortWishListGames("price", true); break;
                case "Price (High to Low)":
                    SortWishListGames("price", false); break;
                case "Rating (High to Low)":
                    SortWishListGames("rating", false); break;
                case "Discount (High to Low)":
                    SortWishListGames("discount", false); break;
            }
        }

        private void LoadWishListGames()
        {
            try
            {
                var games = _userGameService.getWishListGames();
                WishListGames = new ObservableCollection<Game>(games);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                Console.WriteLine($"Error loading wishlist games: {ex.Message}");
            }
        }

        public void HandleSearchWishListGames()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadWishListGames();
                return;
            }

            try
            {
                var games = _userGameService.searchWishListByName(SearchText);
                WishListGames = new ObservableCollection<Game>(games);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                Console.WriteLine($"Error searching wishlist games: {ex.Message}");
            }
        }

        public void FilterWishListGames(string criteria)
        {
            try
            {
                var games = _userGameService.filterWishListGames(criteria);
                WishListGames = new ObservableCollection<Game>(games);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                Console.WriteLine($"Error filtering wishlist games: {ex.Message}");
            }
        }

        public void SortWishListGames(string criteria, bool ascending)
        {
            try
            {
                var games = _userGameService.sortWishListGames(criteria, ascending);
                WishListGames = new ObservableCollection<Game>(games);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                Console.WriteLine($"Error sorting wishlist games: {ex.Message}");
            }
        }

        public void RemoveFromWishlist(Game game)
        {
            try
            {
                _userGameService.removeGameFromWishlist(game);
                WishListGames.Remove(game);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                Console.WriteLine($"Error removing game from wishlist: {ex.Message}");
            }
        }
        private async Task ConfirmAndRemoveFromWishlist(Game game)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirm Removal",
                    Content = $"Are you sure you want to remove {game.Name} from your wishlist?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.m_window.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    RemoveFromWishlist(game);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void BackToHomePage(Frame frame)
        {
            HomePage homePage = new HomePage(GameService,CartService,UserGameService);
            frame.Content = homePage;
        }

        public void ViewGameDetails(Frame frame, Game game)
        {
            GamePage gamePage = new GamePage(GameService, CartService, UserGameService, game);
            frame.Content = gamePage;
        }
    }
} 