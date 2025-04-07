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
        public const string PAYMENT_SUCCESS_TITLE = "Payment Successful";
        public const string PAYMENT_FAILED_TITLE = "Payment Failed";

        // Messages
        public const string PAYMENT_SUCCESS_MESSAGE = "Your purchase has been completed successfully.";
        public const string PAYMENT_SUCCESS_WITH_POINTS_MESSAGE = "Your purchase has been completed successfully. You earned {0} points!";
        public const string PAYMENT_FAILED_MESSAGE = "Please check your credit card details.";

        // Button Text
        public const string OK_BUTTON_TEXT = "OK";
    }
}
