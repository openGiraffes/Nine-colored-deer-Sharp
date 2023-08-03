using Nine_colored_deer_Sharp.Helper;
using Nine_colored_deer_Sharp.utils;
using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nine_colored_deer_Sharp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow self;
        public MainWindow()
        {
            InitializeComponent();
            self = this;
            this.Loaded += MainWindow_Loaded;
        }

        //private void getMobileInfo()
        //{
        //    try
        //    {
        //        var client = kaiosHelper.getAdbClient();
        //        var device = kaiosHelper.getAdbDevice();
        //        if (device != null)
        //        {
        //            var Properties = client.GetProperties(device);
        //            var model = Properties["ro.product.model"];
        //            App.Current.Dispatcher.Invoke(() =>
        //            {
        //                txt_Model.Content = model;
        //            });
        //        }
        //        else
        //        {
        //            DialogUtil.info(grid_info, "找不到设备");
        //            App.Current.Dispatcher.Invoke(() =>
        //            {
        //                txt_Model.Content = "无设备-点击刷新";
        //            });
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (kaiosHelper.checkAdbStatus() == false)
                {
                    DialogUtil.info(grid_info, "adb服务启动失败，软件部分功能可能受限制！");
                }

                helper = new kaiosHelper();
                refreshScreen();
            });
        }

        private void SimplePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        kaiosHelper helper;
        private void btn_screenshot_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                if (helper == null)
                {
                    helper = new kaiosHelper();
                    DialogUtil.info(grid_info, "截图失败");
                    return;
                }
                var img = helper.getImage();
                if (img == null)
                {
                    DialogUtil.info(grid_info, "截图失败");
                    return;
                }
                if (Directory.Exists(Directory.GetCurrentDirectory() + "\\screenshot") == false)
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\screenshot");

                }

                var imgname = Directory.GetCurrentDirectory() + "\\screenshot\\screenshot" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png";

                File.WriteAllBytes(imgname, img);

                var image = ByteArrayToBitmapImage(img);
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.image_screen.Source = image;
                });
                DialogUtil.success(grid_info, "截图成功，保存在软件目录screenshot目录下！");

            });
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        private void refreshScreen()
        {

            Task.Run(() =>
            {
                if (helper == null)
                {
                    helper = new kaiosHelper();
                    DialogUtil.info(grid_info, "刷新截图失败");
                    return;
                }
                var img = helper.getImage();
                if (img == null)
                {
                    DialogUtil.info(grid_info, "刷新截图失败");
                    return;
                }
                var image = ByteArrayToBitmapImage(img);
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.image_screen.Source = image;
                });

            });
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            refreshScreen();
        }

        private void btn_reboot_Click(object sender, RoutedEventArgs e)
        {

            var decive = kaiosHelper.getAdbDevice();
            if (decive == null)
            {
                DialogUtil.info(grid_info, "找不到设备");
                return;
            }
            if (MessageBox.Show("是否确认重启" + decive.Model + "？", "重启确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var client = kaiosHelper.getAdbClient();
                client.Reboot(decive);
            }
        }

        private void btn_shutdown_Click(object sender, RoutedEventArgs e)
        {
            var decive = kaiosHelper.getAdbDevice();
            if (decive == null)
            {
                DialogUtil.info(grid_info, "找不到设备");
                return;
            }
            if (MessageBox.Show("是否确认关闭" + decive.Model + "？", "关机确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var client = kaiosHelper.getAdbClient();
                client.ExecuteShellCommand(decive, "reboot -p", null);
            }
        }
        private void setLoading(bool isloading)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (isloading)
                {
                    grid_loading.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_loading.Visibility = Visibility.Collapsed;
                }
                hc_loading.IsRunning = isloading;
            });

        }
        private void txt_Model_Click(object sender, RoutedEventArgs e)
        {
            setLoading(true);
            Task.Run(() =>
            {
                try
                {
                    var client = kaiosHelper.getAdbClient();
                    //client.KillAdb();
                    kaiosHelper.checkAdbStatus();

                    kaiosHelper.nowdevice = null;
                    var decive = kaiosHelper.getAdbDevice();
                    if (decive == null)
                    {
                        DialogUtil.info(grid_info, "找不到设备");
                    }
                    else
                    {
                        DialogUtil.success(grid_info, "当前选中设备：" + decive.Model);

                    }
                }
                catch (Exception ex)
                {
                    DialogUtil.info(grid_info, ex.Message);
                }
                setLoading(false);
            }
            );
        }

        private void btn_filemanage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_appmanage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
