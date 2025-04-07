using System;
using System.Runtime.Intrinsics.X86;
using Windows.Devices.Pwm;

public class Game
{
   
 public Game()
    {
    }

    public const decimal NOT_COMPUTED = -111111;

    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string ImagePath { get; set; }

    public decimal Price { get; set; }

    public string MinimumRequirements { get; set; }

    public string RecommendedRequirements { get; set; }

    public string Status { get; set; }

    public string[] Tags { get; set; }

    public decimal Rating { get; set; }

    public int noOfRecentPurchases { get; set; }

    public decimal trendingScore { get; set; }

    public string TrailerPath { get; set; }

    public string GameplayPath { get; set; }

    public decimal Discount { get; set; }

    public decimal tagScore { get; set; }

    public int PublisherId { get; set; }
    
}
