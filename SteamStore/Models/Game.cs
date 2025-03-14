using System;
using Windows.Devices.Pwm;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImagePath { get; set; }
    public double Price { get; set; }
    public string MinimumRequirements { get; set; }
    public string RecommendedRequirements { get; set; }

    public string Status { get; set; }
}
