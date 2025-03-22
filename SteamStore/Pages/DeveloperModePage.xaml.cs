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

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                // Set XamlRoot for the dialog
                RejectGameDialog.XamlRoot = this.Content.XamlRoot;
                
                var result = await RejectGameDialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    string rejectionReason = RejectReasonTextBox.Text;
                    
                    try
                    {
                        // If we have a rejection service method with message, use it
                        if (!string.IsNullOrWhiteSpace(rejectionReason))
                        {
                            _viewModel._developerService.RejectGameWithMessage(gameId, rejectionReason);
                        }
                        else
                        {
                            _viewModel.RejectGame(gameId);
                        }
                        
                        // Clear the rejection reason textbox
                        RejectReasonTextBox.Text = "";
                        
                        // Refresh the unvalidated games list
                        _viewModel.LoadUnvalidated();
                    }
                    catch (Exception ex)
                    {
                        await ShowErrorMessage("Error", $"Failed to reject game: {ex.Message}");
                    }
                }
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
        
        private async void RejectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                try
                {
                    string rejectionMessage = _viewModel._developerService.GetRejectionMessage(gameId);
                    
                    if (!string.IsNullOrWhiteSpace(rejectionMessage))
                    {
                        RejectionMessageText.Text = rejectionMessage;
                        RejectionMessageDialog.XamlRoot = this.Content.XamlRoot;
                        await RejectionMessageDialog.ShowAsync();
                    }
                    else
                    {
                        await ShowErrorMessage("Information", "No rejection message available for this game.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorMessage("Error", $"Failed to retrieve rejection message: {ex.Message}");
                }
            }
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                try
                {
                    // Check if the game is owned by any users
                    int ownerCount = _viewModel.GetGameOwnerCount(gameId);
                    
                    ContentDialogResult result;
                    
                    if (ownerCount > 0)
                    {
                        // Game is owned by users, show warning dialog
                        DeleteWarningDialog.XamlRoot = this.Content.XamlRoot;
                        OwnerCountText.Text = $"This game is currently owned by {ownerCount} user{(ownerCount == 1 ? "" : "s")}.";
                        
                        result = await DeleteWarningDialog.ShowAsync();
                    }
                    else
                    {
                        // Game is not owned by any users, show standard confirmation dialog
                        DeleteConfirmationDialog.XamlRoot = this.Content.XamlRoot;
                        result = await DeleteConfirmationDialog.ShowAsync();
                    }
                    
                    if (result == ContentDialogResult.Primary) // User clicked Delete/Delete Anyway
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

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EditButton_Click was called!");
            
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                var game = _viewModel.DeveloperGames.FirstOrDefault(g => g.Id == gameId);
                if (game != null)
                {
                    // Populate edit form with game data
                    EditGameId.Text = game.Id.ToString();
                    EditGameId.IsEnabled = false; // Cannot change game ID
                    EditGameName.Text = game.Name;
                    EditGameDescription.Text = game.Description;
                    EditGamePrice.Text = game.Price.ToString();
                    EditGameImageUrl.Text = game.ImagePath;
                    EditGameplayUrl.Text = game.GameplayPath ?? "";
                    EditTrailerUrl.Text = game.TrailerPath ?? "";
                    EditGameMinReq.Text = game.MinimumRequirements;
                    EditGameRecReq.Text = game.RecommendedRequirements;
                    EditGameDiscount.Text = game.Discount.ToString();
                    
                    // Get game tags and preselect them in the UI
                    EditGameTagList.SelectedItems.Clear();
                    
                    try {
                        var gameTags = _viewModel._developerService.GetGameTags(game.Id);
                        
                        // Ensure EditGameTagList has items
                        if (EditGameTagList.Items != null && EditGameTagList.Items.Count > 0)
                        {
                            foreach (var tag in EditGameTagList.Items)
                            {
                                if (tag is Tag tagItem && gameTags.Any(t => t.tag_id == tagItem.tag_id))
                                {
                                    EditGameTagList.SelectedItems.Add(tag);
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("EditGameTagList has no items loaded");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading game tags: {ex.Message}");
                    }
                    
                    // Set XamlRoot for the dialog
                    EditGameDialog.XamlRoot = this.Content.XamlRoot;
                    
                    var result = await EditGameDialog.ShowAsync();
                    
                    if (result == ContentDialogResult.Primary)
                    {
                        try
                        {
                            // Validation
                            if (string.IsNullOrWhiteSpace(EditGameName.Text) ||
                                string.IsNullOrWhiteSpace(EditGamePrice.Text) ||
                                string.IsNullOrWhiteSpace(EditGameDescription.Text) ||
                                string.IsNullOrWhiteSpace(EditGameImageUrl.Text) ||
                                string.IsNullOrWhiteSpace(EditGameMinReq.Text) ||
                                string.IsNullOrWhiteSpace(EditGameRecReq.Text) ||
                                string.IsNullOrWhiteSpace(EditGameDiscount.Text))
                            {
                                await ShowErrorMessage("Validation Error", "All fields are required.");
                                return;
                            }
                            
                            // Validate price (must be a positive number)
                            if (!double.TryParse(EditGamePrice.Text, out double price) || price < 0)
                            {
                                await ShowErrorMessage("Validation Error", "Price must be a positive number.");
                                return;
                            }
                            
                            // Validate discount (must be between 0 and 100)
                            if (!float.TryParse(EditGameDiscount.Text, out float discount) || discount < 0 || discount > 100)
                            {
                                await ShowErrorMessage("Validation Error", "Discount must be between 0 and 100.");
                                return;
                            }
                            
                            // Validate at least one tag is selected
                            var selectedTags = EditGameTagList.SelectedItems.Cast<Tag>().ToList();
                            if (selectedTags.Count == 0)
                            {
                                await ShowErrorMessage("Validation Error", "Please select at least one tag for the game.");
                                return;
                            }
                            
                            // Create updated game object
                            var updatedGame = new Game
                            {
                                Id = game.Id,
                                Name = EditGameName.Text,
                                Price = price,
                                Description = EditGameDescription.Text,
                                ImagePath = EditGameImageUrl.Text,
                                GameplayPath = EditGameplayUrl.Text,
                                TrailerPath = EditTrailerUrl.Text,
                                MinimumRequirements = EditGameMinReq.Text,
                                RecommendedRequirements = EditGameRecReq.Text,
                                Status = "Pending", // Always reset status to Pending for any edited game to require re-validation
                                Discount = discount,
                                PublisherId = game.PublisherId // Keep existing publisher
                            };
                            
                            System.Diagnostics.Debug.WriteLine($"Game status set to 'Pending' for game ID {game.Id}");
                            
                            // First delete existing tags, then add the new ones
                            try
                            {
                                _viewModel._developerService.DeleteGameTags(game.Id);
                                System.Diagnostics.Debug.WriteLine("Successfully deleted existing game tags");
                            }
                            catch (Exception tagEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error deleting game tags: {tagEx.Message}");
                                await ShowErrorMessage("Tag Error", $"Failed to remove existing game tags: {tagEx.Message}");
                                return;
                            }
                            
                            // Update the game
                            _viewModel.UpdateGame(updatedGame);
                            
                            // Add new tags
                            try
                            {
                                foreach (var tag in selectedTags)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Inserting tag ID {tag.tag_id} for game ID {game.Id}");
                                    _viewModel._developerService.InsertGameTag(game.Id, tag.tag_id);
                                }
                                System.Diagnostics.Debug.WriteLine("Successfully inserted all game tags");
                            }
                            catch (Exception tagEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error inserting game tags: {tagEx.Message}");
                                await ShowErrorMessage("Tag Error", $"Failed to add new game tags: {tagEx.Message}");
                                return;
                            }
                            
                            // Refresh the games list
                            _viewModel.LoadGames();
                        }
                        catch (Exception ex)
                        {
                            await ShowErrorMessage("Error", $"Failed to update game: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
