using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Beans
{
    internal class FileItem
    {
        public string detail { get; set; }

        public string xr { get; set; }

        public string name { get; set; }

        public string parent { get; set; }

        public bool isDirectory { get; set; }

        public bool isLink { get; set; }
        public string size { get; set; }
    }
}
