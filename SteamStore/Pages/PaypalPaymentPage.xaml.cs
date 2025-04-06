using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SteamStore.ViewModels;
using SteamStore.Services.Interfaces;

namespace SteamStore.Pages
{
    public sealed partial class PaypalPaymentPage : Page
    {
        private PaypalPaymentViewModel _viewModel;

        public PaypalPaymentPage(ICartService cartService, IUserGameService userGameService)
        {
            this.InitializeComponent();
            _viewModel = new PaypalPaymentViewModel(cartService, userGameService);
            DataContext = _viewModel;
        }

        private async void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Frame frame)
            {
                await _viewModel.ValidatePayment(frame);
            }
        }
        private void NotificationDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
        }
    }
}