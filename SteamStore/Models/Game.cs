using System;
using System.Runtime.Intrinsics.X86;
using Windows.Devices.Pwm;

public class Game
{
    public const float NOT_COMPUTED = -111111;
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public double Price { get; set; }
    public string MinimumRequirements { get; set; }
    public string RecommendedRequirements { get; set; }

    public string Status { get; set; }

    public string[] Tags { get; set; }

    public float Rating { get; set; }

    public int noOfRecentPurchases { get; set; }

    public float trendingScore { get; set; }

    public string TrailerPath { get; set; }

    public string GameplayPath { get; set; }

    public float Discount { get; set; }

    public Game(int id, string name, string description, string imagePath, string trailerPath, string gameplayPath, double price, string minimumRequirements, string recommendedRequirements, string status, string[] tags, float rating, float discount)
    {
        Id = id;
        Name = name;
        Description = description;
        ImagePath = imagePath;
        Price = price;
        TrailerPath = trailerPath;
        GameplayPath = gameplayPath;
        MinimumRequirements = minimumRequirements;
        RecommendedRequirements = recommendedRequirements;
        Status = status;
        Tags = tags;
        Rating = rating;
        Discount = discount;
    }
    public Game()
    {
    }


}
