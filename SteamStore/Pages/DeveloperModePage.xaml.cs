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

            this.Loaded += DeveloperModePage_Loaded;

            AddGameButton.Click += AddGameButton_Click;
            ReviewGamesButton.Click += ReviewGamesButton_Click;
            MyGamesButton.Click += MyGamesButton_Click;
        }

        private void DeveloperModePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if user is a developer
            if (_viewModel._developerService.GetCurrentUser().UserRole != User.Role.Developer)
            {
                // Show error message dialog
                ShowNotDeveloperMessage();
                
                // Disable all interactive elements
                AddGameButton.IsEnabled = false;
                ReviewGamesButton.IsEnabled = false;
                MyGamesButton.IsEnabled = false;
                DeveloperGamesList.IsEnabled = false;
                ReviewGamesList.IsEnabled = false;
            }
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

                RejectGameDialog.XamlRoot = this.Content.XamlRoot;
                
                var result = await RejectGameDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string rejectionReason = RejectReasonTextBox.Text;
                    await _viewModel.HandleRejectGameAsync(gameId, rejectionReason);
                }
                    
                    //try
                    //{
                    //    if (!string.IsNullOrWhiteSpace(rejectionReason))
                    //    {
                    //        _viewModel._developerService.RejectGameWithMessage(gameId, rejectionReason);
                    //    }
                    //    else
                    //    {
                    //        _viewModel.RejectGame(gameId);
                    //    }
                        
                    //    RejectReasonTextBox.Text = "";
                        
                    //    // Refresh the unvalidated games list
                    //    _viewModel.LoadUnvalidated();
                    //}
                    //catch (Exception ex)
                    //{
                    //    await ShowErrorMessage("Error", $"Failed to reject game: {ex.Message}");
                    //}
                //}
            }
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await AddGameDialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    string rejectionMessage = null;

                    var validationMessage = await _viewModel.CreateGameAsync(
                                    AddGameId.Text,
                                    AddGameName.Text,
                                    AddGamePrice.Text,
                                    AddGameDescription.Text,
                                    AddGameImageUrl.Text,
                                    AddGameplayUrl.Text,
                                    AddTrailerUrl.Text,
                                    AddGameMinReq.Text,
                                    AddGameRecReq.Text,
                                    AddGameDiscount.Text,
                                    AddGameTagList.SelectedItems.Cast<Tag>().ToList()
                                    );
                    if (validationMessage != null)
                    {
                        // Show the validation error message
                        await ShowErrorMessage("Validation Error", validationMessage);
                    }
                    else
                    {
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
                        //_viewModel.LoadGames();
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorMessage("Error", ex.Message);
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
        
        private async void ShowNotDeveloperMessage()
        {
            if (this.Content == null || this.Content.XamlRoot == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot show developer access dialog: XamlRoot is null");
                return;
            }
            
            ContentDialog notDeveloperDialog = new ContentDialog
            {
                Title = "Access Denied",
                Content = "You need to be a registered developer to access this page.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            
            try
            {
                await notDeveloperDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing developer access dialog: {ex.Message}");
            }
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
                    
                    if (result == ContentDialogResult.Primary) 
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
                    EditGameId.IsEnabled = false;
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
