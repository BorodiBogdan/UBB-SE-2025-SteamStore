using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class NotificationStrings
    {
        // Cart-related notifications
        public const string AddToCartSuccessTitle = "Success";
        public const string AddToCartErrorTitle = "Error";
        public const string AddToCartSuccessMessage = "{0} was added to your cart.";
        public const string AddToCartErrorMessage = "Failed to add {0} to your cart.";

        // Wishlist-related notifications
        public const string AddToWishlistSuccessTitle = "Success";
        public const string AddToWishlistErrorTitle = "Error";
        public const string AddToWishlistSuccessMessage = "{0} was added to your wishlist.";
        public const string AddToWishlistErrorMessage = "Failed to add {0} to your wishlist.";
        public const string AlreadyInWishlistErrorMessage = "Already in wishlist.";


        public static class ExceptionMessages
        {
            public const string AllFieldsRequired = "All fields Are required!";
            public const string InvalidGameId = "GameId must be a valid integer!";
            public const string InvalidPrice = "Price must be a positive number.";
            public const string InvalidDiscount = "Discount must be between 0 and 100.";
            public const string NoTagsSelected = "Please select at least one tag for the game.";
            public const string GameIdInUse = "Game ID is already in use.";
            public const string FailedToCreateGame = "Failed to create game.";
            public const string FailedToUpdateGame = "Failed to update game.";
            public const string FailedToDeleteGame = "Failed to delete game.";
            public const string FailedToInsertGameTag = "Failed to insert game tag.";
            public const string FailedToDeleteGameTags = "Failed to delete game tags.";
            public const string FailedToRejectGame = "Failed to reject game.";
            public const string GameAlreadyOwned = "Failed to add {0} to your wishlist: Game already owned";
            public const string GameAlreadyInWishlist = "Failed to add {0} to your wishlist: Already in wishlist";

            public const String IdAlreadyInUse = "Game ID is already in use.";

        }
    }
}
