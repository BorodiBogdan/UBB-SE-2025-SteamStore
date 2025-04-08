// <copyright file="TagRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SteamStore.Repositories;
using System.Collections.ObjectModel;
using System.Data;
using SteamStore.Data;
using SteamStore.Models;

public class TagRepository : ITagRepository
{
    private readonly IDataLink dataLink;

    public TagRepository(IDataLink dataLink)
    {
        this.dataLink = dataLink;
    }

    public Collection<Tag> GetAllTags()
    {
        var tags = new Collection<Tag>();
        var result = this.dataLink.ExecuteReader("GetAllTags");
        foreach (DataRow row in result.Rows)
        {
            var tag = new Tag
            {
                TagId = (int)row["tag_id"],
                Tag_name = (string)row["tag_name"],
                NumberOfUserGamesWithTag = Tag.NOTCOMPUTED,
            };
            tags.Add(tag);
        }

        return tags;
    }
}