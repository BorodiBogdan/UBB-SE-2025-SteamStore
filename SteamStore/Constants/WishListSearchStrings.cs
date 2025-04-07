using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    using System;
    using Windows.ApplicationModel.Contacts.DataProvider;

    namespace SteamStore.Constants
    {
        public static class WishListSearchStrings
        {
            // Filter Constants
            public const string FILTER_ALL = "All Games";
            public const string FILTER_OVERWHELMINGLY_POSITIVE = "Overwhelmingly Positive (4.5+★)";
            public const string FILTER_VERY_POSITIVE = "Very Positive (4-4.5★)";
            public const string FILTER_MIXED = "Mixed (2-4★)";
            public const string FILTER_NEGATIVE = "Negative (<2★)";
            public const string INITIAL_SEARCH_STRING = "";

            // Sort Constants
            public const string SORT_PRICE_ASC = "Price (Low to High)";
            public const string SORT_PRICE_DESC = "Price (High to Low)";
            public const string SORT_RATING_DESC = "Rating (High to Low)";
            public const string SORT_DISCOUNT_DESC = "Discount (High to Low)";

            // Mappings
            public static readonly string OVERWHELMINGLY_POSITIVE = "overwhelmingly_positive";
            public static readonly string VERY_POSITIVE = "very_positive";
            public static readonly string MIXED = "mixed";
            public static readonly string NEGATIVE = "negative";
            public static readonly string ALL = "all";
        }

        public static class FilterCriteria
        {
            public const string OVERWHELMINGLY_POSITIVE = "overwhelmingly_positive";
            public const string VERY_POSITIVE = "very_positive";
            public const string MIXED= "mixed";
            public const string NEGATIVE = "negative";

            public const string PRICE = "price";
            public const string RATING = "rating";
            public const string DISCOUNT = "discount";
        }
    }

}
