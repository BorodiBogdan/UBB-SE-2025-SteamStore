using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SteamStore.Models;
using SteamStore.Pages;
using SteamStore.Repositories;
using SteamStore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;


namespace SteamStore
{
    public sealed partial class MainWindow : Window
    {
        private GameService gameService;
        private CartService cartService;
        private UserGameService userGameService;
        private DeveloperService developerService;
        private PointShopService pointShopService;
        public User user;

        public MainWindow()
        {
            
            this.InitializeComponent();

            //initiate the user
            // this will need to be changed when we conenct with a database query to get the user
            User loggedInUser = new User(1, "John Doe", "johnyDoe@gmail.com", 999999.99f, 6000f, User.Role.Developer);
            
            // Assign to the class field so it can be used in navigation
            this.user = loggedInUser;

            var dataLink = new DataLink(new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build());

            var gameRepository = new GameRepository(dataLink);
            gameService = new GameService(gameRepository);
            cartService = new CartService(new CartRepository(dataLink, loggedInUser));
            userGameService = new UserGameService(new UserGameRepository(dataLink, loggedInUser),gameRepository);
            pointShopService = new PointShopService(loggedInUser, dataLink);

            var developerRepository = new DeveloperRepository(dataLink,loggedInUser);
            developerService = new DeveloperService(developerRepository);


            if (ContentFrame == null)
            {
                throw new Exception("ContentFrame is not initialized.");
            }
            ContentFrame.Content = new HomePage(gameService, cartService, userGameService);
        }
        public void ResetToHomePage()
        {
            ContentFrame.Content = new HomePage(gameService, cartService, userGameService);
        }
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var tag = args.SelectedItemContainer.Tag.ToString();
                switch (tag)
                {
                    case "HomePage":
                        ContentFrame.Content = new HomePage(gameService, cartService, userGameService);
                        break;
                    case "CartPage":
                        ContentFrame.Content = new CartPage(cartService, userGameService);
                        break;
                    case "PointsShopPage":
                        ContentFrame.Content = new PointsShopPage(pointShopService);
                        break;
                    case "WishlistPage":
                        ContentFrame.Content = new WishListView(userGameService, gameService, cartService);
                        break;
                    case "DeveloperModePage":
                        ContentFrame.Content = new DeveloperModePage(developerService, userGameService);
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