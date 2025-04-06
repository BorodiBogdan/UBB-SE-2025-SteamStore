using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    using System;

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
        }
    }

}
