using System.Data;
using System.Security.Cryptography;
using SteamStore.Models;

namespace SteamStore.Tests.TestUtils;

public static class GameTestUtils
{
    private static readonly Random Random = new();

    public static Game CreateRandomGame()
    {
        return new Game
        {
            Identifier = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Name = CommonTestUtils.RandomName(50),
            Description = CommonTestUtils.RandomName(100),
            ImagePath = CommonTestUtils.RandomPath(),
            Price = CommonTestUtils.RandomNumber(0, 1000, 2),
            TrailerPath = CommonTestUtils.RandomPath(),
            GameplayPath = CommonTestUtils.RandomPath(),
            MinimumRequirements = CommonTestUtils.RandomName(100),
            RecommendedRequirements = CommonTestUtils.RandomName(100),
            Status = CommonTestUtils.RandomElement(new[] { "Approved", "Pending" }),
            Tags = null,
            Rating = CommonTestUtils.RandomNumber(0, 10, 2),
            Discount = CommonTestUtils.RandomNumber(0, 100, 0),
            PublisherIdentifier = 1
        };
    }

    public static Tag[] RandomTags()
    {
        Tag[] allTags =
        {
            new() { TagId = 1, Tag_name = "Rogue-Like" },
            new() { TagId = 2, Tag_name = "Third-Person Shooter" },
            new() { TagId = 3, Tag_name = "Multiplayer" },
            new() { TagId = 4, Tag_name = "Horror" },
            new() { TagId = 5, Tag_name = "First-Person Shooter" },
            new() { TagId = 6, Tag_name = "Action" },
            new() { TagId = 7, Tag_name = "Platformer" },
            new() { TagId = 8, Tag_name = "Adventure" },
            new() { TagId = 9, Tag_name = "Puzzle" },
            new() { TagId = 10, Tag_name = "Exploration" },
            new() { TagId = 11, Tag_name = "Sandbox" },
            new() { TagId = 12, Tag_name = "Survival" },
            new() { TagId = 13, Tag_name = "Arcade" },
            new() { TagId = 14, Tag_name = "RPG" },
            new() { TagId = 15, Tag_name = "Racing" }
        };

        var shuffled = allTags.OrderBy(tag => Random.Next()).ToList();

        var subsetSize = Random.Next(1, allTags.Length + 1);

        return shuffled.Take(subsetSize).OrderBy(tag => tag.Tag_name).ToArray();
    }
}