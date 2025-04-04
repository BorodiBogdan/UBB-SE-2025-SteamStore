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
using System.Threading.Tasks;
using SteamStore.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamStore.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreditCardPaymentPage : Page
    {
        private  CreditCardPaymentViewModel _viewModel { get; }

        public CreditCardPaymentPage(CartService cartService, UserGameService userGameService)
        {
            this.InitializeComponent();
            _viewModel = new CreditCardPaymentViewModel(cartService, userGameService);
            this.DataContext = _viewModel;
        }

        private async void ProcessPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Frame frame)
            {

                await _viewModel.ProcessPaymentAsync(frame);
            }
        }

        private void NotificationDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
        }
    }
}
