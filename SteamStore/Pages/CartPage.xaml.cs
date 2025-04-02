using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SteamStore.Pages
{
    public sealed partial class CartPage : Page
    {
        private CartViewModel _viewModel;

        public CartPage(CartService cartService, UserGameService userGameService)
        {
            this.InitializeComponent();
            _viewModel = new CartViewModel(cartService, userGameService);
            this.DataContext = _viewModel;
        }

        //private void RemoveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button button && button.DataContext is Game game)
        //    {
        //        _viewModel.RemoveGameFromCart(game);

        //    }
        //}

        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (_viewModel.CartGames.Count > 0)
            {
                var selectedPaymentMethod = (PaymentMethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                if(this.Parent is Frame frame)
                {
                    _viewModel.ChangeToPaymentPage(frame, selectedPaymentMethod);
                }

                //if (this.Parent is Frame frame)
                //{

                //    if (selectedPaymentMethod == "PayPal")
                //    {

                //        PaypalPaymentPage paypalPaymentPage = new PaypalPaymentPage(_viewModel._cartService, _viewModel._userGameService);
                //        frame.Content = paypalPaymentPage;

                //    }
                //    else
                //        if(selectedPaymentMethod == "Credit Card")
                //        {
                //            CreditCardPaymentPage creditCardPaymentPage = new CreditCardPaymentPage(_viewModel._cartService, _viewModel._userGameService);
                //            frame.Content = creditCardPaymentPage;
                //        }
                //    else
                //    {
                //        float totalPrice = _viewModel.CartGames.Sum(game => (float)game.Price);
                //        float userFunds = _viewModel.showUserFunds();

                //        if (userFunds < totalPrice)
                //        {
                //            await ShowErrorMessageAsync("Insufficient Funds", "You do not have enough funds in your Steam Wallet to complete this purchase.");
                //            return; 
                //        }

                //        bool isConfirmed = await ShowConfirmationDialogAsync();
                //        if (!isConfirmed)
                //            return;
                        
                //        _viewModel.PurchaseGames();
                        
                //        // Show notification about earned points
                //        if (_viewModel.LastEarnedPoints > 0)
                //        {
                //            // Store the points in App resources for PointsShopPage to access
                //            try
                //            {
                //                Application.Current.Resources["RecentEarnedPoints"] = _viewModel.LastEarnedPoints;
                //            }
                //            catch (Exception ex)
                //            {
                //                System.Diagnostics.Debug.WriteLine($"Error storing points: {ex.Message}");
                //            }
                            
                //            await ShowPointsEarnedDialogAsync(_viewModel.LastEarnedPoints);
                //        }
                //    }
                //}
            }
        }
        //private async Task<bool> ShowConfirmationDialogAsync()
        //{
        //    ContentDialog confirmDialog = new ContentDialog
        //    {
        //        Title = "Confirm Purchase",
        //        Content = "Are you sure you want to proceed with the purchase using your Steam Wallet?",
        //        PrimaryButtonText = "Yes",
        //        CloseButtonText = "No",
        //        DefaultButton = ContentDialogButton.Primary
        //    };

        //    confirmDialog.XamlRoot = this.Content.XamlRoot;

        //    ContentDialogResult result = await confirmDialog.ShowAsync();

        //    return result == ContentDialogResult.Primary;
        //}
        //private async Task ShowErrorMessageAsync(string title, string message)
        //{
        //    ContentDialog errorDialog = new ContentDialog
        //    {
        //        Title = title,
        //        Content = message,
        //        CloseButtonText = "OK",
        //        DefaultButton = ContentDialogButton.Close
        //    };

        //    await errorDialog.ShowAsync();
        //}
        //private async Task ShowPointsEarnedDialogAsync(int pointsEarned)
        //{
        //    ContentDialog pointsDialog = new ContentDialog
        //    {
        //        Title = "Points Earned!",
        //        Content = $"You earned {pointsEarned} points for your purchase!\nVisit the Points Shop to spend your points on exclusive items.",
        //        CloseButtonText = "OK",
        //        DefaultButton = ContentDialogButton.Close
        //    };

        //    pointsDialog.XamlRoot = this.Content.XamlRoot;
        //    await pointsDialog.ShowAsync();
        //}
    }
}