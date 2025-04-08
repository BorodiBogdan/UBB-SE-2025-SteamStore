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

        // SQL Parameters
        public const string GameIdParameter = "@game_id";
        public const string PublisherIdParameter = "@publisher_id";
        public const string TagIdParameter = "@tag_id";
        public const string RejectionMessageParameter = "@rejection_message";
        public const string GidParameter = "@gid";
        public const string NameParameter = "@name";
        public const string PriceParameter = "@price";
        public const string DescriptionParameter = "@description";
        public const string ImageUrlParameter = "@image_url";
        public const string TrailerUrlParameter = "@trailer_url";
        public const string GameplayUrlParameter = "@gameplay_url";
        public const string MinimumRequirementsParameter = "@minimum_requirements";
        public const string RecommendedRequirementsParameter = "@recommended_requirements";
        public const string StatusParameter = "@status";
        public const string DiscountParameter = "@discount";
       


        // Stored Procedure Names
        public const string ValidateGameProcedure = "validateGame";
        public const string GetAllUnvalidatedProcedure = "GetAllUnvalidated";
        public const string DeleteGameTagsProcedure = "DeleteGameTags";
        public const string DeleteGameReviewsProcedure = "DeleteGameReviews";
        public const string DeleteGameTransactionsProcedure = "DeleteGameTransactions";
        public const string DeleteGameFromUserLibrariesProcedure = "DeleteGameFromUserLibraries";
        public const string DeleteGameDeveloperProcedure = "DeleteGameDeveloper";
        public const string GetDeveloperGamesProcedure = "GetDeveloperGames";
        public const string UpdateGameProcedure = "UpdateGame";
        public const string RejectGameProcedure = "RejectGame";
        public const string RejectGameWithMessageProcedure = "RejectGameWithMessage";
        public const string GetRejectionMessageProcedure = "GetRejectionMessage";
        public const string InsertGameTagsProcedure = "InsertGameTags";
        public const string IsGameIdInUseProcedure = "IsGameIdInUse";
        public const string GetGameTagsProcedure = "GetGameTags";
        public const string InsertGameProcedure = "InsertGame";
        public const string GetGameRatingProcedure = "getGameRating";
        public const string GetNumberOfRecentSalesProcedure = "getNoOfRecentSalesForGame";
        public const string GetAllGamesProcedure = "GetAllGames";

        // Column Names
        public const string GameIdColumn = "game_id";
        public const string PublisherIdColumn = "publisher_id";
        public const string GameNameColumn = "name";
        public const string GameDescriptionColumn = "description";
        public const string ImageUrlColumn = "image_url";
        public const string TrailerUrlColumn = "trailer_url";
        public const string GameplayUrlColumn = "gameplay_url";
        public const string GamePriceColumn = "price";
        public const string MinimumRequirementsColumn = "minimum_requirements";
        public const string RecommendedRequirementsColumn = "recommended_requirements";
        public const string GameStatusColumn = "status";
        public const string DiscountColumn = "discount";
        public const string TagIdColumn = "tag_id";
        public const string TagNameColumn = "tag_name";
        public const string RejectionMessageColumn = "reject_message";
        public const string QueryResultColumn = "Result";
    }
}
