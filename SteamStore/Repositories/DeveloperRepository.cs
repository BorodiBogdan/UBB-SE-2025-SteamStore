using System.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamStore.Pages;
using SteamStore.Models;
using System.Diagnostics;
public class DeveloperRepository
{
    private DataLink dataLink;
    private User user;

    public DeveloperRepository(DataLink dataLink, User user)
    {
        this.dataLink = dataLink;
        this.user = user;
    }
    public void ValidateGame(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id),
        };
        try
        {
            dataLink.ExecuteNonQuery("validateGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

    }
    public void CreateGame(Game game)
    {
        game.PublisherId = user.UserId;
        
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game.Id),
            new SqlParameter("@name", game.Name),
            new SqlParameter("@price", game.Price),
            new SqlParameter("@publisher_id", game.PublisherId),
            new SqlParameter("@description", game.Description),
            new SqlParameter("@image_url", game.ImagePath),
            new SqlParameter("@trailer_url", game.TrailerPath ?? ""),
            new SqlParameter("@gameplay_url", game.GameplayPath ?? ""),
            new SqlParameter("@minimum_requirements", game.MinimumRequirements),
            new SqlParameter("@recommended_requirements", game.RecommendedRequirements),
            new SqlParameter("@status", game.Status),
            new SqlParameter("@discount", game.Discount)
        };
        try
        {
            dataLink.ExecuteNonQuery("InsertGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public List<Game> GetUnvalidated()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@publisher_id", user.UserId)
        };

        DataTable? result = dataLink.ExecuteReader("GetAllUnvalidated", parameters);
        List<Game> games = new List<Game>();

        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["game_id"],
                    Name = (string)row["name"],
                    Price = Convert.ToDouble(row["price"]),
                    Description = (string)row["description"],
                    ImagePath = (string)row["image_url"],
                    TrailerPath = (string)row["trailer_url"],
                    GameplayPath = (string)row["gameplay_url"],
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Discount = Convert.ToSingle(row["discount"]),
                    PublisherId = (int)row["publisher_id"]
                };
                games.Add(game);
            }
        }
        return games;
    }
    public void DeleteGameTags(int game_id)
    {
        //System.Diagnostics.Debug.WriteLine("delete game tags in repo");
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id)
        };
        try
        {
            dataLink.ExecuteNonQuery("DeleteGameTags", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting game tags: {e.Message}");
        }
    }
    public void DeleteGame(int game_id)
    {
        try
        {
            // Delete related data from all tables in the correct order to avoid foreign key constraint violations
            
            // 1. First delete game tags
            DeleteGameTags(game_id);
            
            // 2. Delete game reviews
            SqlParameter[] reviewParams = new SqlParameter[]
            {
                new SqlParameter("@game_id", game_id)
            };
            dataLink.ExecuteNonQuery("DeleteGameReviews", reviewParams);
            
            // 3. Delete game from transaction history
            SqlParameter[] transactionParams = new SqlParameter[]
            {
                new SqlParameter("@game_id", game_id)
            };
            dataLink.ExecuteNonQuery("DeleteGameTransactions", transactionParams);
            
            // 4. Delete game from user libraries
            SqlParameter[] libraryParams = new SqlParameter[]
            {
                new SqlParameter("@game_id", game_id)
            };
            dataLink.ExecuteNonQuery("DeleteGameFromUserLibraries", libraryParams);
            
            // 5. delete the game itself
            SqlParameter[] gameParams = new SqlParameter[]
            {
                new SqlParameter("@game_id", game_id)
            };
            dataLink.ExecuteNonQuery("DeleteGameDeveloper", gameParams);
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting game: {e.Message}");
        }
    }
    public List<Game> GetDeveloperGames()
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@publisher_id", user.UserId)
        };
        DataTable? result = dataLink.ExecuteReader("GetDeveloperGames", parameters);
        List<Game> games = new List<Game>();
        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["game_id"],
                    Name = (string)row["name"],
                    Price = Convert.ToDouble(row["price"]),
                    Description = (string)row["description"],
                    ImagePath = (string)row["image_url"],
                    TrailerPath = (string)row["trailer_url"],
                    GameplayPath = (string)row["gameplay_url"],
                    MinimumRequirements = (string)row["minimum_requirements"],
                    RecommendedRequirements = (string)row["recommended_requirements"],
                    Status = (string)row["status"],
                    Discount = Convert.ToSingle(row["discount"]),
                    PublisherId = (int)row["publisher_id"]
                };
                games.Add(game);
            }
        }
        return games;
    }
    public void UpdateGame(int game_id, Game game)
    {
        game.PublisherId = user.UserId;
        
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id),
            new SqlParameter("@name", game.Name),
            new SqlParameter("@price", game.Price),
            new SqlParameter("@description", game.Description),
            new SqlParameter("@image_url", game.ImagePath),
            new SqlParameter("@trailer_url", game.TrailerPath),
            new SqlParameter("@gameplay_url", game.GameplayPath),
            new SqlParameter("@minimum_requirements", game.MinimumRequirements),
            new SqlParameter("@recommended_requirements", game.RecommendedRequirements),
            new SqlParameter("@status", game.Status),
            new SqlParameter("@discount", game.Discount),
            new SqlParameter("@publisher_id", game.PublisherId)
        };
        try
        {
            dataLink.ExecuteNonQuery("UpdateGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void RejectGame(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id)
        };
        try
        {
            dataLink.ExecuteNonQuery("RejectGame", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public void RejectGameWithMessage(int game_id, string message)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id),
            new SqlParameter("@rejection_message", message)
        };
        try
        {
            dataLink.ExecuteNonQuery("RejectGameWithMessage", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception($"Error rejecting game with message: {e.Message}");
        }
    }
    public string GetRejectionMessage(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id)
        };
        try
        {
            DataTable? result = dataLink.ExecuteReader("GetRejectionMessage", sqlParameters);
            
            if (result != null && result.Rows.Count > 0)
            {
                if (result.Rows[0]["reject_message"] != DBNull.Value)
                {
                    return result.Rows[0]["reject_message"].ToString();
                }
                return string.Empty;
            }
            
            return string.Empty;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting rejection message: {e.Message}");
        }
    }
    public Collection<Tag> GetAllTags()
    {
        Collection<Tag> tags = new Collection<Tag>();
        
        try
        {
            DataTable? result = dataLink.ExecuteReader("GetAllTags", null);
            
            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    Tag tag = new Tag
                    {
                        tag_id = (int)row["tag_id"],
                        tag_name = (string)row["tag_name"]
                    };
                    tags.Add(tag);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting tags: {e.Message}");
        }
        
        return tags;
    }
    
    public void InsertGameTag(int gameId, int tagId)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", gameId),
            new SqlParameter("@tag_id", tagId)
        };
        
        try
        {
            dataLink.ExecuteNonQuery("InsertGameTags", sqlParameters);
        }
        catch (Exception e)
        {
            throw new Exception($"Error inserting game tag: {e.Message}");
        }
    }
    public bool IsGameIdInUse(int gameId)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", gameId)
        };
        
        try
        {
            DataTable? result = dataLink.ExecuteReader("IsGameIdInUse", sqlParameters);
            
            if (result != null && result.Rows.Count > 0)
            {
                int count = Convert.ToInt32(result.Rows[0]["Result"]);
                return count > 0;
            }
            
            return false;
        }
        catch (Exception e)
        {
            throw new Exception($"Error checking if game ID is in use: {e.Message}");
        }
    }
    
    public List<Tag> GetGameTags(int gameId)
    {
        List<Tag> tags = new List<Tag>();
        
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@gid", gameId)
        };
        
        try
        {
            DataTable? result = dataLink.ExecuteReader("GetGameTags", sqlParameters);
            
            if (result != null)
            {
                foreach (DataRow row in result.Rows)
                {
                    Tag tag = new Tag
                    {
                        
                        tag_id = (int)row["tag_id"],
                        tag_name = (string)row["tag_name"]
                    };
                    System.Diagnostics.Debug.WriteLine(tag.tag_id,tag.tag_name);
                    tags.Add(tag);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting game tags: {e.Message}");
        }
        System.Diagnostics.Debug.WriteLine(tags);
        return tags;
    }
    public int GetGameOwnerCount(int game_id)
    {
        SqlParameter[] sqlParameters = new SqlParameter[]
        {
            new SqlParameter("@game_id", game_id)
        };
        
        try
        {
            DataTable? result = dataLink.ExecuteReader("GetGameOwnerCount", sqlParameters);
            
            if (result != null && result.Rows.Count > 0)
            {
                return Convert.ToInt32(result.Rows[0]["OwnerCount"]);
            }
            
            return 0;
        }
        catch (Exception e)
        {
            throw new Exception($"Error checking game ownership: {e.Message}");
        }
    }
    
    public User GetCurrentUser()
    {
        return user;
    }
}