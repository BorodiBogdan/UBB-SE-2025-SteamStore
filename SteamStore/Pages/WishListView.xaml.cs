// <copyright file="WishListView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Pages
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using SteamStore;
    using SteamStore.Models;
    using SteamStore.Pages;
    using SteamStore.Services.Interfaces;
    using SteamStore.ViewModels;

    public sealed partial class WishListView : Page
    {
        private WishListViewModel viewModel;

        public WishListView(IUserGameService userGameService, IGameService gameService, ICartService cartService)
        {
            this.InitializeComponent();
            this.viewModel = new WishListViewModel(userGameService, gameService, cartService);
            this.DataContext = this.viewModel;
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Game game)
            {
                if (this.Parent is Frame frame)
                {
                    this.viewModel.ViewGameDetails(frame, game);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Frame frame)
            {
                this.viewModel.BackToHomePage(frame);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.HandleSearchWishListGames();
            }
        }
    }
}