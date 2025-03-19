using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SteamStore.Pages;
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

namespace SteamStore
{
    public sealed partial class MainWindow : Window
    {
        private GameService gameService;
        private CartService cartService;
        private User user;

        public MainWindow()
        {
            this.InitializeComponent();

            //initiate the user
            User loggedInUser = new User(1, "John Doe", "johnyDoe@gmail.com", 999999999, 99999999, User.Role.Developer);

            // Resolve dependencies (e.g., GameRepository and DataLink)
            var dataLink = new DataLink(new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build());

            gameService = new GameService(new GameRepository(dataLink));
            cartService = new CartService(new CartRepository(dataLink, loggedInUser));

            if (ContentFrame == null)
            {
                throw new Exception("ContentFrame is not initialized.");
            }
            ContentFrame.Content = new HomePage(gameService);
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var tag = args.SelectedItemContainer.Tag.ToString();
                switch (tag)
                {
                    case "HomePage":
                        ContentFrame.Content = new HomePage(gameService);
                        break;
                    case "CartPage":
                        ContentFrame.Content = new CartPage(cartService);
                        break;
                    case "PointsShopPage":
                        ContentFrame.Navigate(typeof(PointsShopPage));
                        break;
                    case "WishlistPage":
                        ContentFrame.Navigate(typeof(WishListPage));
                        break;
                    case "DeveloperModePage":
                        ContentFrame.Navigate(typeof(DeveloperModePage));
                        break;
                }
            }
            if (NavView != null)
            {
                // Deselect the NavigationViewItem when moving to a non-menu page
                NavView.SelectedItem = null;
            }
        }
    }
}