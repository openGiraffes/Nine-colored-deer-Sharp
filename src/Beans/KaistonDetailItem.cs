using JsonFx.Json;
using Nine_colored_deer_Sharp.Beans;
using Nine_colored_deer_Sharp.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Beans
{
    public class KaistonDetailItem : IStonItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string subtitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Developer developer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> icons { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string package_path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long packaged_size { get; set; }
        [JsonIgnore]
        public string packaged_size_str
        {
            get
            {
                return KaiSton.FormatBytes(packaged_size);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string default_locale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> screenshots { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string theme { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<int> category_list { get; set; } = new List<int>();
        [JsonIgnore]
        static Dictionary<int, string> CATEGORY = new Dictionary<int, string>()
        {
            {110,"教育"},{30,"社交"},{100,"书籍&参考文献"},{40,"购物"},{10,"游戏"},{70,"生活"},{80,"健康"},{60,"工具"},{20,"娱乐"},{50,"新闻"},{90,"运动"}
        };
        [JsonIgnore]
        public List<string> category_list_str
        {
            get
            {
                List<string> catlist = new List<string>();

                foreach (var category in category_list)
                {
                    try
                    {
                        catlist.Add(CATEGORY[category]);
                    }
                    catch (Exception) { }
                }
                return catlist;
            }
        }
        public string _category_str;
        [JsonIgnore]
        public string category_str
        {
            get
            {
                var ret = string.Join(",", category_list_str);
                if (string.IsNullOrWhiteSpace(ret))
                {
                    ret = _category_str;
                }
                return ret;
            }
        }
        public string _icon;
        [JsonIgnore]
        public string icon
        {
            get
            {
                if (icons != null && icons.Count > 0)
                {
                    return icons[icons.Keys.First()];
                }
                return _icon;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string paid { get; set; }

    }
}
