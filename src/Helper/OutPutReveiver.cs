using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Helper
{
    internal class OutPutReveiver : IShellOutputReceiver
    {
        public bool ParsesErrors => throw new NotImplementedException();

        public void AddOutput(string line)
        {
            Console.WriteLine(line);
            if (line.StartsWith("/data"))
            {
                var lines = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var Size = lines[1];
                var Used = lines[2];
                var Free = lines[3];

                App.Current?.Dispatcher?.Invoke(new Action(() =>
                {
                    MainWindow.self.txt_memory.Text = string.Format("用户空间大小：{0}，已用：{1}，剩余：{2}", Size, Used, Free);
                }));
            }
        }

        public void Flush()
        {

        }
    }
}
