using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Nine_colored_deer_Sharp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            AppCenter.LogLevel = LogLevel.Verbose;
            AppCenter.Start(Encoding.UTF8.GetString(Convert.FromBase64String("NzcwZGIxOTgtYmM2Mi00NjVmLWIyZjQtYzE1MTBjNDkxYmU2")),
                  typeof(Analytics), typeof(Crashes));

        }
    }
}
