namespace SteamStore.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PointShopItem
    {
        public PointShopItem(int itemId, string name, string description, string imagePath, double pointPrice, string itemType)
        {
            this.ItemId = itemId;
            this.Name = name;
            this.Description = description;
            this.ImagePath = imagePath;
            this.PointPrice = pointPrice;
            this.ItemType = itemType;
            this.IsActive = false;
        }

        public PointShopItem()
        {
        }

        public int ItemId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ImagePath { get; set; }

        public double PointPrice { get; set; }

        public string ItemType { get; set; } // E.g., "ProfileBackground", "Avatar", "Emoticon", etc.

        public bool IsActive { get; set; }
    }

    public class PointShopTransaction
    {
        public PointShopTransaction(int id, string itemName, double pointsSpent, string itemType)
        {
            this.Id = id;
            this.ItemName = itemName;
            this.PointsSpent = pointsSpent;
            this.PurchaseDate = DateTime.Now;
            this.ItemType = itemType;
        }

        public int Id { get; set; }

        public string ItemName { get; set; }

        public double PointsSpent { get; set; }

        public DateTime PurchaseDate { get; set; }

        public string ItemType { get; set; }
    }
}