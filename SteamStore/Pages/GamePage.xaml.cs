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
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    public sealed partial class GamePage : Page
    {
        internal GamePageViewModel _viewModel;

        public GamePage()
        {
            this.InitializeComponent();
        }
        
        public GamePage(GameService gameService,CartService cartService, UserGameService userGameService, Game game)
        {
            this.InitializeComponent();
            _viewModel = new GamePageViewModel(gameService, cartService, userGameService);
            this.DataContext = _viewModel;
            
            if (game != null)
            {
                _viewModel.LoadGame(game);
                LoadGameUi();
            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                if (_viewModel == null)
                {
                    Debug.WriteLine("Error: Services not available");
                    return;
                }

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
            catch (Exception ex)
            {
                // Handle any errors
                Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
            }
        }
        
        public void LoadGameUi()
        {
            try
            {
                if (_viewModel.Game == null)
                {
                    Debug.WriteLine("Error: Game not found");
                    return;
                }
                
                GameTitle.Text = _viewModel.Game.Name;
                GamePrice.Text = $"${_viewModel.Game.Price:F2}";
                GameDescription.Text = _viewModel.Game.Description;

                GameDeveloper.Text = "Developer: " + _viewModel.Game.Name; 
                
                if (!string.IsNullOrEmpty(_viewModel.Game.ImagePath))
                {
                    try
                    {
                        GameImage.Source = new BitmapImage(new Uri(_viewModel.Game.ImagePath));
                    }
                    catch
                    {
                    }
                }
                
                MinimumRequirements.Text = _viewModel.Game.MinimumRequirements;
                RecommendedRequirements.Text = _viewModel.Game.RecommendedRequirements;
               
               
                AddMediaLinks(_viewModel.Game);
                
                LoadSimilarGamesUi();
                
                GameRating.Value = _viewModel.Game.Rating;
                OwnedStatus.Text = _viewModel.IsOwned ? "OWNED" : "NOT OWNED";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading game UI: {ex.Message}");
            }
        }
        
        private void AddMediaLinks(Game game)
        {
            MediaLinksPanel.Children.Clear();
         
            AddMediaLink("Official Trailer", game.TrailerPath);
            AddMediaLink("Gameplay Video", game.GameplayPath);
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
                SimilarGame1.Visibility = Visibility.Collapsed;
                SimilarGame2.Visibility = Visibility.Collapsed;
                SimilarGame3.Visibility = Visibility.Collapsed;
                
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
                Debug.WriteLine($"Error loading similar games: {ex.Message}");
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
                }
            }
            
            title.Text = game.Name;
            rating.Text = $"Rating: 4.0/5.0";
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.AddToCart();

                NotificationTip.Title = "Success";
                NotificationTip.Subtitle = $"{_viewModel.Game.Name} was added to your cart.";
                NotificationTip.IsOpen = true;
            }
            catch (Exception ex)
            {
                NotificationTip.Title = "Error";
                NotificationTip.Subtitle = $"Failed to add {_viewModel.Game.Name} to your cart";
                NotificationTip.IsOpen = true;
            }
        }

        private void WishlistButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.AddToWishlist();

                NotificationTip.Title = "Success";
                NotificationTip.Subtitle = $"{_viewModel.Game.Name} was added to your wishlist.";
                NotificationTip.IsOpen = true;
            }
            catch (Exception ex)
            {
                NotificationTip.Title = "Error";
                string errorMessage = ex.Message;
                if (errorMessage.Contains("ExecuteNonQuery"))
                {
                    errorMessage = $"Failed to add {_viewModel.Game.Name} to your wishlist: Already in wishlist";
                }
                NotificationTip.Subtitle = errorMessage;
                NotificationTip.IsOpen = true;
            }
        }

        private void SimilarGame_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Game game)
            {
                Frame frame = this.Parent as Frame;
                
                if (frame != null)
                {
                    var gamePage = new GamePage(_viewModel._gameService, _viewModel._cartService, _viewModel._userGameService, game);
                    
                    frame.Content = gamePage;
                   
                }
                else
                {
                    Frame.Navigate(typeof(GamePage), game);
                }
            }
        }
    }
}

