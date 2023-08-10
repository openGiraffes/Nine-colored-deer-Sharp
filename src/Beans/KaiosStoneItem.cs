using Newtonsoft.Json;
using Nine_colored_deer_Sharp.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Beans
{
    public class KaiosStoneItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string display { get; set; }
        public string version { get; set; }
        public string manifest_url { get; set; }

        public Dictionary<string, string> icons { get; set; }
        [JsonIgnore]
        public string packaged_size_str
        {
            get
            {
                return "未知";
            }
        }

        public string icon
        {
            get
            {
                if (icons != null && icons.Count > 0)
                {
                    return icons[icons.Keys.First()];
                }
                return "";
            }
        }

        public string package_path { get; set; }
    }
}
