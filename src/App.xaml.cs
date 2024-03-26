using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Nine_colored_deer_Sharp.utils;

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


            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; 
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DialogUtil.info(Nine_colored_deer_Sharp.MainWindow.self.grid_info, "发生错误:" + e.Exception.Message.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DialogUtil.info(Nine_colored_deer_Sharp.MainWindow.self.grid_info, "发生错误:" + e.ToString());
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            DialogUtil.info(Nine_colored_deer_Sharp.MainWindow.self.grid_info, "发生错误:" + e.Exception.Message.ToString());
        }
    }
}
