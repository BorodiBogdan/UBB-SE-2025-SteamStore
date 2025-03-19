using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    public sealed partial class GamePage : Page
    {
        // Make viewModel internal so MainWindow can access it
        internal GamePageViewModel _viewModel;

        public GamePage()
        {
            this.InitializeComponent();
        }
        
        // Constructor that accepts services and game
        public GamePage(GameService gameService, CartService cartService, Game game)
        {
            this.InitializeComponent();
            _viewModel = new GamePageViewModel(gameService, cartService);
            this.DataContext = _viewModel;
            
            // Load the game directly
            if (game != null)
            {
                _viewModel.LoadGame(game);
                LoadGameUi();
            }
        }
        
        // Constructor that only accepts services
        public GamePage(GameService gameService, CartService cartService)
        {
            this.InitializeComponent();
            _viewModel = new GamePageViewModel(gameService, cartService);
            this.DataContext = _viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If our ViewModel is already initialized (through constructor), use it
            if (_viewModel == null)
            {
                // TODO: Implement proper service injection when frame navigating
                // For now, we'll just log an error and return
                System.Diagnostics.Debug.WriteLine("Error: Services not available");
                return;
            }

            // Handle navigation parameters
            if (e.Parameter is Game selectedGame)
            {
                _viewModel.LoadGame(selectedGame);
                LoadGameUi();
            }
            else if (e.Parameter is int gameId)
            {
                _viewModel.LoadGameById(gameId);
                LoadGameUi();
            }
        }
        
        // Make LoadGameUi public so MainWindow can call it
        public void LoadGameUi()
        {
            try
            {
                // Ensure we have a game to display
                if (_viewModel.Game == null)
                {
                    // Game not found, just log and return
                    System.Diagnostics.Debug.WriteLine("Error: Game not found");
                    return;
                }
                
                // Display basic game info
                GameTitle.Text = _viewModel.Game.Name;
                GamePrice.Text = $"${_viewModel.Game.Price:F2}";
                GameDescription.Text = _viewModel.Game.Description;
                
                // Set game developer (using Name since we don't have a dedicated field)
                GameDeveloper.Text = "Developer: " + _viewModel.Game.Name; 
                
                // Set image if available
                if (!string.IsNullOrEmpty(_viewModel.Game.ImagePath))
                {
                    try
                    {
                        GameImage.Source = new BitmapImage(new Uri(_viewModel.Game.ImagePath));
                    }
                    catch
                    {
                        // Handle image loading error silently
                    }
                }
                
                // Set system requirements
                MinimumRequirements.Text = _viewModel.Game.MinimumRequirements;
                RecommendedRequirements.Text = _viewModel.Game.RecommendedRequirements;
                
                // Add sample media links
                AddMediaLinks();
                
                // Load similar games from ViewModel
                LoadSimilarGamesUi();
                
                // Set a default rating for now
                GameRating.Value = 4.5;
            }
            catch (Exception ex)
            {
                // Just log the error instead of showing a dialog
                System.Diagnostics.Debug.WriteLine($"Error loading game UI: {ex.Message}");
            }
        }
        
        private void AddMediaLinks()
        {
            // Clear existing links
            MediaLinksPanel.Children.Clear();
            
            // Add sample media links
            AddMediaLink("Official Trailer", "https://www.youtube.com");
            AddMediaLink("Gameplay Video", "https://www.youtube.com");
        }
        
        private void AddMediaLink(string title, string url)
        {
            HyperlinkButton link = new HyperlinkButton
            {
                Content = title,
                NavigateUri = new Uri(url)
            };
            MediaLinksPanel.Children.Add(link);
        }
        
        private void LoadSimilarGamesUi()
        {
            try
            {
                // Hide all similar game buttons initially
                SimilarGame1.Visibility = Visibility.Collapsed;
                SimilarGame2.Visibility = Visibility.Collapsed;
                SimilarGame3.Visibility = Visibility.Collapsed;
                
                // Display available similar games from ViewModel
                var similarGames = _viewModel.SimilarGames.ToList();
                for (int i = 0; i < similarGames.Count; i++)
                {
                    if (i == 0) DisplaySimilarGame(SimilarGame1, SimilarGame1Image, SimilarGame1Title, SimilarGame1Rating, similarGames[i]);
                    if (i == 1) DisplaySimilarGame(SimilarGame2, SimilarGame2Image, SimilarGame2Title, SimilarGame2Rating, similarGames[i]);
                    if (i == 2) DisplaySimilarGame(SimilarGame3, SimilarGame3Image, SimilarGame3Title, SimilarGame3Rating, similarGames[i]);
                }
            }
            catch (Exception ex)
            {
                // Just log the error
                System.Diagnostics.Debug.WriteLine($"Error loading similar games: {ex.Message}");
            }
        }
        
        private void DisplaySimilarGame(Button button, Image image, TextBlock title, TextBlock rating, Game game)
        {
            button.Visibility = Visibility.Visible;
            button.Tag = game;
            
            if (!string.IsNullOrEmpty(game.ImagePath))
            {
                try
                {
                    image.Source = new BitmapImage(new Uri(game.ImagePath));
                }
                catch
                {
                    // Handle image loading error silently
                }
            }
            
            title.Text = game.Name;
            rating.Text = $"Rating: 4.0/5.0"; // Placeholder - would come from reviews
        }
        
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Cart functionality is not implemented yet
            // Just log that the button was clicked
            System.Diagnostics.Debug.WriteLine($"Add to cart clicked for game: {_viewModel.Game?.Name}");
            
            // Uncomment this when cart functionality is ready
            //_viewModel.AddToCart();
        }
        
        private void WishlistButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Wishlist functionality is not implemented yet
            // Just log that the button was clicked
            System.Diagnostics.Debug.WriteLine($"Add to wishlist clicked for game: {_viewModel.Game?.Name}");
            
            // Uncomment this when wishlist functionality is ready
            //_viewModel.AddToWishlist();
        }
        
        private void SimilarGame_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Game game)
            {
                // Get the parent frame (this should be the main content frame)
                Frame frame = this.Parent as Frame;
                
                if (frame != null)
                {
                    // Create a new GamePage passing just the GameService (CartService might be null)
                    var gamePage = new GamePage(_viewModel._gameService, null);
                    
                    // Set it as the content of the frame
                    frame.Content = gamePage;
                    
                    // Now load the game
                    gamePage._viewModel.LoadGame(game);
                    gamePage.LoadGameUi();
                }
                else
                {
                    // Fallback to standard navigation
                    Frame.Navigate(typeof(GamePage), game);
                }
            }
        }
    }
}

