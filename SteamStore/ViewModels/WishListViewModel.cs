using Microsoft.UI.Xaml.Controls;
using SteamStore.Models;
using SteamStore.Pages;
using SteamStore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamStore.Constants;
using SteamStore.Constants.SteamStore.Constants;

namespace SteamStore.ViewModels
{
    public class WishListViewModel : INotifyPropertyChanged
    {
     

        private readonly IUserGameService _userGameService;
        private readonly IGameService _gameService;
        private readonly ICartService _cartService;
        private ObservableCollection<Game> _wishListGames = new ObservableCollection<Game>();
        private string _searchText = WishListSearchStrings.INITIALSEARCHSTRING;

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
        public IGameService GameService => _gameService;
        public ICartService CartService => _cartService;
        public IUserGameService UserGameService => _userGameService;

        public ICommand RemoveFromWishlistCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand BackCommand { get; }

        public List<string> FilterOptions { get; } = new()
    {
        WishListSearchStrings.FILTERALL, WishListSearchStrings.FILTEROVERWHELMINGLYPOSITIVE, WishListSearchStrings.FILTERVERYPOSITIVE,
        WishListSearchStrings.FILTERMIXED, WishListSearchStrings.FILTERNEGATIVE
    };

        public List<string> SortOptions { get; } = new()
    {
        WishListSearchStrings.SORTPRICEASCENDING, WishListSearchStrings.SORTPRICEDESCENDING, WishListSearchStrings.SORTRATINGDESCENDING, WishListSearchStrings.SORTDISCOUNTDESCENDING
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

        public WishListViewModel(IUserGameService userGameService, IGameService gameService, ICartService cartService)
        {
            _userGameService = userGameService;
            _gameService = gameService;
            _cartService = cartService;
            _wishListGames = new ObservableCollection<Game>();
            RemoveFromWishlistCommand = new RelayCommand<Game>(async (game) => await ConfirmAndRemoveFromWishlist(game));
            LoadWishListGames();
            
        }
        private void HandleFilterChange()
        {
            string criteria = SelectedFilter switch
            {
                WishListSearchStrings.FILTEROVERWHELMINGLYPOSITIVE => WishListSearchStrings.OVERWHELMINGLYPOSITIVE,
                WishListSearchStrings.FILTERVERYPOSITIVE => WishListSearchStrings.VERYPOSITIVE,
                WishListSearchStrings.FILTERMIXED => WishListSearchStrings.MIXED,
                WishListSearchStrings.FILTERNEGATIVE => WishListSearchStrings.NEGATIVE,
                _ => WishListSearchStrings.ALL
            };

            FilterWishListGames(criteria);
        }

        private void HandleSortChange()
        {
            switch (SelectedSort)
            {
                case WishListSearchStrings.SORTPRICEASCENDING:
                    SortWishListGames("price", true); break;
                case WishListSearchStrings.SORTPRICEDESCENDING:
                    SortWishListGames("price", false); break;
                case WishListSearchStrings.SORTRATINGDESCENDING:
                    SortWishListGames("rating", false); break;
                case WishListSearchStrings.SORTDISCOUNTDESCENDING:
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
                    Title = ConfirmationDialogStrings.CONFIRMREMOVALTITLE,
                    Content = string.Format(ConfirmationDialogStrings.CONFIRMREMOVALMESSAGE, game.Name),
                    PrimaryButtonText = ConfirmationDialogStrings.YESBUTTONTEXT,
                    CloseButtonText = ConfirmationDialogStrings.NOBUTTONTEXT,
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