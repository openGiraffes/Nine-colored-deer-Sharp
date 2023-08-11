using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Beans
{
    public interface IStonItem
    {
        string package_path { get; set; }
        string name { get; set; }
        string version { get; set; }
    }
}
