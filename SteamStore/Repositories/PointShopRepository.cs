using SteamStore.Data;
using SteamStore.Models;
using SteamStore.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Repositories
{
    public class PointShopRepository : IPointShopRepository
    {
        private User _user;
        private IDataLink _data;

        public PointShopRepository(User user, IDataLink data)
        {
            _user = user;
            _data = data;
        }

        public List<PointShopItem> GetAllItems()
        {
            var items = new List<PointShopItem>();

            try
            {
                DataTable result = _data.ExecuteReader("GetAllPointShopItems");
                
                foreach (DataRow row in result.Rows)
                {
                    var item = new PointShopItem
                    {
                        ItemId = Convert.ToInt32(row["ItemId"]),
                        Name = row["Name"].ToString(),
                        Description = row["Description"].ToString(),
                        ImagePath = row["ImagePath"].ToString(),
                        PointPrice = Convert.ToDouble(row["PointPrice"]),
                        ItemType = row["ItemType"].ToString()
                    };
                    
                    items.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve point shop items: {ex.Message}");
            }

            return items;
        }

        public List<PointShopItem> GetUserItems()
        {
            var items = new List<PointShopItem>();

            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", _user.UserId)
                };

                DataTable result = _data.ExecuteReader("GetUserPointShopItems", parameters);
                
                foreach (DataRow row in result.Rows)
                {
                    var item = new PointShopItem
                    {
                        ItemId = Convert.ToInt32(row["ItemId"]),
                        Name = row["Name"].ToString(),
                        Description = row["Description"].ToString(),
                        ImagePath = row["ImagePath"].ToString(),
                        PointPrice = Convert.ToDouble(row["PointPrice"]),
                        ItemType = row["ItemType"].ToString(),
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    };
                    
                    items.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve user's point shop items: {ex.Message}");
            }

            return items;
        }

        public void PurchaseItem(PointShopItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Cannot purchase a null item");
            }
            
            if (_user == null)
            {
                throw new InvalidOperationException("User is not initialized");
            }
            
            if (_user.PointsBalance < item.PointPrice)
            {
                throw new Exception("Insufficient points to purchase this item");
            }

            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", _user.UserId),
                    new SqlParameter("@ItemId", item.ItemId)
                };

                _data.ExecuteNonQuery("PurchasePointShopItem", parameters);
                
                // Update user's point balance in memory
                _user.PointsBalance -= (float)item.PointPrice;
                
                // Update user's point balance in the database
                UpdateUserPointBalance();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to purchase item: {ex.Message}");
            }
        }

        public void ActivateItem(PointShopItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Cannot activate a null item");
            }
            
            if (_user == null)
            {
                throw new InvalidOperationException("User is not initialized");
            }
            
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", _user.UserId),
                    new SqlParameter("@ItemId", item.ItemId)
                };

                _data.ExecuteNonQuery("ActivatePointShopItem", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to activate item: {ex.Message}");
            }
        }

        public void DeactivateItem(PointShopItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Cannot deactivate a null item");
            }
            
            if (_user == null)
            {
                throw new InvalidOperationException("User is not initialized");
            }
            
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", _user.UserId),
                    new SqlParameter("@ItemId", item.ItemId)
                };

                _data.ExecuteNonQuery("DeactivatePointShopItem", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to deactivate item: {ex.Message}");
            }
        }

        public void UpdateUserPointBalance()
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", _user.UserId),
                    new SqlParameter("@PointBalance", _user.PointsBalance)
                };

                _data.ExecuteNonQuery("UpdateUserPointBalance", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user point balance: {ex.Message}");
            }
        }
    }
} 