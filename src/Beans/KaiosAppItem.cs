using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Beans
{
    public class KaiosAppItem
    {
        public string manifestURL { get; set; }
        public string name { get; set; }

        public string oldVersion { get; set; }

        public Manifest manifest { get; set; }

    }

    public class Developer
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    public class Manifest
    {
        public string name { get; set; }
        public string description { get; set; }

        public string version { get; set; }
        public string type { get; set; }

        public Developer developer { get; set; }

    }
}
