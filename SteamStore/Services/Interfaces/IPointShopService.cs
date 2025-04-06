using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Services.Interfaces
{
    public interface IPointShopService
    {
        User GetCurrentUser();
        List<PointShopItem> GetAllItems();
        List<PointShopItem> GetUserItems();
        void PurchaseItem(PointShopItem item);
        void ActivateItem(PointShopItem item);
        void DeactivateItem(PointShopItem item);
        List<PointShopItem> GetFilteredItems(string filterType, string searchText, double minPrice, double maxPrice);

    }
}
