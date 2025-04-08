// <copyright file="GameRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories.Interfaces;

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
            new SqlParameter("@game_id", gameId),
        };
        this.dataLink.ExecuteNonQuery("validateGame", sqlParameters);
    }

    public List<Game> GetUnvalidated(int userId)
    {
        var parameters = new[] { new SqlParameter("@publisher_id", userId) };

        var result = this.dataLink.ExecuteReader("GetAllUnvalidated", parameters);
        var games = new List<Game>();

        foreach (DataRow row in result.Rows)
        {
            var game = new Game
            {
                Identifier = (int)row["game_id"],
                Name = (string)row["name"],
                Price = Convert.ToDecimal(row["price"]),
                Description = (string)row["description"],
                ImagePath = (string)row["image_url"],
                TrailerPath = (string)row["trailer_url"],
                GameplayPath = (string)row["gameplay_url"],
                MinimumRequirements = (string)row["minimum_requirements"],
                RecommendedRequirements = (string)row["recommended_requirements"],
                Status = (string)row["status"],
                Discount = Convert.ToDecimal(row["discount"]),
                PublisherIdentifier = (int)row["publisher_id"],
            };
            games.Add(game);
        }

        return games;
    }

    public void DeleteGameTags(int gameId)
    {
        var sqlParameters = new[] { new SqlParameter("@game_id", gameId) };
        this.dataLink.ExecuteNonQuery("DeleteGameTags", sqlParameters);
    }

    public void DeleteGame(int gameId)
    {
        // Delete related data from all tables in the correct order to avoid foreign key constraint violations

        // 1. First delete game tags
        this.DeleteGameTags(gameId);

        // 2. Delete game reviews
        SqlParameter[] reviewParams = { new ("@game_id", gameId) };
        this.dataLink.ExecuteNonQuery("DeleteGameReviews", reviewParams);

        // 3. Delete game from transaction history
        SqlParameter[] transactionParams = { new ("@game_id", gameId) };
        this.dataLink.ExecuteNonQuery("DeleteGameTransactions", transactionParams);

        // 4. Delete game from user libraries
        SqlParameter[] libraryParams = { new ("@game_id", gameId) };
        this.dataLink.ExecuteNonQuery("DeleteGameFromUserLibraries", libraryParams);

        // 5. delete the game itself
        SqlParameter[] gameParams = { new ("@game_id", gameId) };
        this.dataLink.ExecuteNonQuery("DeleteGameDeveloper", gameParams);
    }

    public List<Game> GetDeveloperGames(int userId)
    {
        SqlParameter[] parameters = { new ("@publisher_id", userId) };
        var result = this.dataLink.ExecuteReader("GetDeveloperGames", parameters);

        return (from DataRow row in result.Rows
            select new Game
            {
                Identifier = (int)row["game_id"],
                Name = (string)row["name"],
                Price = Convert.ToDecimal(row["price"]),
                Description = (string)row["description"],
                ImagePath = (string)row["image_url"],
                TrailerPath = (string)row["trailer_url"],
                GameplayPath = (string)row["gameplay_url"],
                MinimumRequirements = (string)row["minimum_requirements"],
                RecommendedRequirements = (string)row["recommended_requirements"],
                Status = (string)row["status"],
                Discount = Convert.ToDecimal(row["discount"]),
                PublisherIdentifier = (int)row["publisher_id"],
            }).ToList();
    }

    public void UpdateGame(int gameId, Game game)
    {
        SqlParameter[] sqlParameters =
        {
            new ("@game_id", gameId),
            new ("@name", game.Name),
            new ("@price", game.Price),
            new ("@description", game.Description),
            new ("@image_url", game.ImagePath),
            new ("@trailer_url", game.TrailerPath),
            new ("@gameplay_url", game.GameplayPath),
            new ("@minimum_requirements", game.MinimumRequirements),
            new ("@recommended_requirements", game.RecommendedRequirements),
            new ("@status", game.Status),
            new ("@discount", game.Discount),
            new ("@publisher_id", game.PublisherIdentifier),
        };
        this.dataLink.ExecuteNonQuery("UpdateGame", sqlParameters);
    }

    public void RejectGame(int gameId)
    {
        SqlParameter[] sqlParameters = { new ("@game_id", gameId) };
        this.dataLink.ExecuteNonQuery("RejectGame", sqlParameters);
    }

    public void RejectGameWithMessage(int gameId, string message)
    {
        SqlParameter[] sqlParameters =
        {
            new ("@game_id", gameId),
            new ("@rejection_message", message),
        };
        this.dataLink.ExecuteNonQuery("RejectGameWithMessage", sqlParameters);
    }

    public string GetRejectionMessage(int gameId)
    {
        var sqlParameters = new[] { new SqlParameter("@game_id", gameId) };

        var result = this.dataLink.ExecuteReader("GetRejectionMessage", sqlParameters);

        if (result == null || result.Rows.Count <= 0)
        {
            return string.Empty;
        }

        return result.Rows[0]["reject_message"] != DBNull.Value
            ? result.Rows[0]["reject_message"].ToString()
            : string.Empty;
    }

    public void InsertGameTag(int gameId, int tagId)
    {
        var sqlParameters = new SqlParameter[]
        {
            new ("@game_id", gameId),
            new ("@tag_id", tagId),
        };

        this.dataLink.ExecuteNonQuery("InsertGameTags", sqlParameters);
    }

    public bool IsGameIdInUse(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new ("@game_id", gameId) };

        var result = this.dataLink.ExecuteReader("IsGameIdInUse", sqlParameters);

        if (result == null || result.Rows.Count <= 0)
        {
            return false;
        }

        var count = Convert.ToInt32(result.Rows[0]["Result"]);
        return count > 0;
    }

    public List<Tag> GetGameTags(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new ("@gid", gameId) };

        var result = this.dataLink.ExecuteReader("GetGameTags", sqlParameters);
        return (from DataRow row in result.Rows
                select new Tag { TagId = (int)row["tag_id"], Tag_name = (string)row["tag_name"] })
            .OrderBy(tag => tag.Tag_name).ToList();
    }

    public void CreateGame(Game game)
    {
        var sqlParameters = new SqlParameter[]
        {
            new ("@game_id", game.Identifier),
            new ("@name", game.Name),
            new ("@price", game.Price),
            new ("@publisher_id", game.PublisherIdentifier),
            new ("@description", game.Description),
            new ("@image_url", game.ImagePath),
            new ("@trailer_url", game.TrailerPath ?? string.Empty),
            new ("@gameplay_url", game.GameplayPath ?? string.Empty),
            new ("@minimum_requirements", game.MinimumRequirements),
            new ("@recommended_requirements", game.RecommendedRequirements),
            new ("@status", game.Status),
            new ("@discount", game.Discount),
        };
        this.dataLink.ExecuteNonQuery("InsertGame", sqlParameters);
    }

    public decimal GetGameRating(int gameId)
    {
        SqlParameter[] parameters = { new ("@gid", gameId) };

        decimal? result = this.dataLink.ExecuteScalar<decimal>("getGameRating", parameters);
        return (decimal)result;
    }

    public int GetNoOfRecentSalesForGame(int gameId)
    {
        SqlParameter[] parameters = { new ("@gid", gameId) };
        int? result = this.dataLink.ExecuteScalar<int>("getNoOfRecentSalesForGame", parameters);
        return (int)result;
    }

    public Collection<Game> GetAllGames()
    {
        var result = this.dataLink.ExecuteReader("GetAllGames");
        var games = (from DataRow row in result.Rows
            select new Game
            {
                Identifier = (int)row["game_id"],
                PublisherIdentifier = (int)row["publisher_id"],
                Name = (string)row["name"],
                Description = (string)row["Description"],
                ImagePath = (string)row["image_url"],
                TrailerPath = (string)row["trailer_url"],
                GameplayPath = (string)row["gameplay_url"],
                Price = Convert.ToDecimal(row["price"]),
                MinimumRequirements = (string)row["minimum_requirements"],
                RecommendedRequirements = (string)row["recommended_requirements"],
                Status = (string)row["status"],
                Discount = (int)row["discount"],
                Tags = this.GetGameTags((int)row["game_id"]).Select(tag => tag.Tag_name).ToArray(),
                Rating = this.GetGameRating((int)row["game_id"]),
                NumberOfRecentPurchases = this.GetNoOfRecentSalesForGame((int)row["game_id"]),
                TrendingScore = Game.NOTCOMPUTED,
                TagScore = Game.NOTCOMPUTED,
            }).ToList();

        return new Collection<Game>(games);
    }
}