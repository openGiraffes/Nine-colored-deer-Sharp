using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Beans
{
    public class Download
    {
        /// <summary>
        /// 
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string manifest { get; set; }

        public string version { get; set; }
    }

    public class Meta
    {
        /// <summary>
        /// 
        /// </summary>
        public string tags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> categories { get; set; }
    }

    public class BHAppItem
    {
        public KaistonDetailItem convertToKaistonDetail()
        {
            KaistonDetailItem ret = new KaistonDetailItem();

            ret.name = this.name;
            ret._icon = this.icon;
            ret.description = this.description;

            ret.package_path = this.download.url;

            if (ret.package_path.StartsWith("https://git.yumenaka.net/"))
            {
                ret.package_path = ret.package_path.Replace("https://git.yumenaka.net/", "https://kaios.tri1.workers.dev/?url=");
            }

            if (ret.package_path.StartsWith("https://kaios.tri1.workers.dev/?url="))
            {
                ret.package_path = ret.package_path.Replace("https://kaios.tri1.workers.dev/?url=", "https://ghproxy.com/");
            }
            if (ret.package_path.StartsWith("https://raw.githubusercontent.com/") || ret.package_path.StartsWith("https://www.github.com/"))
            {
                ret.package_path = "https://ghproxy.com/" + ret.package_path;
            }
            //if (ret.package_path.StartsWith("https://groups.google.com/"))
            //{
            //    ret.package_path = ret.package_path.Replace("https://groups.google.com/", "https://74.125.206.210/");
            //}

            ret.developer = new Developer();
            ret.developer.name = string.Join(",", this.author);
            ret._category_str = string.Join(",", this.meta.categories);

            ret.version = this.download.version;
            ret.type = this.type;
            int i = 0;

            ret.screenshots = new Dictionary<string, string>();
            foreach (var item in screenshots)
            {
                ret.screenshots.Add(i.ToString(), item);
                i++;
            }
            return ret;
        }
        public KaiosStoneItem convertToKaistonItem()
        {
            KaiosStoneItem ret = new KaiosStoneItem();

            ret.name = this.name;
            ret.thumbnail_url = this.icon;
            ret.description = this.description;
            ret.package_path = this.download.url;
            ret.bHAppItem = this;
            ret.version = this.download.version;

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string icon { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string git_repo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Download download { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string license { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> author { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> maintainer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string has_ads { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string has_tracking { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Meta meta { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string slug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> screenshots { get; set; }
    }
}
