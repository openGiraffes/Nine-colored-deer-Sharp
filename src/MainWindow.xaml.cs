using Nine_colored_deer_Sharp.Helper;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void checkAdbStatus()
        {
            if (!AdbServer.Instance.GetStatus().IsRunning)
            {
                AdbServer server = new AdbServer();
                StartServerResult result = server.StartServer(Directory.GetCurrentDirectory() + "//adb//adb.exe", false);
                if (result != StartServerResult.Started)
                {
                    Console.WriteLine("Can't start adb server");
                    MessageBox.Show("adb服务启动失败，软件部分功能可能受限制！");
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            checkAdbStatus();
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

        kaiosHelper helper = new kaiosHelper();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var img = helper.getImage();

            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\screenshot") == false)
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\screenshot");

            }

            var imgname = Directory.GetCurrentDirectory() + "\\screenshot\\screenshot" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png";

            File.WriteAllBytes(imgname, img);
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
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var img = helper.getImage();
            var image = ByteArrayToBitmapImage(img);
            this.image_screen.Source = image;

        }
    }
}
