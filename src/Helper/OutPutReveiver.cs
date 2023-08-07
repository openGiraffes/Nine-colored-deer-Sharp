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
            try
            {
                Console.WriteLine(line);
                if (line.StartsWith("/sdcard"))
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
                else if (line.StartsWith("up time"))
                {
                    var lines = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var uptime = lines[0].Replace("up time: ", "");
                    App.Current?.Dispatcher?.Invoke(new Action(() =>
                    {
                        MainWindow.self.txt_uptime.Text = string.Format("开机时间：" + uptime);
                    }));
                }
                else if (line.StartsWith("Linux version"))
                {
                    var lines = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var kenral = lines[2];
                    App.Current?.Dispatcher?.Invoke(new Action(() =>
                    {
                        MainWindow.self.txt_kenral.Text = "内核版本：" + kenral;
                    }));
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Flush()
        {

        }
    }
}
