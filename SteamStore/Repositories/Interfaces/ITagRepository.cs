using System.Collections.ObjectModel;
using SteamStore.Models;

namespace SteamStore.Repositories;

public interface ITagRepository
{
     Collection<Tag> GetAllTags();

}