using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Models
{
    public class PointShopItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public double PointPrice { get; set; }
        public string ItemType { get; set; } // E.g., "ProfileBackground", "Avatar", "Emoticon", etc.
        public bool IsActive { get; set; }

        public PointShopItem(int itemId, string name, string description, string imagePath, double pointPrice, string itemType)
        {
            ItemId = itemId;
            Name = name;
            Description = description;
            ImagePath = imagePath;
            PointPrice = pointPrice;
            ItemType = itemType;
            IsActive = false;
        }

        public PointShopItem()
        {
        }
    }
} 