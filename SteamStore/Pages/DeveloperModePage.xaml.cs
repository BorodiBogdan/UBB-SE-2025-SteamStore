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
using SteamStore.Models;
using System.Threading.Tasks;

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
                    // Validation
                    if (string.IsNullOrWhiteSpace(AddGameId.Text) ||
                        string.IsNullOrWhiteSpace(AddGameName.Text) ||
                        string.IsNullOrWhiteSpace(AddGamePrice.Text) ||
                        string.IsNullOrWhiteSpace(AddGameDescription.Text) ||
                        string.IsNullOrWhiteSpace(AddGameImageUrl.Text) ||
                        string.IsNullOrWhiteSpace(AddGameMinReq.Text) ||
                        string.IsNullOrWhiteSpace(AddGameRecReq.Text) ||
                        string.IsNullOrWhiteSpace(AddGameDiscount.Text))
                    {
                        await ShowErrorMessage("Validation Error", "All fields are required.");
                        return;
                    }
                    
                    // Validate game ID (must be an integer and not already in use)
                    if (!int.TryParse(AddGameId.Text, out int gameId))
                    {
                        await ShowErrorMessage("Validation Error", "Game ID must be a valid integer.");
                        return;
                    }
                    
                    // Check if game ID is already in use
                    if (_viewModel.IsGameIdInUse(gameId))
                    {
                        await ShowErrorMessage("Validation Error", "Game ID is already in use. Please choose another ID.");
                        return;
                    }
                    
                    // Validate price (must be a positive number)
                    if (!double.TryParse(AddGamePrice.Text, out double price) || price < 0)
                    {
                        await ShowErrorMessage("Validation Error", "Price must be a positive number.");
                        return;
                    }
                    
                    // Validate discount (must be between 0 and 100)
                    if (!float.TryParse(AddGameDiscount.Text, out float discount) || discount < 0 || discount > 100)
                    {
                        await ShowErrorMessage("Validation Error", "Discount must be between 0 and 100.");
                        return;
                    }
                    
                    // Validate at least one tag is selected
                    var selectedTags = AddGameTagList.SelectedItems.Cast<Tag>().ToList();
                    if (selectedTags.Count == 0)
                    {
                        await ShowErrorMessage("Validation Error", "Please select at least one tag for the game.");
                        return;
                    }
                    
                    var game = new Game
                    {
                        Id = gameId,
                        Name = AddGameName.Text,
                        Price = price,
                        Description = AddGameDescription.Text,
                        ImagePath = AddGameImageUrl.Text,
                        GameplayPath = AddGameplayUrl.Text,
                        TrailerPath = AddTrailerUrl.Text,
                        MinimumRequirements = AddGameMinReq.Text,
                        RecommendedRequirements = AddGameRecReq.Text,
                        Status = "Pending",
                        Discount = discount
                    };
                    
                    _viewModel.CreateGame(game, selectedTags);

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
                    AddGameTagList.SelectedItems.Clear();

                    // Refresh the games list
                    _viewModel.LoadGames();
                }
                catch (Exception ex)
                {
                    await ShowErrorMessage("Error", $"Failed to add game: {ex.Message}");
                }
            }
        }

        private async Task ShowErrorMessage(string title, string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await errorDialog.ShowAsync();
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
