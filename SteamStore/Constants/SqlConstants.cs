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
        public const string GET_ALL_CART_GAMES = "GetAllCartGames";
        public const string ADD_GAME_TO_CART = "AddGameToCart";
        public const string REMOVE_GAME_FROM_CART = "RemoveGameFromCart";

        // Column Names
        public const string GAME_ID_COLUMN = "game_id";
        public const string NAME_COLUMN  = "name";
        public const string DESCRIPTION_COLUMN = "Description";
        public const string IMAGE_URL_COLUMN = "image_url";
        public const string PRICE_COLUMN = "price";
    }

}
