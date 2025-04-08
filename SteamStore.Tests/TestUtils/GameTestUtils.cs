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
            Id = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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
            PublisherId = 1
        };
    }

    public static Tag[] RandomTags()
    {
        Tag[] allTags =
        {
            new() { tag_id = 1, tag_name = "Rogue-Like" },
            new() { tag_id = 2, tag_name = "Third-Person Shooter" },
            new() { tag_id = 3, tag_name = "Multiplayer" },
            new() { tag_id = 4, tag_name = "Horror" },
            new() { tag_id = 5, tag_name = "First-Person Shooter" },
            new() { tag_id = 6, tag_name = "Action" },
            new() { tag_id = 7, tag_name = "Platformer" },
            new() { tag_id = 8, tag_name = "Adventure" },
            new() { tag_id = 9, tag_name = "Puzzle" },
            new() { tag_id = 10, tag_name = "Exploration" },
            new() { tag_id = 11, tag_name = "Sandbox" },
            new() { tag_id = 12, tag_name = "Survival" },
            new() { tag_id = 13, tag_name = "Arcade" },
            new() { tag_id = 14, tag_name = "RPG" },
            new() { tag_id = 15, tag_name = "Racing" }
        };

        var shuffled = allTags.OrderBy(tag => Random.Next()).ToList();

        var subsetSize = Random.Next(1, allTags.Length + 1);

        return shuffled.Take(subsetSize).OrderBy(tag => tag.tag_name).ToArray();
    }
}