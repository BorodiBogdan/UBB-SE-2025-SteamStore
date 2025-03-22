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
            await AddGameDialog.ShowAsync();
        }

        private async void ShowRejectionMessage(string message)
        {
            RejectionMessageText.Text = message;
            await RejectionMessageDialog.ShowAsync();
        }
    }
}
