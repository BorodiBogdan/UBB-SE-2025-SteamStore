using System.Collections.ObjectModel;
using System.Data;
using SteamStore.Data;
using SteamStore.Models;

namespace SteamStore.Repositories;

public class TagRepository: ITagRepository
{
    private readonly IDataLink _dataLink;

    public TagRepository(IDataLink dataLink)
    {
        _dataLink = dataLink;
    }


    public Collection<Tag> GetAllTags()
    {
        var tags = new Collection<Tag>();
        var result = _dataLink.ExecuteReader("GetAllTags");
        foreach (DataRow row in result.Rows)
        {
            var tag = new Tag
            {
                tag_id = (int)row["tag_id"],
                tag_name = (string)row["tag_name"],
                no_of_user_games_with_tag = Tag.NOT_COMPUTED

            };
            tags.Add(tag);
        }

        return tags;
    }
}