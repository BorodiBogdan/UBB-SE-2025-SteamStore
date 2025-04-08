// <copyright file="SqlConstants.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Constants
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class SqlConstants
    {
        // Stored Procedure Names
        public const string GETALLCARTGAMES = "GetAllCartGames";
        public const string ADDGAMETOCART = "AddGameToCart";
        public const string REMOVEGAMEFROMCART = "RemoveGameFromCart";

        // Column Names
        public const string GAMEIDCOLUMN = "game_id";
        public const string NAMECOLUMN = "name";
        public const string DESCRIPTIONCOLUMN = "Description";
        public const string IMAGEURLCOLUMN = "image_url";
        public const string PRICECOLUMN = "price";
    }
}
