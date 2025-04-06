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
using SteamStore.Constants;
using SteamStore.Services.Interfaces;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    public sealed partial class GamePage : Page
    {
        internal GamePageViewModel _viewModel;
        private const int MaxNumberOfSimilarGamesToDisplay = 3;
        public GamePage()
        {
            this.InitializeComponent();
        }
        
        public GamePage(IGameService gameService,ICartService cartService, IUserGameService userGameService, Game game)
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

                GameDeveloper.Text = LabelStrings.DeveloperPrefix + _viewModel.Game.Name; 
                
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
                OwnedStatus.Text = _viewModel.IsOwned ? LabelStrings.Owned : LabelStrings.NotOwned;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading game UI: {ex.Message}");
            }
        }
        
        private void AddMediaLinks(Game game)
        {
            MediaLinksPanel.Children.Clear();
         
            AddMediaLink(MediaLinkLabels.OfficialTrailer, game.TrailerPath);
            AddMediaLink(MediaLinkLabels.GameplayVideo, game.GameplayPath);
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
                var similarGameButtons = new[] { SimilarGame1, SimilarGame2, SimilarGame3 };
                var similarGameImages = new[] { SimilarGame1Image, SimilarGame2Image, SimilarGame3Image };
                var similarGameTitles = new[] { SimilarGame1Title, SimilarGame2Title, SimilarGame3Title };
                var similarGameRatings = new[] { SimilarGame1Rating, SimilarGame2Rating, SimilarGame3Rating };

                var similarGames = _viewModel.SimilarGames.ToList();
                for (int similarGameIndex = 0; similarGameIndex < similarGames.Count; similarGameIndex++)
                {
                    DisplaySimilarGame(similarGameButtons[similarGameIndex], similarGameImages[similarGameIndex], similarGameTitles[similarGameIndex], similarGameRatings[similarGameIndex], similarGames[similarGameIndex]);
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

            rating.Text = string.Format(LabelStrings.RatingFormat, game.Rating);

        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.AddToCart();

                ShowSuccessNotificationForBuy();
            }
            catch (Exception ex)
            {
                NotificationTip.Title = NotificationStrings.AddToCartErrorTitle;
                NotificationTip.Subtitle = string.Format(NotificationStrings.AddToCartErrorMessage, _viewModel.Game.Name) + " " + ex.Message;
                NotificationTip.IsOpen = true;
            }
        }
        private void ShowSuccessNotificationForBuy()
        {
            NotificationTip.Title = NotificationStrings.AddToCartSuccessTitle;
            NotificationTip.Subtitle = string.Format(NotificationStrings.AddToCartSuccessMessage, _viewModel.Game.Name);
            NotificationTip.IsOpen = true;
        }
        

        private void WishlistButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.AddToWishlist();

                NotificationTip.Title = NotificationStrings.AddToWishlistSuccessTitle;
                NotificationTip.Subtitle = string.Format(NotificationStrings.AddToWishlistSuccessMessage, _viewModel.Game.Name);
                NotificationTip.IsOpen = true;
            }
            catch (Exception ex)
            {
                NotificationTip.Title = NotificationStrings.AddToWishlistErrorTitle;
                string errorMessage = ex.Message;
                if (errorMessage.Contains(ErrorStrings.SqlNonQueryFailureIndicator))
                {
                    errorMessage = string.Format(ErrorStrings.AddToWishlistAlreadyExistsMessage, _viewModel.Game.Name);
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
                _viewModel.GetSimilarGames(game, frame);
                
                //if (frame != null)
                //{
                //    var gamePage = new GamePage(_viewModel._gameService, _viewModel._cartService, _viewModel._userGameService, game);
                    
                //    frame.Content = gamePage;
                   
                //}
                //else
                //{
                //    Frame.Navigate(typeof(GamePage), game);
                //}
            }
        }
    }
}

