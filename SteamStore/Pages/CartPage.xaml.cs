// <copyright file="CartPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Pages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using SteamStore.Services.Interfaces;

    public sealed partial class CartPage : Page
    {
        private CartViewModel viewModel;

        public CartPage(ICartService cartService, IUserGameService userGameService)
        {
            this.InitializeComponent();
            this.viewModel = new CartViewModel(cartService, userGameService);
            this.DataContext = this.viewModel;
        }

        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.CartGames.Count > 0)
            {
                if (this.Parent is Frame frame)
                {
                    this.viewModel.ChangeToPaymentPage(frame);
                }
            }
        }
    }
}