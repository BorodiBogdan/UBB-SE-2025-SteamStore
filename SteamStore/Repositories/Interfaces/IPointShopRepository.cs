using SteamStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Repositories.Interfaces
{
    public interface IPointShopRepository
    {
        List<PointShopItem> GetAllItems();
        List<PointShopItem> GetUserItems();
        void PurchaseItem(PointShopItem item);
        void ActivateItem(PointShopItem item);
        void DeactivateItem(PointShopItem item);
        void UpdateUserPointBalance();

    }
}
