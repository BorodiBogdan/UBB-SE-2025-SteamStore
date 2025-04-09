// <copyright file="GameRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories.Interfaces;
using SteamStore.Constants;

public class GameRepository : IGameRepository
{
    private readonly IDataLink dataLink;

    public GameRepository(IDataLink dataLink)
    {
        this.dataLink = dataLink;
    }

    public void ValidateGame(int gameId)
    {
        var sqlParameters = new[]
        {
            new SqlParameter(SqlConstants.GameIdParameter, gameId),
        };
        this.dataLink.ExecuteNonQuery(SqlConstants.ValidateGameProcedure, sqlParameters);
    }

    public List<Game> GetUnvalidated(int userId)
    {
        var parameters = new[] { new SqlParameter(SqlConstants.PublisherIdParameter, userId) };

        var result = this.dataLink.ExecuteReader(SqlConstants.GetAllUnvalidatedProcedure, parameters);
        var games = new List<Game>();

        foreach (DataRow row in result.Rows)
        {
            var game = new Game
            {
                Identifier = (int)row[SqlConstants.GameIdColumn],
                Name = (string)row[SqlConstants.GameNameColumn],
                Price = Convert.ToDecimal(row[SqlConstants.GamePriceColumn]),
                Description = (string)row[SqlConstants.GameDescriptionColumn],
                ImagePath = (string)row[SqlConstants.ImageUrlColumn],
                TrailerPath = (string)row[SqlConstants.TrailerUrlColumn],
                GameplayPath = (string)row[SqlConstants.GameplayUrlColumn],
                MinimumRequirements = (string)row[SqlConstants.MinimumRequirementsColumn],
                RecommendedRequirements = (string)row[SqlConstants.RecommendedRequirementsColumn],
                Status = (string)row[SqlConstants.GameStatusColumn],
                Discount = Convert.ToDecimal(row[SqlConstants.DiscountColumn]),
                PublisherIdentifier = (int)row[SqlConstants.PublisherIdColumn],
            };
            games.Add(game);
        }

        return games;
    }

    public void DeleteGameTags(int gameId)
    {
        var sqlParameters = new[] { new SqlParameter(SqlConstants.GameIdParameter, gameId) };
        this.dataLink.ExecuteNonQuery(SqlConstants.DeleteGameTagsProcedure, sqlParameters);
    }

    public void DeleteGame(int gameId)
    {
        // Delete related data from all tables in the correct order to avoid foreign key constraint violations

        // 1. First delete game tags
        this.DeleteGameTags(gameId);

        // 2. Delete game reviews
        SqlParameter[] reviewParams = { new (SqlConstants.GameIdParameter, gameId) };
        this.dataLink.ExecuteNonQuery(SqlConstants.DeleteGameReviewsProcedure, reviewParams);

        // 3. Delete game from transaction history
        SqlParameter[] transactionParams = { new (SqlConstants.GameIdParameter, gameId) };
        this.dataLink.ExecuteNonQuery(SqlConstants.DeleteGameTransactionsProcedure, transactionParams);

        // 4. Delete game from user libraries
        SqlParameter[] libraryParams = { new (SqlConstants.GameIdParameter, gameId) };
        this.dataLink.ExecuteNonQuery(SqlConstants.DeleteGameFromUserLibrariesProcedure, libraryParams);

        // 5. delete the game itself
        SqlParameter[] gameParams = { new (SqlConstants.GameIdParameter, gameId) };
        this.dataLink.ExecuteNonQuery(SqlConstants.DeleteGameDeveloperProcedure, gameParams);
    }

    public List<Game> GetDeveloperGames(int userId)
    {
        SqlParameter[] parameters = { new (SqlConstants.PublisherIdParameter, userId) };
        var result = this.dataLink.ExecuteReader(SqlConstants.GetDeveloperGamesProcedure, parameters);

        return (from DataRow row in result.Rows
            select new Game
            {
                Identifier = (int)row[SqlConstants.GameIdColumn],
                Name = (string)row[SqlConstants.GameNameColumn],
                Price = Convert.ToDecimal(row[SqlConstants.GamePriceColumn]),
                Description = (string)row[SqlConstants.GameDescriptionColumn],
                ImagePath = (string)row[SqlConstants.ImageUrlColumn],
                TrailerPath = (string)row[SqlConstants.TrailerUrlColumn],
                GameplayPath = (string)row[SqlConstants.GameplayUrlColumn],
                MinimumRequirements = (string)row[SqlConstants.MinimumRequirementsColumn],
                RecommendedRequirements = (string)row[SqlConstants.RecommendedRequirementsColumn],
                Status = (string)row[SqlConstants.GameStatusColumn],
                Discount = Convert.ToDecimal(row[SqlConstants.DiscountColumn]),
                PublisherIdentifier = (int)row[SqlConstants.PublisherIdColumn],
            }).ToList();
    }

    public void UpdateGame(int gameId, Game game)
    {
        SqlParameter[] sqlParameters =
        {
            new (SqlConstants.GameIdParameter, gameId),
            new (SqlConstants.NameParameter, game.Name),
            new (SqlConstants.PriceParameter, game.Price),
            new (SqlConstants.DescriptionParameter, game.Description),
            new (SqlConstants.ImageUrlParameter, game.ImagePath),
            new (SqlConstants.TrailerUrlParameter, game.TrailerPath),
            new (SqlConstants.GameplayUrlParameter, game.GameplayPath),
            new (SqlConstants.MinimumRequirementsParameter, game.MinimumRequirements),
            new (SqlConstants.RecommendedRequirementsParameter, game.RecommendedRequirements),
            new (SqlConstants.StatusParameter, game.Status),
            new (SqlConstants.DiscountParameter, game.Discount),
            new (SqlConstants.PublisherIdParameter, game.PublisherIdentifier),
        };
        this.dataLink.ExecuteNonQuery(SqlConstants.UpdateGameProcedure, sqlParameters);
    }

    public void RejectGame(int gameId)
    {
        SqlParameter[] sqlParameters = { new (SqlConstants.GameIdParameter, gameId) };
        this.dataLink.ExecuteNonQuery(SqlConstants.RejectGameProcedure, sqlParameters);
    }

    public void RejectGameWithMessage(int gameId, string message)
    {
        SqlParameter[] sqlParameters =
        {
            new (SqlConstants.GameIdParameter, gameId),
            new (SqlConstants.RejectionMessageParameter, message),
        };
        this.dataLink.ExecuteNonQuery(SqlConstants.RejectGameWithMessageProcedure, sqlParameters);
    }

    public string GetRejectionMessage(int gameId)
    {
        var sqlParameters = new[] { new SqlParameter(SqlConstants.GameIdParameter, gameId) };

        var result = this.dataLink.ExecuteReader(SqlConstants.GetRejectionMessageProcedure, sqlParameters);

        if (result == null || result.Rows.Count <= 0)
        {
            return string.Empty;
        }

        return result.Rows[0][SqlConstants.RejectionMessageColumn] != DBNull.Value
            ? result.Rows[0][SqlConstants.RejectionMessageColumn].ToString()
            : string.Empty;
    }

    public void InsertGameTag(int gameId, int tagId)
    {
        var sqlParameters = new SqlParameter[]
        {
            new (SqlConstants.GameIdParameter, gameId),
            new (SqlConstants.TagIdParameter, tagId),
        };

        this.dataLink.ExecuteNonQuery(SqlConstants.InsertGameTagsProcedure, sqlParameters);
    }

    public bool IsGameIdInUse(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new (SqlConstants.GameIdParameter, gameId) };

        var result = this.dataLink.ExecuteReader(SqlConstants.IsGameIdInUseProcedure, sqlParameters);

        if (result == null || result.Rows.Count <= 0)
        {
            return false;
        }

        var count = Convert.ToInt32(result.Rows[0][SqlConstants.QueryResultColumn]);
        return count > 0;
    }

    public List<Tag> GetGameTags(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new (SqlConstants.GidParameter, gameId) };

        var result = this.dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, sqlParameters);
        return (from DataRow row in result.Rows
                select new Tag { TagId = (int)row[SqlConstants.TagIdColumn], Tag_name = (string)row[SqlConstants.TagNameColumn] })
            .OrderBy(tag => tag.Tag_name).ToList();
    }

    public void CreateGame(Game game)
    {
        var sqlParameters = new SqlParameter[]
        {
            new (SqlConstants.GameIdParameter, game.Identifier),
            new (SqlConstants.NameParameter, game.Name),
            new (SqlConstants.PriceParameter, game.Price),
            new (SqlConstants.PublisherIdParameter, game.PublisherIdentifier),
            new (SqlConstants.DescriptionParameter, game.Description),
            new (SqlConstants.ImageUrlParameter, game.ImagePath),
            new (SqlConstants.TrailerUrlParameter, game.TrailerPath ?? string.Empty),
            new (SqlConstants.GameplayUrlParameter, game.GameplayPath ?? string.Empty),
            new (SqlConstants.MinimumRequirementsParameter, game.MinimumRequirements),
            new (SqlConstants.RecommendedRequirementsParameter, game.RecommendedRequirements),
            new (SqlConstants.StatusParameter, game.Status),
            new (SqlConstants.DiscountParameter, game.Discount),
        };
        this.dataLink.ExecuteNonQuery(SqlConstants.InsertGameProcedure, sqlParameters);
    }

    public decimal GetGameRating(int gameId)
    {
        SqlParameter[] parameters = { new (SqlConstants.GidParameter, gameId) };

        decimal? result = this.dataLink.ExecuteScalar<decimal>(SqlConstants.GetGameRatingProcedure, parameters);
        return (decimal)result;
    }

    public int GetNoOfRecentSalesForGame(int gameId)
    {
        SqlParameter[] parameters = { new (SqlConstants.GidParameter, gameId) };
        int? result = this.dataLink.ExecuteScalar<int>(SqlConstants.GetNumberOfRecentSalesProcedure, parameters);
        return (int)result;
    }

    public Collection<Game> GetAllGames()
    {
        var result = this.dataLink.ExecuteReader(SqlConstants.GetAllGamesProcedure);
        var games = (from DataRow row in result.Rows
            select new Game
            {
                Identifier = (int)row[SqlConstants.GameIdColumn],
                PublisherIdentifier = (int)row[SqlConstants.PublisherIdColumn],
                Name = (string)row[SqlConstants.GameNameColumn],
                Description = (string)row[SqlConstants.GameDescriptionColumn],
                ImagePath = (string)row[SqlConstants.ImageUrlColumn],
                TrailerPath = (string)row[SqlConstants.TrailerUrlColumn],
                GameplayPath = (string)row[SqlConstants.GameplayUrlColumn],
                Price = Convert.ToDecimal(row[SqlConstants.GamePriceColumn]),
                MinimumRequirements = (string)row[SqlConstants.MinimumRequirementsColumn],
                RecommendedRequirements = (string)row[SqlConstants.RecommendedRequirementsColumn],
                Status = (string)row[SqlConstants.GameStatusColumn],
                Discount = (int)row[SqlConstants.DiscountColumn],
                Tags = this.GetGameTags((int)row[SqlConstants.GameIdColumn]).Select(tag => tag.Tag_name).ToArray(),
                Rating = this.GetGameRating((int)row[SqlConstants.GameIdColumn]),
                NumberOfRecentPurchases = this.GetNoOfRecentSalesForGame((int)row["game_id"]),
                TrendingScore = Game.NOTCOMPUTED,
                TagScore = Game.NOTCOMPUTED,
            }).ToList();

        return new Collection<Game>(games);
    }
}