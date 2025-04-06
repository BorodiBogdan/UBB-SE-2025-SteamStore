using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class LabelStrings
    {
        public const string DeveloperPrefix = "Developer: ";
        public const string Owned = "OWNED";
        public const string NotOwned = "NOT OWNED";
        public const string RatingFormat = "Rating: {0}/5.0";
    }

    public static class MediaLinkLabels
    {
        public const string OfficialTrailer = "Official Trailer";
        public const string GameplayVideo = "Gameplay Video";
    }

    public static class ErrorStrings
    {
        public const string SqlNonQueryFailureIndicator = "ExecuteNonQuery";
        public const string AddToWishlistAlreadyExistsMessage = "Failed to add {0} to your wishlist: Already in wishlist";

    }

    public static class FormatStrings
    {
        public const string PriceFormat = "${0:F2}";
        public const string RatingFormat = "Rating: {0}/5.0";
    }

    public static class LogMessages
    {
        public const string SimilarGamesLoadError = "Error loading similar games: ";
        public const string GameUiLoadError = "Error loading game UI: ";
    }

}
