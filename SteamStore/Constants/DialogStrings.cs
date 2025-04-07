using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class DialogStrings
    {
        
        public const string CONFIRM_PURCHASE_TITLE = "Confirm Purchase";
        public const string CONFIRM_PURCHASE_MESSAGE = "Are you sure you want to proceed with the purchase using your Steam Wallet?";
        public const string OK_BUTTON_TEXT = "OK";
        public const string YES_BUTTON_TEXT = "Yes";
        public const string NO_BUTTON_TEXT = "No";
        public const string POINTS_EARNED_TITLE = "Points Earned!";
        public const string POINTS_EARNED_MESSAGE = "You earned {0} points for your purchase! Visit the Points Shop to spend your points on exclusive items.";
    }

    public static class DeveloperDialogStrings
    {
        public const string ERROR_TITLE = "Error";
        public const string INFO_TITLE = "Information";
        public const string NO_REJECTION_MESSAGE = "No rejection message available for this game.";
        public const string FAILED_REJECTION_RETRIEVAL = "Failed to retrieve rejection message: {0}";
        public const string FAILED_TO_DELETE = "Failed to delete game: {0}";
        public const string DELETE_CONFIRMATION_OWNED = "This game is currently owned by {0} user{1}.";
        public const string ACCESS_DENIED_TITLE = "Access Denied";
        public const string ACCESS_DENIED_MESSAGE = "You do not have developer permissions to access this page.";
        public const string CLOSE_BUTTON_TEXT = "Close";
    }
}
