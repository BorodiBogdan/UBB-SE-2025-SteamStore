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
using SteamStore.Constants;


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
            if (! _viewModel.CheckIfUserIsADeveloper())
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
            PageTitle.Text = DeveloperPageTitles.ReviewGames;
        }

        private void MyGamesButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadGames();
            DeveloperGamesList.Visibility = Visibility.Visible;
            ReviewGamesList.Visibility = Visibility.Collapsed;
            PageTitle.Text = DeveloperPageTitles.MyGames;
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
                    
                    
            }
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await AddGameDialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    

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
                       
                        await ShowErrorMessage("Validation Error", validationMessage);
                    }
                    else
                    {
                        ClearFieldsForAddingAGame();

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
        private void ClearFieldsForAddingAGame()
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
                Title = NotDeveloperDialogStrings.AccessDeniedTitle,
                Content = NotDeveloperDialogStrings.AccessDeniedMessage,
                CloseButtonText = NotDeveloperDialogStrings.CloseButtonText,
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
                    string rejectionMessage = _viewModel.GetRejectionMessage(gameId);
                    
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
           // System.Diagnostics.Debug.WriteLine("EditButton_Click was called!");
            
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

                    try
                    {
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
                          
                            string errorMessage = await _viewModel.UpdateGameAsync(
                                EditGameId.Text,
                                EditGameName.Text,
                                EditGamePrice.Text,
                                EditGameDescription.Text,
                                EditGameImageUrl.Text,
                                EditGameplayUrl.Text,
                                EditTrailerUrl.Text,
                                EditGameMinReq.Text,
                                EditGameRecReq.Text,
                                EditGameDiscount.Text,
                                EditGameTagList.SelectedItems.Cast<Tag>().ToList()
                                
                                );
                            if (errorMessage != null)
                            {
                                await ShowErrorMessage("Error",errorMessage);
                            }
                        }
                        catch (Exception ex)
                        {

                            await ShowErrorMessage("Error",ex.Message);

                        }
                            
                            _viewModel.LoadGames();
                        
                        
                    }
                }
            }
        }
    }
}
