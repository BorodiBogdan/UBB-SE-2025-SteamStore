using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;


public class GameRepository
{
    private readonly DataLink dataLink;
    public GameRepository(DataLink dataLink)
    {
        this.dataLink = dataLink;
    }

    public int CreateGame(Game game)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@Name", game.Name),
            new SqlParameter("@Description", game.Description),
            new SqlParameter("@ImagePath", game.ImagePath),
            new SqlParameter("@Price", game.Price),
            new SqlParameter("@ReleaseDate", game.ReleaseDate),
            new SqlParameter("@Developer", game.Developer),
            new SqlParameter("@MinimumRequirements", game.MinimumRequirements),
            new SqlParameter("@RecommendedRequirements", game.RecommendedRequirements),
            new SqlParameter("@Status", game.Status)
        };

        try
        {
            int? result = dataLink.ExecuteScalar<int>("CreateGame", parameters);
            return result ?? 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 0;
        }
    }
    public Collection<Game> getAllGames()
    {

        DataTable? result = dataLink.ExecuteReader("GetAllGames");
        List<Game> games = new List<Game>();
        
        if (result != null)
        {
            foreach (DataRow row in result.Rows)
            {
                Game game = new Game
                {
                    Id = (int)row["Id"],
                    Name = (string)row["Name"],
                    Description = (string)row["Description"],
                    ImagePath = (string)row["ImagePath"],
                    Price = (double)row["Price"],
                    ReleaseDate = (DateTime)row["ReleaseDate"],
                    Developer = (string)row["Developer"],
                    MinimumRequirements = (string)row["MinimumRequirements"],
                    RecommendedRequirements = (string)row["RecommendedRequirements"],
                    Status = (string)row["Status"]
                };
                games.Add(game);
            }
        }
        
        return new Collection<Game>(games);
    }
}