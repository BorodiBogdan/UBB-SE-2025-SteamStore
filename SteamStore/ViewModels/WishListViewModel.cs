using SteamStore.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public WishListViewModel(UserGameService userGameService, GameService gameService, CartService cartService)
        {
            _userGameService = userGameService;
            _gameService = gameService;
            _cartService = cartService;
            _wishListGames = new ObservableCollection<Game>();
            LoadWishListGames();
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 