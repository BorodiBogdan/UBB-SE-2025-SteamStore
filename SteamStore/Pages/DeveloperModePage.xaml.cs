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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeveloperModePage : Page
    {
        private DeveloperViewModel _viewModel;

        public DeveloperModePage(DeveloperService developerService, UserGameService userGameService)
        {
            InitializeComponent();
            _viewModel = new DeveloperViewModel(developerService, userGameService);
            this.DataContext = _viewModel;

            AddGameButton.Click += AddGameButton_Click;
            ReviewGamesButton.Click += ReviewGamesButton_Click;
            MyGamesButton.Click += MyGamesButton_Click;
        }

        private void ReviewGamesButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadUnvalidated();
            DeveloperGamesList.Visibility = Visibility.Collapsed;
            ReviewGamesList.Visibility = Visibility.Visible;
            PageTitle.Text = "Review Games";
        }

        private void MyGamesButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadGames();
            DeveloperGamesList.Visibility = Visibility.Visible;
            ReviewGamesList.Visibility = Visibility.Collapsed;
            PageTitle.Text = "My Games";
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                _viewModel.ValidateGame(gameId);
                // Refresh the unvalidated games list
                _viewModel.LoadUnvalidated();
            }
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await AddGameDialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    var game = new Game
                    {
                        Id = int.Parse(AddGameId.Text),
                        Name = AddGameName.Text,
                        Price = double.Parse(AddGamePrice.Text),
                        Description = AddGameDescription.Text,
                        ImagePath = AddGameImageUrl.Text,
                        GameplayPath = AddGameplayUrl.Text,
                        TrailerPath = AddTrailerUrl.Text,
                        MinimumRequirements = AddGameMinReq.Text,
                        RecommendedRequirements = AddGameRecReq.Text,
                        Status = "Pending",
                        Discount = float.Parse(AddGameDiscount.Text)
                    };

                    _viewModel.CreateGame(game);

                    // Clear all fields after successful addition
                    AddGameId.Text = "";
                    AddGameName.Text = "";
                    AddGamePrice.Text = "";
                    AddGameDescription.Text = "";
                    AddGameImageUrl.Text = "";
                    AddGameplayUrl.Text = "";
                    AddTrailerUrl.Text = "";
                    AddGameMinReq.Text = "";
                    AddGameRecReq.Text = "";
                    AddGameDiscount.Text = "";

                    // Refresh the games list
                    _viewModel.LoadGames();
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to add game: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void ShowRejectionMessage(string message)
        {
            RejectionMessageText.Text = message;
            await RejectionMessageDialog.ShowAsync();
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                try
                {
                    // Show confirmation dialog
                    var result = await DeleteConfirmationDialog.ShowAsync();
                    
                    if (result == ContentDialogResult.Primary) // User clicked Delete
                    {
                        _viewModel.DeleteGame(gameId);
                        // Refresh the games list
                        _viewModel.LoadGames();
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to delete game: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}
