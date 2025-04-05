using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class PaymentDialogStrings
    {
        // Titles
        public const string PaymentSuccessTitle = "Payment Successful";
        public const string PaymentFailedTitle = "Payment Failed";

        // Messages
        public const string PaymentSuccessMessage = "Your purchase has been completed successfully.";
        public const string PaymentSuccessWithPointsMessage = "Your purchase has been completed successfully. You earned {0} points!";
        public const string PaymentFailedMessage = "Please check your credit card details.";

        // Button Text
        public const string OkButtonText = "OK";
    }
}
