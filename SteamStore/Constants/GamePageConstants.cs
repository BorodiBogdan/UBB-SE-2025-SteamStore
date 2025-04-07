using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class LabelStrings
    {
        public const string DEVELOPER_PREFIX = "Developer: ";
        public const string OWNED = "OWNED";
        public const string NOT_OWNED = "NOT OWNED";
        public const string RATING_FORMAT = "Rating: {0}/5.0";
    }

    public static class MediaLinkLabels
    {
        public const string OFFICIAL_TRAILER = "Official Trailer";
        public const string GAMEPLAY_VIDEO = "Gameplay Video";
    }

    public static class ErrorStrings
    {
        public const string SQL_NON_QUERY_FAILURE_INDICATORr = "ExecuteNonQuery";
        public const string ADD_TO_WISHLIST_ALREADY_EXISTS_MESSAGE = "Failed to add {0} to your wishlist: Already in wishlist";

    }

    public static class FormatStrings
    {
        public const string PRICE_FORMAT = "${0:F2}";
        public const string RATING_FORMAT = "Rating: {0}/5.0";
    }

    public static class LogMessages
    {
        public const string SIMILAR_GAMES_LOAD_ERROR = "Error loading similar games: ";
        public const string GAME_UI_LOAD_ERROR = "Error loading game UI: ";
    }

}
