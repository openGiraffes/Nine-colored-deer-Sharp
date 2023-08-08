using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Helper
{
    internal class ProcessViewer : IProgress<int>
    {
        public void Report(int value)
        {
            Console.WriteLine(value);
            if (value >= 100)
            {
                MainWindow.self.setLoading(false);
            }
            else
            {
                MainWindow.self.setProcess(value);
            }
        }
    }
}
