// <copyright file="DeveloperModePage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Pages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using SteamStore.Constants;
    using SteamStore.Models;
    using SteamStore.Services.Interfaces;
    using Windows.Foundation;
    using Windows.Foundation.Collections;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeveloperModePage : Page
    {
        private DeveloperViewModel viewModel;

        public DeveloperModePage(IDeveloperService developerService)
        {
            this.InitializeComponent();
            this.viewModel = new DeveloperViewModel(developerService);
            this.DataContext = this.viewModel;

            this.Loaded += this.DeveloperModePage_Loaded;

            this.AddGameButton.Click += this.AddGameButton_Click;
            this.ReviewGamesButton.Click += this.ReviewGamesButton_Click;
            this.MyGamesButton.Click += this.MyGamesButton_Click;
        }

        private void DisableControls()
        {
            this.AddGameButton.IsEnabled = false;
            this.ReviewGamesButton.IsEnabled = false;
            this.MyGamesButton.IsEnabled = false;
            this.DeveloperGamesList.IsEnabled = false;
            this.ReviewGamesList.IsEnabled = false;
        }

        private void DeveloperModePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if user is a developer
            if (!this.viewModel.CheckIfUserIsADeveloper())
            {
                // Show error message dialog
                this.ShowNotDeveloperMessage();

                // Disable all interactive elements
                this.DisableControls();
            }
        }

        private void ReviewGamesButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.LoadUnvalidated();
            this.DeveloperGamesList.Visibility = Visibility.Collapsed;
            this.ReviewGamesList.Visibility = Visibility.Visible;
            this.PageTitle.Text = DeveloperPageTitles.REVIEWGAMES;
        }

        private void MyGamesButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.LoadGames();
            this.DeveloperGamesList.Visibility = Visibility.Visible;
            this.ReviewGamesList.Visibility = Visibility.Collapsed;
            this.PageTitle.Text = DeveloperPageTitles.MYGAMES;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                this.viewModel.ValidateGame(gameId);
                this.viewModel.LoadUnvalidated();
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                this.RejectGameDialog.XamlRoot = this.Content.XamlRoot;

                var result = await this.RejectGameDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string rejectionReason = this.RejectReasonTextBox.Text;
                    await this.viewModel.HandleRejectGameAsync(gameId, rejectionReason);
                }
            }
        }

        private async void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.AddGameDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await this.viewModel.CreateGameAsync(
                        this.AddGameId.Text,
                        this.AddGameName.Text,
                        this.AddGamePrice.Text,
                        this.AddGameDescription.Text,
                        this.AddGameImageUrl.Text,
                        this.AddGameplayUrl.Text,
                        this.AddTrailerUrl.Text,
                        this.AddGameMinReq.Text,
                        this.AddGameRecReq.Text,
                        this.AddGameDiscount.Text,
                        this.AddGameTagList.SelectedItems.Cast<Tag>().ToList());

                    this.ClearFieldsForAddingAGame();
                }
                catch (Exception exception)
                {
                    await this.ShowErrorMessage("Error", exception.Message);
                }
            }
        }

        private void ClearFieldsForAddingAGame()
        {
            this.AddGameId.Text = string.Empty;
            this.AddGameName.Text = string.Empty;
            this.AddGamePrice.Text = string.Empty;
            this.AddGameDescription.Text = string.Empty;
            this.AddGameImageUrl.Text = string.Empty;
            this.AddGameplayUrl.Text = string.Empty;
            this.AddTrailerUrl.Text = string.Empty;
            this.AddGameMinReq.Text = string.Empty;
            this.AddGameRecReq.Text = string.Empty;
            this.AddGameDiscount.Text = string.Empty;
            this.AddGameTagList.SelectedItems.Clear();
        }

        private async Task ShowErrorMessage(string title, string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = DialogStrings.OKBUTTONTEXT,
                XamlRoot = this.Content.XamlRoot,
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
                Title = NotDeveloperDialogStrings.ACCESSDENIEDTITLE,
                Content = NotDeveloperDialogStrings.ACCESSDENIEDMESSAGE,
                CloseButtonText = NotDeveloperDialogStrings.CLOSEBUTTONTEXT,
                XamlRoot = this.Content.XamlRoot,
            };

            try
            {
                await notDeveloperDialog.ShowAsync();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing developer access dialog: {exception.Message}");
            }
        }

        private async void RejectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                try
                {
                    string rejectionMessage = this.viewModel.GetRejectionMessage(gameId);
                    if (!string.IsNullOrWhiteSpace(rejectionMessage))
                    {
                        this.RejectionMessageText.Text = rejectionMessage;
                        this.RejectionMessageDialog.XamlRoot = this.Content.XamlRoot;
                        await this.RejectionMessageDialog.ShowAsync();
                    }
                    else
                    {
                        await this.ShowErrorMessage(DeveloperDialogStrings.INFOTITLE, DeveloperDialogStrings.NOREJECTIONMESSAGE);
                    }
                }
                catch (Exception exception)
                {
                    await this.ShowErrorMessage("Error", $"Failed to retrieve rejection message: {exception.Message}");
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
                    int ownerCount = this.viewModel.GetGameOwnerCount(gameId);
                    ContentDialogResult result;
                    if (ownerCount > 0)
                    {
                        // Game is owned by users, show warning dialog
                        this.DeleteWarningDialog.XamlRoot = this.Content.XamlRoot;

                        // OwnerCountText.Text = $"This game is currently owned by {ownerCount} user{(ownerCount == 1 ? "" : "s")}.";
                        this.OwnerCountText.Text = string.Format(DeveloperDialogStrings.DELETECONFIRMATIONOWNED, ownerCount, ownerCount == 1 ? string.Empty : "s");
                        result = await this.DeleteWarningDialog.ShowAsync();
                    }
                    else
                    {
                        // Game is not owned by any users, show standard confirmation dialog
                        this.DeleteConfirmationDialog.XamlRoot = this.Content.XamlRoot;
                        result = await this.DeleteConfirmationDialog.ShowAsync();
                    }

                    if (result == ContentDialogResult.Primary)
                    {
                        this.viewModel.DeleteGame(gameId);

                        // Refresh the games list
                        this.viewModel.LoadGames();
                    }
                }
                catch (Exception exception)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = DeveloperDialogStrings.ERRORTITLE,
                        Content = string.Format(DeveloperDialogStrings.FAILEDTODELETE, exception.Message),
                        CloseButtonText = DialogStrings.OKBUTTONTEXT,
                        XamlRoot = this.Content.XamlRoot,
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int gameId)
            {
                Game gameToEdit = this.viewModel.GetGameByIdInDeveloperGameList(gameId);
                if (gameToEdit != null)
                {
                    // System.Diagnostics.Debug.WriteLine("Im in edit");
                    try
                    {
                        this.PopulateEditForm(gameToEdit);
                        this.EditGameDialog.XamlRoot = this.Content.XamlRoot;
                        var result = await this.EditGameDialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            await this.SaveUpdatedGameAsync();

                            // Reload games after the update
                            this.viewModel.LoadGames();
                        }
                    }
                    catch (Exception exception)
                    {
                        await this.ShowErrorMessage("Error", exception.Message);
                    }
                }
            }
        }

        private async Task SaveUpdatedGameAsync()
        {
            try
            {
                await this.viewModel.UpdateGameAsync(
                    this.EditGameId.Text,
                    this.EditGameName.Text,
                    this.EditGamePrice.Text,
                    this.EditGameDescription.Text,
                    this.EditGameImageUrl.Text,
                    this.EditGameplayUrl.Text,
                    this.EditTrailerUrl.Text,
                    this.EditGameMinReq.Text,
                    this.EditGameRecReq.Text,
                    this.EditGameDiscount.Text,
                    this.EditGameTagList.SelectedItems.Cast<Tag>().ToList());
            }
            catch (Exception exception)
            {
                await this.ShowErrorMessage("Error", exception.Message);
            }
        }

        private void PopulateEditForm(Game game)
        {
            this.EditGameId.Text = game.Identifier.ToString();
            this.EditGameId.IsEnabled = false;
            this.EditGameName.Text = game.Name;
            this.EditGameDescription.Text = game.Description;
            this.EditGamePrice.Text = game.Price.ToString();
            this.EditGameImageUrl.Text = game.ImagePath;
            this.EditGameplayUrl.Text = game.GameplayPath ?? string.Empty;
            this.EditTrailerUrl.Text = game.TrailerPath ?? string.Empty;
            this.EditGameMinReq.Text = game.MinimumRequirements;
            this.EditGameRecReq.Text = game.RecommendedRequirements;
            this.EditGameDiscount.Text = game.Discount.ToString();
            this.LoadGameTags(game);
        }

        private void LoadGameTags(Game game)
        {
            this.EditGameTagList.SelectedItems.Clear();

            try
            {
                var gameTags = this.viewModel.GetGameTags(game.Identifier);
                if (gameTags.Any())
                {
                    foreach (var tag in this.EditGameTagList.Items)
                    {
                        if (tag is Tag tagItem && gameTags.Any(t => t.TagId == tagItem.TagId))
                        {
                            this.EditGameTagList.SelectedItems.Add(tag);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No tags found for the game.");
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game tags: {exception.Message}");
            }
        }
    }
}
