using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Models
{
    public class Tag
    {
        public const int NOT_COMPUTED = -11111;
        public int tag_id { get; set; }
        public string tag_name { get; set; }
        public int no_of_user_games_with_tag { get; set; }
    }
}
