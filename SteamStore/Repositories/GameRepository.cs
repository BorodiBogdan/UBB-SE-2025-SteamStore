using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories.Interfaces;

namespace SteamStore.Repositories;

public class GameRepository : IGameRepository
{
    private readonly IDataLink _dataLink;

    public GameRepository(IDataLink dataLink)
    {
        _dataLink = dataLink;
    }

    public void ValidateGame(int gameId)
    {
        var sqlParameters = new[]
        {
            new SqlParameter("@game_id", gameId),
        };
        _dataLink.ExecuteNonQuery("validateGame", sqlParameters);
    }

    public List<Game> GetUnvalidated(int userId)
    {
        var parameters = new[] { new SqlParameter("@publisher_id", userId) };

        var result = _dataLink.ExecuteReader("GetAllUnvalidated", parameters);
        var games = new List<Game>();

        foreach (DataRow row in result.Rows)
        {
            var game = new Game
            {
                Id = (int)row["game_id"],
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
                PublisherId = (int)row["publisher_id"]
            };
            games.Add(game);
        }

        return games;
    }

    public void DeleteGameTags(int gameId)
    {
        var sqlParameters = new[] { new SqlParameter("@game_id", gameId) };
        _dataLink.ExecuteNonQuery("DeleteGameTags", sqlParameters);
    }

    public void DeleteGame(int gameId)
    {
        // Delete related data from all tables in the correct order to avoid foreign key constraint violations
        
        // 1. First delete game tags
        DeleteGameTags(gameId);

        // 2. Delete game reviews
        SqlParameter[] reviewParams = { new("@game_id", gameId) };
        _dataLink.ExecuteNonQuery("DeleteGameReviews", reviewParams);

        // 3. Delete game from transaction history
        SqlParameter[] transactionParams = { new("@game_id", gameId) };
        _dataLink.ExecuteNonQuery("DeleteGameTransactions", transactionParams);

        // 4. Delete game from user libraries
        SqlParameter[] libraryParams = { new("@game_id", gameId) };
        _dataLink.ExecuteNonQuery("DeleteGameFromUserLibraries", libraryParams);

        // 5. delete the game itself
        SqlParameter[] gameParams = { new("@game_id", gameId) };
        _dataLink.ExecuteNonQuery("DeleteGameDeveloper", gameParams);
    }

    public List<Game> GetDeveloperGames(int userId)
    {
        SqlParameter[] parameters = { new("@publisher_id", userId) };
        var result = _dataLink.ExecuteReader("GetDeveloperGames", parameters);

        return (from DataRow row in result.Rows
            select new Game
            {
                Id = (int)row["game_id"],
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
                PublisherId = (int)row["publisher_id"]
            }).ToList();
    }

    public void UpdateGame(int gameId, Game game)
    {
        SqlParameter[] sqlParameters =
        {
            new("@game_id", gameId),
            new("@name", game.Name),
            new("@price", game.Price),
            new("@description", game.Description),
            new("@image_url", game.ImagePath),
            new("@trailer_url", game.TrailerPath),
            new("@gameplay_url", game.GameplayPath),
            new("@minimum_requirements", game.MinimumRequirements),
            new("@recommended_requirements", game.RecommendedRequirements),
            new("@status", game.Status),
            new("@discount", game.Discount),
            new("@publisher_id", game.PublisherId)
        };
        _dataLink.ExecuteNonQuery("UpdateGame", sqlParameters);
    }

    public void RejectGame(int gameId)
    {
        SqlParameter[] sqlParameters = { new("@game_id", gameId) };
        _dataLink.ExecuteNonQuery("RejectGame", sqlParameters);
    }

    public void RejectGameWithMessage(int gameId, string message)
    {
        SqlParameter[] sqlParameters =
        {
            new("@game_id", gameId),
            new("@rejection_message", message)
        };
        _dataLink.ExecuteNonQuery("RejectGameWithMessage", sqlParameters);
    }

    public string GetRejectionMessage(int gameId)
    {
        var sqlParameters = new[] { new SqlParameter("@game_id", gameId) };

        var result = _dataLink.ExecuteReader("GetRejectionMessage", sqlParameters);

        if (result == null || result.Rows.Count <= 0) return string.Empty;
        return result.Rows[0]["reject_message"] != DBNull.Value
            ? result.Rows[0]["reject_message"].ToString()
            : string.Empty;
    }


    public void InsertGameTag(int gameId, int tagId)
    {
        var sqlParameters = new SqlParameter[]
        {
            new("@game_id", gameId),
            new("@tag_id", tagId)
        };

        _dataLink.ExecuteNonQuery("InsertGameTags", sqlParameters);
    }

    public bool IsGameIdInUse(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new("@game_id", gameId) };

        var result = _dataLink.ExecuteReader("IsGameIdInUse", sqlParameters);

        if (result == null || result.Rows.Count <= 0) return false;
        var count = Convert.ToInt32(result.Rows[0]["Result"]);
        return count > 0;
    }

    public List<Tag> GetGameTags(int gameId)
    {
        var sqlParameters = new SqlParameter[] { new("@gid", gameId) };

        var result = _dataLink.ExecuteReader("GetGameTags", sqlParameters);
        return (from DataRow row in result.Rows
                select new Tag { tag_id = (int)row["tag_id"], tag_name = (string)row["tag_name"] })
            .OrderBy(tag => tag.tag_name).ToList();
    }


    public void CreateGame(Game game)
    {
        var sqlParameters = new SqlParameter[]
        {
            new("@game_id", game.Id),
            new("@name", game.Name),
            new("@price", game.Price),
            new("@publisher_id", game.PublisherId),
            new("@description", game.Description),
            new("@image_url", game.ImagePath),
            new("@trailer_url", game.TrailerPath ?? ""),
            new("@gameplay_url", game.GameplayPath ?? ""),
            new("@minimum_requirements", game.MinimumRequirements),
            new("@recommended_requirements", game.RecommendedRequirements),
            new("@status", game.Status),
            new("@discount", game.Discount)
        };
        _dataLink.ExecuteNonQuery("InsertGame", sqlParameters);
    }


    public decimal GetGameRating(int gameId)
    {
        SqlParameter[] parameters = {new("@gid", gameId)};

        decimal? result = _dataLink.ExecuteScalar<decimal>("getGameRating", parameters);
        return (decimal)result;
    }

    public int GetNoOfRecentSalesForGame(int gameId)
    {
        SqlParameter[] parameters ={new("@gid", gameId)};
        int? result = _dataLink.ExecuteScalar<int>("getNoOfRecentSalesForGame", parameters);
        return (int)result;
    }

    public Collection<Game> GetAllGames()
    {
        var result = _dataLink.ExecuteReader("GetAllGames");
        var games = (from DataRow row in result.Rows
            select new Game
            {
                Id = (int)row["game_id"],
                PublisherId = (int)row["publisher_id"],
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
                Tags = GetGameTags((int)row["game_id"]).Select(tag => tag.tag_name).ToArray(),
                Rating = GetGameRating((int)row["game_id"]),
                noOfRecentPurchases = GetNoOfRecentSalesForGame((int)row["game_id"]),
                trendingScore = Game.NOT_COMPUTED,
                tagScore = Game.NOT_COMPUTED
            }).ToList();

        return new Collection<Game>(games);
    }
}