using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class DialogStrings
    {
        
        public const string ConfirmPurchaseTitle = "Confirm Purchase";
        public const string ConfirmPurchaseMessage = "Are you sure you want to proceed with the purchase using your Steam Wallet?";
        public const string OkButtonText = "OK";
        public const string YesButtonText = "Yes";
        public const string NoButtonText = "No";
        public const string PointsEarnedTitle = "Points Earned!";
        public const string PointsEarnedMessage = "You earned {0} points for your purchase! Visit the Points Shop to spend your points on exclusive items.";
    }

    public static class DeveloperDialogStrings
    {
        public const string ErrorTitle = "Error";
        public const string InfoTitle = "Information";
        public const string NoRejectionMessage = "No rejection message available for this game.";
        public const string FailedRejectionRetrieval = "Failed to retrieve rejection message: {0}";
        public const string FailedToDelete = "Failed to delete game: {0}";
        public const string DeleteConfirmationOwned = "This game is currently owned by {0} user{1}.";
        public const string AccessDeniedTitle = "Access Denied";
        public const string AccessDeniedMessage = "You do not have developer permissions to access this page.";
        public const string CloseButtonText = "Close";
    }
}
