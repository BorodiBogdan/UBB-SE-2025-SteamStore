using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Constants
{
    public static class SqlConstants
    {
        // Stored Procedure Names
        public const string GetAllCartGames = "GetAllCartGames";
        public const string AddGameToCart = "AddGameToCart";
        public const string RemoveGameFromCart = "RemoveGameFromCart";

        // Column Names
        public const string GameIdColumn = "game_id";
        public const string NameColumn = "name";
        public const string DescriptionColumn = "Description";
        public const string ImageUrlColumn = "image_url";
        public const string PriceColumn = "price";
    }

}
