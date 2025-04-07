using SteamStore.Repositories;
using SteamStore.Tests.TestUtils;

namespace SteamStore.Tests.Repositories;

public class TagRepositoryTest
{
    private readonly TagRepository _subject = new(TestDataLink.GetDataLink());

    [Fact]
    public void GetAllTags()
    {
        var tags = _subject.GetAllTags();
        Assert.Equal(15, tags.Count);
        var tagNames = tags.Select(tag => tag.tag_name).ToList();
        Assert.Equal(new List<string>
        {
            "Rogue-Like",
            "Third-Person Shooter",
            "Multiplayer",
            "Horror",
            "First-Person Shooter",
            "Action",
            "Platformer",
            "Adventure",
            "Puzzle",
            "Exploration",
            "Sandbox",
            "Survival",
            "Arcade",
            "RPG",
            "Racing"
        }, tagNames);
    }
}