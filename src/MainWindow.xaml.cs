using Nine_colored_deer_Sharp.Beans;
using Nine_colored_deer_Sharp.Helper;
using Nine_colored_deer_Sharp.utils;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using System;
using System.Collections;
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
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Nine_colored_deer_Sharp.Converts;
using System.Windows.Markup;
using EasyHttp.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;

namespace Nine_colored_deer_Sharp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow self;
        bool isclosed = false;
        public MainWindow()
        {
            InitializeComponent();
            int MYTHREAD_COUNT = 20;
            ThreadPool.SetMinThreads(MYTHREAD_COUNT, MYTHREAD_COUNT);
            ThreadPool.SetMaxThreads(MYTHREAD_COUNT * 5, MYTHREAD_COUNT * 5);

            grid_info.Visibility = Visibility.Visible;
            self = this;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isclosed = true;
            try
            {
                AdbClient adbClient = new AdbClient();
                adbClient.KillAdb();
            }
            catch (Exception ex)
            {

            }
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
        //            App.Current?.Dispatcher?.Invoke(() =>
        //            {
        //                txt_Model.Content = model;
        //            });
        //        }
        //        else
        //        {
        //            DialogUtil.info(grid_info, "找不到设备");
        //            App.Current?.Dispatcher?.Invoke(() =>
        //            {
        //                txt_Model.Content = "无设备-点击刷新";
        //            });
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        bool isload = false;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txt_title.Text = "KaiOS手机助手 v" + getVersion();
            MulitImageCacheConverter.CacheInit(System.IO.Path.Combine(Environment.CurrentDirectory, "Cache"), 3000, 3);

            setLoading(true);
            Task.Run(() =>
            {
                if (kaiosHelper.checkAdbStatus() == false)
                {
                    DialogUtil.info(grid_info, "adb服务启动失败，软件部分功能可能受限制！");
                }
                helper = new kaiosHelper();
                refreshScreen();
                setLoading(false);
            });
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        if (isclosed)
                        {
                            break;
                        }
                        if (autorefresh)
                        {
                            if (helper != null)
                            {
                                refreshScreen(false);
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(100);
                    }
                }
            });
            isload = true;
        }

        private string getVersion()
        {
            try
            {
                AssemblyName assemblyName2 = Assembly.GetExecutingAssembly().GetName();
                Version version2 = assemblyName2.Version;
                string version = version2.ToString();
                string[] t = version.Split('.');
                return string.Format("{0}.{1}.{2}", t[0], t[1], t[2]);
            }
            catch
            {
                return "1.0.0";
            }
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
                    DialogUtil.info(grid_info, "截图失败");
                    return;
                }
                byte[] img = null;
                if (autorefresh)
                {
                    img = lstimg;
                }
                else
                {
                    img = helper.getImage();
                }
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
                App.Current?.Dispatcher?.Invoke(() =>
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
        byte[] lstimg = null;
        private void refreshScreen(bool showtips = true)
        {

            if (helper == null)
            {
                if (showtips)
                {
                    DialogUtil.info(grid_info, "刷新截图失败");
                    return;
                }
            }
            var img = helper.getImage();
            if (img == null)
            {
                if (showtips)
                {
                    DialogUtil.info(grid_info, "刷新截图失败");
                }
                return;
            }
            lstimg = img;
            var image = ByteArrayToBitmapImage(img);
            App.Current?.Dispatcher?.Invoke(() =>
            {
                this.image_screen.Source = image;
            });
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            if (autorefresh)
            {
                DialogUtil.info(grid_info, "当前已开启自动刷新，不需要手动刷新");
                return;
            }
            Task.Run(() =>
            {
                refreshScreen();
            });
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
        public void setLoading(bool isloading)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                if (isloading)
                {
                    grid_loading.Visibility = Visibility.Visible;
                    hc_loading.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_loading.Visibility = Visibility.Collapsed;
                    hc_loading.Visibility = Visibility.Collapsed;
                }
                hc_progress.Visibility = Visibility.Collapsed;
                hc_text_process.Visibility = Visibility.Collapsed;
                hc_loading.IsRunning = isloading;
            });
        }
        public void setProcess(int value)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                grid_loading.Visibility = Visibility.Visible;
                hc_loading.Visibility = Visibility.Collapsed;
                hc_text_process.Visibility = Visibility.Visible;
                hc_progress.Visibility = Visibility.Visible;
                hc_loading.IsRunning = false;
                hc_progress.Value = value;
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
                    try
                    {
                        client.RemoveAllForwards(kaiosHelper.nowdevice);
                    }
                    catch (Exception ex)
                    {

                    }
                    kaiosHelper.nowdevice = null;
                    var device = kaiosHelper.getAdbDevice();
                    if (device == null)
                    {
                        DialogUtil.info(grid_info, "找不到设备");
                    }
                    else
                    {
                        try
                        {
                            client.CreateForward(device, "tcp:6000", "localfilesystem:/data/local/debugger-socket", false);
                        }
                        catch (Exception ex)
                        {

                        }
                        DialogUtil.success(grid_info, "当前选中设备：" + device.Model);
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
            //SimKey("*");
        }

        Dictionary<string, string> KeyCodeList = new Dictionary<string, string>()
        {
            { "1","2"},
            { "2","3"},
            { "3","4"},
            { "4","5"},
            { "5","6"},
            { "6","7"},
            { "7","8"},
            { "8","9"},
            { "9","10"},
            { "0","11"},
            { "*","522"},
            { "#","523"},
            { "left","105"},
            { "right","106"},
            { "up","103"},
            { "down","108"},
            { "softleft","139"},
            { "softright","158"},
            { "call","231"},
            { "menu","212"},
            { "back","14"},
            { "power","107"},
            { "ok","352"}
        };

        private void SimKey(string keycode)
        {
            var client = kaiosHelper.getAdbClient();
            var device = kaiosHelper.getAdbDevice();
            if (device == null)
            {
                return;
            }


            var key = KeyCodeList[keycode].PadLeft(4, '0');

            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0001 " + key + " 00000001", null);
            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0000 0000 00000000", null);

            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0001 " + key + " 00000000", null);
            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0000 0000 00000000", null);

        }

        private void SimKeyDown(string keycode)
        {
            var client = kaiosHelper.getAdbClient();
            var device = kaiosHelper.getAdbDevice();
            if (device == null)
            {
                return;
            }
            var key = KeyCodeList[keycode].PadLeft(4, '0');

            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0001 " + key + " 00000001", null);
            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0000 0000 00000000", null);

        }
        private void SimKeyUp(string keycode)
        {
            var client = kaiosHelper.getAdbClient();
            var device = kaiosHelper.getAdbDevice();
            if (device == null)
            {
                return;
            }
            var key = KeyCodeList[keycode].PadLeft(4, '0');

            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0001 " + key + " 00000000", null);
            client.ExecuteShellCommand(device, "sendevent /dev/input/event1 0000 0000 00000000", null);

        }

        private void btn_appmanage_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tag = (sender as Button)?.Tag.ToString();
            if (string.IsNullOrWhiteSpace(tag) == false)
            {
                SimKeyDown(tag);
            }
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tag = (sender as Button)?.Tag.ToString();
            if (string.IsNullOrWhiteSpace(tag) == false)
            {
                SimKeyUp(tag);
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        bool autorefresh = false;
        private void ckb_autorefresh_Click(object sender, RoutedEventArgs e)
        {
            var ischeck = ckb_autorefresh.IsChecked == true;
            autorefresh = ischeck;
        }
        List<KaiosStoneItem> allapps { get; set; }
        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabs.SelectedIndex == 1)
            {
                if (itemsfiles.ItemsSource == null)
                {
                    itemsfiles.ItemsSource = await getRootFiles();
                }
            }
            else if (tabs.SelectedIndex == 2)
            {
                if (itemapps.ItemsSource == null)
                {
                    await Task.Run(async () =>
                    {
                        while (helper == null)
                        {
                            Thread.Sleep(100);
                        }
                        await refreshApps();
                    });

                }
            }
            else if (tabs.SelectedIndex == 3)
            {
                if (itemskaistonlist.ItemsSource != null)
                {
                    return;
                }
                getGuanfangApps();
            }
        }
        /// <summary>
        /// 获取官方商店数据
        /// </summary>
        private void getGuanfangApps()
        {
            setLoading(true);
            Task.Run(() =>
            {
                try
                {

                    KaiSton.getKey();

                    string apps = "";
                    if (File.Exists("apps.json") == false)
                    {
                        var ret = KaiSton.Request("GET", "/v3.0/apps?software=KaiOS_2.5.4.1&locale=zh-CN", "");//&category=30&page_num=1&page_size=20 
                        apps = JObject.Parse(ret)["apps"].ToString(Formatting.None);
                        File.WriteAllText("apps.json", apps);
                    }
                    else
                    {
                        apps = File.ReadAllText("apps.json");
                    }

                    allapps = JsonConvert.DeserializeObject<List<KaiosStoneItem>>(apps);
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        itemskaistonlist.ItemsSource = SplitPages(allapps);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    DialogUtil.info(this.grid_info, "加载数据出错：" + ex.Message);
                }
                finally
                {

                    setLoading(false);
                }
            });
        }
        /// <summary>
        /// 获取长颈鹿商店源
        /// </summary>
        private void getOpengiraffesApps()
        {
            setLoading(true);
            Task.Run(() =>
            {
                try
                {

                    KaiSton.getKey();

                    string apps = "";
                    if (File.Exists("opengiraffesapps.json") == false)
                    {
                        HttpClient httpClient = new HttpClient();
                        var ret = httpClient.Get("https://storedb.opengiraffes.top/data.json");
                        apps = ret.RawText;
                        File.WriteAllText("opengiraffesapps.json", apps);
                    }
                    else
                    {
                        apps = File.ReadAllText("opengiraffesapps.json");
                    }
                    apps = JObject.Parse(apps)["apps"].ToString();
                    var t = JsonConvert.DeserializeObject<List<BHAppItem>>(apps);

                    allapps = new List<KaiosStoneItem>();

                    foreach (var item in t)
                    {
                        allapps.Add(item.convertToKaistonItem());
                    }

                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        itemskaistonlist.ItemsSource = SplitPages(allapps);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    DialogUtil.info(this.grid_info, "加载数据出错：" + ex.Message);
                }
                finally
                {

                    setLoading(false);
                }
            });
        }
        /// <summary>
        /// 获取BH源
        /// </summary>
        private void geBHApps()
        {
            setLoading(true);
            Task.Run(() =>
            {
                try
                {

                    KaiSton.getKey();

                    string apps = "";
                    if (File.Exists("bhapps.json") == false)
                    {
                        HttpClient httpClient = new HttpClient();
                        var ret = httpClient.Get("https://banana-hackers.gitlab.io/store-db/data.json");
                        apps = ret.RawText;
                        File.WriteAllText("bhapps.json", apps);
                    }
                    else
                    {
                        apps = File.ReadAllText("bhapps.json");
                    }
                    apps = JObject.Parse(apps)["apps"].ToString();
                    var t = JsonConvert.DeserializeObject<List<BHAppItem>>(apps);

                    allapps = new List<KaiosStoneItem>();

                    foreach (var item in t)
                    {
                        allapps.Add(item.convertToKaistonItem());
                    }

                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        itemskaistonlist.ItemsSource = SplitPages(allapps);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    DialogUtil.info(this.grid_info, "加载数据出错：" + ex.Message);
                }
                finally
                {

                    setLoading(false);
                }
            });
        }
        private string NowPath = "";
        private async Task<List<FileItem>> getRootFiles(string path = "/")
        {
            var client = kaiosHelper.getAdbClient();
            var device = kaiosHelper.getAdbDevice();
            if (device == null)
            {
                return null;
            }

            FileReceiver fileReceiver = new FileReceiver();
            await client.ExecuteRemoteCommandAsync("ls -la \"" + path + "\" -F", device, fileReceiver, Encoding.UTF8, CancellationToken.None);

            while (fileReceiver.isCompleted == false)
            {
                Thread.Sleep(100);
            }
            if (fileReceiver.FileList != null)
            {
                foreach (var f in fileReceiver.FileList)
                {
                    f.parent = path;
                }
            }
            NowPath = path;
            txt_nowdir.Text = NowPath;
            return fileReceiver.FileList;
        }

        private async void grid_fileinfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {

                    FileItem item = (sender as Grid)?.Tag as FileItem;
                    if (item != null)
                    {
                        if (item.isDirectory)
                        {
                            string path = "";
                            if (item.parent.EndsWith("/"))
                            {
                                path = item.parent + item.name + "/";
                            }
                            else
                            {
                                path = item.parent + "/" + item.name + "/";
                            }

                            itemsfiles.ItemsSource = await getRootFiles(path);
                        }
                        else if (item.isLink)
                        {
                            var link = item.name.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                            itemsfiles.ItemsSource = await getRootFiles(link);
                            //DialogUtil.info(grid_info, "暂时不可以操作link");
                        }
                        else
                        {
                            var device = kaiosHelper.getAdbDevice();
                            if (device != null)
                            {
                                var path = "";
                                if (item.parent.EndsWith("/"))
                                {
                                    path = item.parent + item.name;
                                }
                                else
                                {
                                    path = item.parent + "/" + item.name;
                                }
                                if (MessageBox.Show("是否下载文件:" + path + "？", "下载确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    try
                                    {
                                        if (Directory.Exists(Directory.GetCurrentDirectory() + "/download") == false)
                                        {
                                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/download");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    var downpath = Directory.GetCurrentDirectory() + "\\download\\" + item.name;
                                    var shelldata = path;
                                    hc_text_process.Text = "正在下载文件";
                                    await Task.Run(() =>
                                    {
                                        kaiosHelper.DownloadFile(shelldata, downpath, new ProcessViewer());
                                        DialogUtil.success(grid_info, "成功下载 " + path + " 到 download 目录");
                                    });

                                }
                            }
                            else
                            {
                                DialogUtil.info(grid_info, "无法获取到当前设备状态");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }
        private async void refreshFileList()
        {
            try
            {
                var file = NowPath;
                if (string.IsNullOrWhiteSpace(file))
                {
                    itemsfiles.ItemsSource = await getRootFiles();
                }
                else
                {
                    itemsfiles.ItemsSource = await getRootFiles(file);
                }
            }
            catch (Exception ex)
            {

                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
                Console.WriteLine(ex.Message);
            }
        }
        private async void btn_refreshFileList_Click(object sender, RoutedEventArgs e)
        {
            refreshFileList();
        }

        private async void btn_uploadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = NowPath;
                if (string.IsNullOrWhiteSpace(file))
                {
                    file = "/";
                }
                var openFileDialog = new Microsoft.Win32.OpenFileDialog()
                {
                    Filter = "All files (*.*)|*.*"
                };
                var result = openFileDialog.ShowDialog();
                if (result == true)
                {

                    var filename = openFileDialog.FileName;
                    var fname = System.IO.Path.GetFileName(filename);
                    if (file.EndsWith("/"))
                    {
                        file = file + fname;
                    }
                    else
                    {
                        file = file + "/" + fname;
                    }
                    hc_text_process.Text = "正在上传文件";
                    await Task.Run(() =>
                    {
                        kaiosHelper.UploadFile(file, filename, new ProcessViewer());
                        DialogUtil.success(grid_info, "已成功上传至" + file);
                    });
                    refreshFileList();
                }
            }
            catch (Exception ex)
            {

                DialogUtil.info(grid_info, "上传失败" + ex.Message);
            }
        }

        private async void btn_newDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputDialog inputDialog = new InputDialog();
                inputDialog.Owner = this;
                inputDialog.ShowDialog();
                if (string.IsNullOrWhiteSpace(inputDialog.value))
                {
                    return;
                }
                var filename = inputDialog.value;

                var device = kaiosHelper.getAdbDevice();
                if (device != null)
                {
                    var client = kaiosHelper.getAdbClient();
                    var path = NowPath;
                    if (path.EndsWith("/"))
                    {
                        path = NowPath + filename;
                    }
                    else
                    {
                        path = NowPath + "/" + filename;
                    }
                    path = "mkdir \"" + path + "\"";

                    await client.ExecuteRemoteCommandAsync(path, device, null, CancellationToken.None);
                    refreshFileList();
                }
                else
                {
                    DialogUtil.info(grid_info, "无法获取到当前设备状态");
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        private async void btn_return_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string file = "";
                if (NowPath.EndsWith("/") && NowPath != "/")
                {
                    file = NowPath.Substring(0, NowPath.LastIndexOf("/", NowPath.Length - 2));
                }
                else
                {
                    file = NowPath.Substring(0, NowPath.LastIndexOf("/"));
                }
                if (string.IsNullOrWhiteSpace(file))
                {
                    itemsfiles.ItemsSource = await getRootFiles();
                }
                else
                {
                    itemsfiles.ItemsSource = await getRootFiles(file);
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private void grid_fileinfo_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menu = sender as MenuItem;
                var header = menu.Header.ToString();
                if (header == "刷新列表")
                {
                    refreshFileList();
                    return;
                }
                if (menu != null)
                {
                    var cm = menu.Parent as ContextMenu;
                    if (cm == null)
                    {
                        return;
                    }
                    var grid = cm.PlacementTarget as Grid;
                    if (grid != null)
                    {
                        var tag = grid.Tag as FileItem;
                        if (tag != null)
                        {
                            if (header == "下载文件")
                            {
                                if (tag.isDirectory)
                                {
                                    DialogUtil.info(grid_info, "文件夹请手动下载！");
                                    return;
                                }
                                var device = kaiosHelper.getAdbDevice();
                                if (device != null)
                                {
                                    var path = "";
                                    if (tag.parent.EndsWith("/"))
                                    {
                                        path = tag.parent + tag.name;
                                    }
                                    else
                                    {
                                        path = tag.parent + "/" + tag.name;
                                    }
                                    try
                                    {
                                        if (Directory.Exists(Directory.GetCurrentDirectory() + "/download") == false)
                                        {
                                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/download");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    var downpath = Directory.GetCurrentDirectory() + "\\download\\" + tag.name;
                                    var shelldata = path;
                                    hc_text_process.Text = "正在下载文件";
                                    await Task.Run(() =>
                                    {
                                        kaiosHelper.DownloadFile(shelldata, downpath, new ProcessViewer());
                                        DialogUtil.success(grid_info, "成功下载 " + path + " 到 download 目录");
                                    });
                                }
                                else
                                {
                                    DialogUtil.info(grid_info, "无法获取到当前设备状态");
                                }
                            }
                            else if (header == "删除文件")
                            {
                                var device = kaiosHelper.getAdbDevice();
                                if (device != null)
                                {
                                    var path = "";
                                    if (tag.parent.EndsWith("/"))
                                    {
                                        path = tag.parent + tag.name;
                                    }
                                    else
                                    {
                                        path = tag.parent + "/" + tag.name;
                                    }
                                    if (path.Contains("*") || string.IsNullOrWhiteSpace(path) || path == "/")
                                    {
                                        DialogUtil.info(grid_info, "危险操作，文件名包含*？已经取消操作！");
                                        return;
                                    }
                                    if (tag.isDirectory || tag.isLink)
                                    {
                                        if (MessageBox.Show("是否删除文件夹:" + path + "？", "删除确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                        {
                                            var client = kaiosHelper.getAdbClient();
                                            client.ExecuteShellCommand(device, "rm -r \"" + path + "\"", null);
                                            DialogUtil.success(grid_info, path + "删除完毕！");
                                            refreshFileList();
                                            return;
                                        }
                                        return;
                                    }
                                    if (MessageBox.Show("是否删除文件:" + path + "？", "删除确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        var client = kaiosHelper.getAdbClient();
                                        client.ExecuteShellCommand(device, "rm -rf \"" + path + "\"", null);
                                        DialogUtil.success(grid_info, path + "删除成功！");
                                    }
                                    refreshFileList();
                                }
                                else
                                {
                                    DialogUtil.info(grid_info, "无法获取到当前设备状态");
                                }
                            }
                            else if (header == "重命名")
                            {
                                var device = kaiosHelper.getAdbDevice();
                                if (device != null)
                                {
                                    InputDialog inputDialog = new InputDialog(tag.name);
                                    inputDialog.Owner = this;
                                    inputDialog.ShowDialog();

                                    if (inputDialog.DialogResult == true)
                                    {
                                        var value = inputDialog.value;

                                        var oldname = "";
                                        var newname = "";
                                        if (tag.parent.EndsWith("/"))
                                        {
                                            oldname = tag.parent + tag.name;
                                            newname = tag.parent + value;
                                        }
                                        else
                                        {
                                            oldname = tag.parent + "/" + tag.name;
                                            newname = tag.parent + "/" + value;
                                        }
                                        var client = kaiosHelper.getAdbClient();
                                        client.ExecuteShellCommand(device, "mv \"" + oldname + "\" \"" + newname + "\"", null);
                                        DialogUtil.success(grid_info, "重命名完毕！");
                                    }
                                    refreshFileList();
                                }
                                else
                                {
                                    DialogUtil.info(grid_info, "无法获取到当前设备状态");
                                }
                            }
                            else if (header == "修改权限")
                            {
                                var device = kaiosHelper.getAdbDevice();
                                if (device != null)
                                {
                                    var oldqx = "";
                                    oldqx = tag.xr;
                                    oldqx = getNumberXR(oldqx);

                                    InputDialog inputDialog = new InputDialog(oldqx);
                                    inputDialog.Owner = this;
                                    inputDialog.ShowDialog();

                                    if (inputDialog.DialogResult == true)
                                    {
                                        var oldname = "";
                                        if (tag.parent.EndsWith("/"))
                                        {
                                            oldname = tag.parent + tag.name;
                                        }
                                        else
                                        {
                                            oldname = tag.parent + "/" + tag.name;
                                        }
                                        var client = kaiosHelper.getAdbClient();
                                        client.ExecuteShellCommand(device, "chmod " + inputDialog.value + " \"" + oldname + "\"", null);
                                        DialogUtil.success(grid_info, "权限修改完毕！");
                                    }
                                    refreshFileList();
                                }
                                else
                                {
                                    DialogUtil.info(grid_info, "无法获取到当前设备状态");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        static Dictionary<string, int> QX = new Dictionary<string, int>()
        {
            { "r",4},
            { "w",2},
            { "x",1},
            { "-",0},
        };
        private string getNumberXR(string xr)
        {
            try
            {
                char[] data = xr.ToCharArray();
                var qx = "";
                for (int i = 1; i < data.Length; i += 3)
                {
                    string q1 = data[i].ToString();
                    string q2 = data[i + 1].ToString();
                    string q3 = data[i + 2].ToString();
                    int nowqx = QX[q1] + QX[q2] + QX[q3];
                    qx = qx + nowqx.ToString();
                }
                return qx;
            }
            catch (Exception ex)
            {
                return "777";
            }
        }

        private async void btn_goNei_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                itemsfiles.ItemsSource = await getRootFiles("/data/usbmsc_mnt/");
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }

        }

        private async void btn_goSdcard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                itemsfiles.ItemsSource = await getRootFiles("/sdcard/");
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        private async void btn_delete_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var app = (sender as Button)?.Tag as KaiosAppItem;
                if (app != null)
                {
                    if (MessageBox.Show("是否删除文件:" + app.name + "？", "删除确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        helper.uninstallApp(app.manifestURL);
                        await Task.Delay(500);
                        await refreshApps();
                    }
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        private async void btn_start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var app = (sender as Button)?.Tag as KaiosAppItem;
                if (app != null)
                {
                    helper.launchApp(app.manifestURL);
                    await Task.Delay(100);
                    await refreshApps();
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        private async Task refreshApps()
        {
            await Task.Run(() =>
            {
                try
                {
                    var apps = helper.getAllApps();
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        itemapps.ItemsSource = apps;
                    });

                    var runningapps = helper.getAllRunningApps();
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        itemsrunningapps.ItemsSource = runningapps;
                    });
                }
                catch (Exception ex)
                {
                    DialogUtil.info(grid_info, "操作失败：" + ex.Message);
                }
            });
        }

        private async void btn_refreshapps_Click(object sender, RoutedEventArgs e)
        {
            setLoading(true);
            await Task.Run(async () =>
            {
                await refreshApps();
                Thread.Sleep(300);
                setLoading(false);
            });
        }

        private async void btn_closeapp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var app = (sender as Button)?.Tag as KaiosAppItem;
                if (app != null)
                {
                    helper.closeApp(app.manifestURL);
                    await Task.Delay(1000);
                    await refreshApps();
                }
            }
            catch (Exception ex)
            {
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        private void btn_installapp_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "所有受支持的文件(*.webapp;*.zip)|*.webapp;*.zip|kaios程序描述文件(*.webapp)|*.webapp|zip包(*.zip)|*.zip|全部文件(*.*)|*.*"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                var filename = openFileDialog.FileName;
                if (filename.EndsWith(".zip"))
                {
                    setLoading(true);
                    Task.Run(() =>
                    {
                        try
                        {

                            ZipFile zipFile = new ZipFile(filename);
                            var zipEntry = zipFile.FindEntry("manifest.webapp", true);
                            var zipEntry2 = zipFile.FindEntry("application.zip", true);
                            if (zipEntry != -1 || zipEntry2 != -1)
                            {
                                zipFile.Close();
                                helper.installApp(filename);
                            }
                            else
                            {
                                DialogUtil.info(grid_info, System.IO.Path.GetFileName(filename) + " 不是受支持的kaios安装包！");
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            DialogUtil.info(grid_info, System.IO.Path.GetFileName(filename) + " 安装失败！" + ex.Message);
                        }
                        finally
                        {
                            setLoading(false);
                        }
                    });

                }
                else if (filename.EndsWith(".webapp"))
                {
                    setLoading(true);
                    Task.Run(async () =>
                    {
                        try
                        {
                            SharpZip.PackFiles(Directory.GetCurrentDirectory() + "\\tempapp.zip", System.IO.Path.GetDirectoryName(filename));
                            var ret = helper.installApp(Directory.GetCurrentDirectory() + "\\tempapp.zip");
                            if (ret)
                            {
                                DialogUtil.success(grid_info, System.IO.Path.GetFileName(filename) + " 安装成功！");
                            }
                            else
                            {
                                DialogUtil.info(grid_info, System.IO.Path.GetFileName(filename) + " 安装失败！");
                            }
                            await refreshApps();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            DialogUtil.info(grid_info, System.IO.Path.GetFileName(filename) + " 安装失败！" + ex.Message);
                        }
                        finally
                        {
                            setLoading(false);
                        }
                    });

                }
                else
                {
                    DialogUtil.info(grid_info, System.IO.Path.GetFileName(filename) + " 不受支持！");
                }
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var tag = (sender as StackPanel)?.Tag as KaiosStoneItem;

                var index = cmb_appsource.SelectedIndex;

                if (tag != null)
                {
                    setLoading(true);
                    Task.Run(() =>
                    {

                        try
                        {
                            if (index == 0)
                            {
                                var url = "https://api.kaiostech.com/apps/manifest/" + tag.id;

                                var ret = KaiSton.Request("GET", url, "");

                                var data = JsonConvert.DeserializeObject<KaistonDetailItem>(ret);
                                App.Current?.Dispatcher?.Invoke(() =>
                                {
                                    gridshopinfo.DataContext = data;
                                    gridshopinfo.Visibility = Visibility.Visible;
                                });
                            }
                            else
                            {
                                var bhitem = tag.bHAppItem;
                                if (bhitem != null)
                                {
                                    var kaideatil = bhitem.convertToKaistonDetail();

                                    HttpClient httpClient = new HttpClient();
                                    var ret = httpClient.Head(kaideatil.package_path);
                                    var size = ret.ContentLength;
                                    kaideatil.packaged_size = size;
                                    App.Current?.Dispatcher?.Invoke(() =>
                                    {
                                        gridshopinfo.DataContext = kaideatil;
                                        gridshopinfo.Visibility = Visibility.Visible;
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Current?.Dispatcher?.Invoke(() =>
                            {
                                gridshopinfo.DataContext = tag;
                                gridshopinfo.Visibility = Visibility.Visible;
                            });
                            // DialogUtil.info(grid_info, "操作失败：" + ex.Message);
                        }
                        finally
                        {
                            setLoading(false);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                DialogUtil.info(grid_info, "操作失败：" + ex.Message);
            }
        }

        private void btnclosegruishopinfo_Click(object sender, RoutedEventArgs e)
        {
            gridshopinfo.Visibility = Visibility.Collapsed;
        }

        private void btn_download_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag as IStonItem;

            if (tag != null)
            {
                var downlink = tag.package_path;
                var downname = tag.name + "v" + tag.version + ".zip";
                setLoading(true);
                Task.Run(() =>
                {
                    var path = Directory.GetCurrentDirectory() + "\\" + "shopdown";
                    try
                    {
                        try
                        {
                            if (Directory.Exists(path) == false)
                            {
                                Directory.CreateDirectory(path);
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        var res = KaiSton.RequestDown("GET", downlink, "");
                        File.WriteAllBytes(path + "\\" + downname, res);
                        DialogUtil.success(grid_info, "下载成功！");
                    }
                    catch (Exception ex)
                    {
                        DialogUtil.info(grid_info, "下载失败:" + ex.Message);
                    }
                    finally
                    {
                        setLoading(false);
                    }

                });

            }
        }

        private void btn_install_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag as IStonItem;
            if (tag != null)
            {
                var downlink = tag.package_path;
                var downname = tag.name + "v" + tag.version + ".zip";
                setLoading(true);
                Task.Run(() =>
                {
                    var path = Directory.GetCurrentDirectory() + "\\" + "shopdown";
                    try
                    {
                        try
                        {
                            if (Directory.Exists(path) == false)
                            {
                                Directory.CreateDirectory(path);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        if (File.Exists(path + "\\" + downname) == false)
                        {
                            var res = KaiSton.RequestDown("GET", downlink, "");
                            File.WriteAllBytes(path + "\\" + downname, res);

                        }
                        var ret = helper.installApp(path + "\\" + downname);
                        if (ret)
                        {
                            DialogUtil.success(grid_info, downname + " 安装成功！");
                        }
                        else
                        {
                            DialogUtil.info(grid_info, downname + " 安装失败！");
                        }
                    }
                    catch (Exception ex)
                    {
                        DialogUtil.info(grid_info, downname + " 安装失败！" + ex.Message);
                    }
                    finally
                    {
                        setLoading(false);
                    }

                });

            }
        }

        private void btnOpenlocaldir_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Directory.GetCurrentDirectory() + "\\download\\";//文件（文件夹）路径
            System.Diagnostics.Process.Start(filePath);
        }

        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_search.Text))
            {
                itemskaistonlist.ItemsSource = SplitPages(allapps);
            }
            else
            {
                var appsnew = allapps.Where(p => p.name.Contains(txt_search.Text.Trim())).ToList();
                itemskaistonlist.ItemsSource = SplitPages(appsnew);
            }
        }

        private List<KaiosStoneItem> nowShowApps = new List<KaiosStoneItem>();
        private List<KaiosStoneItem> SplitPages(List<KaiosStoneItem> items)
        {
            nowShowApps = items;

            hc_page.PageIndex = 1;
            hc_page.MaxPageCount = Convert.ToInt32(Math.Ceiling(nowShowApps.Count / 20m));

            List<KaiosStoneItem> ret = new List<KaiosStoneItem>();

            ret = nowShowApps.Skip(20 * (hc_page.PageIndex - 1)).Take(20).ToList();

            return ret;
        }

        private void btn_refreshappslist_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("刷新列表会重新下载应用列表，这将会耗费数十秒的时间！是否刷新", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var index = cmb_appsource.SelectedIndex;
                setLoading(true);
                Task.Run(() =>
                {
                    try
                    {
                        if (index == 0)
                        {
                            KaiSton.getKey();
                            string apps = "";
                            var ret = KaiSton.Request("GET", "/v3.0/apps?software=KaiOS_2.5.4.1&locale=zh-CN", "");//&category=30&page_num=1&page_size=20 
                            apps = JObject.Parse(ret)["apps"].ToString(Formatting.None);
                            File.WriteAllText("apps.json", apps);

                            allapps = JsonConvert.DeserializeObject<List<KaiosStoneItem>>(apps);
                            App.Current?.Dispatcher?.Invoke(() =>
                            {
                                itemskaistonlist.ItemsSource = SplitPages(allapps);
                            });
                        }
                        else if (index == 1)
                        {
                            HttpClient httpClient = new HttpClient();
                            var ret = httpClient.Get("https://storedb.opengiraffes.top/data.json");
                            var apps = ret.RawText;
                            File.WriteAllText("opengiraffesapps.json", apps);
                            apps = JObject.Parse(apps)["apps"].ToString();
                            var t = JsonConvert.DeserializeObject<List<BHAppItem>>(apps);

                            allapps = new List<KaiosStoneItem>();

                            foreach (var item in t)
                            {
                                allapps.Add(item.convertToKaistonItem());
                            }

                            App.Current?.Dispatcher?.Invoke(() =>
                            {
                                itemskaistonlist.ItemsSource = SplitPages(allapps);
                            });
                        }
                        else if (index == 2)
                        {
                            HttpClient httpClient = new HttpClient();
                            var ret = httpClient.Get("https://banana-hackers.gitlab.io/store-db/data.json");
                            var apps = ret.RawText;
                            File.WriteAllText("bhapps.json", apps);
                            apps = JObject.Parse(apps)["apps"].ToString();
                            var t = JsonConvert.DeserializeObject<List<BHAppItem>>(apps);

                            allapps = new List<KaiosStoneItem>();

                            foreach (var item in t)
                            {
                                allapps.Add(item.convertToKaistonItem());
                            }

                            App.Current?.Dispatcher?.Invoke(() =>
                            {
                                itemskaistonlist.ItemsSource = SplitPages(allapps);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {

                        setLoading(false);
                    }
                });
            }
        }

        private void hc_content_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //向上滚动
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = System.Windows.UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                hc_content?.RaiseEvent(eventArg);
            }
            else
            {
                //向下滚动
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = System.Windows.UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                hc_content?.RaiseEvent(eventArg);
            }
        }

        private void Pagination_PageUpdated(object sender, HandyControl.Data.FunctionEventArgs<int> e)
        {
            var ret = nowShowApps.Skip(20 * (hc_page.PageIndex - 1)).Take(20).ToList();
            itemskaistonlist.ItemsSource = ret;
        }

        private void btn_search_net_Click(object sender, RoutedEventArgs e)
        {
            var searchtext = txt_search.Text.Trim();
            setLoading(true);
            Task.Run(() =>
            {
                try
                {
                    var ret = KaiSton.Request("GET", "https://search.kaiostech.com/v3/_search?imei=12345678912345&mcc=null&mnc=null&curef=LEO-23LIO87CN00&platform=2.5.4.1&bookmark=true&page=0&size=1000&query=" + searchtext + "&querySource=landing_page&locale=zh-CN", "");

                    var apps = JObject.Parse(ret)["organic"].ToString(Formatting.None);

                    nowShowApps = JsonConvert.DeserializeObject<List<KaiosStoneItem>>(apps);
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        itemskaistonlist.ItemsSource = SplitPages(nowShowApps);
                    });
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    setLoading(false);
                }
            });
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isload == false)
            {
                return;
            }
            if (cmb_appsource.SelectedIndex == 0)
            {
                btn_search_net.Visibility = Visibility.Visible;
                getGuanfangApps();
            }
            else if (cmb_appsource.SelectedIndex == 1)
            {
                btn_search_net.Visibility = Visibility.Collapsed;
                getOpengiraffesApps();
            }
            else if (cmb_appsource.SelectedIndex == 2)
            {
                btn_search_net.Visibility = Visibility.Collapsed;
                geBHApps();
            }
        }

        private void btn_getupdate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.dospy.wang/thread-20208-1-1.html"));
        }

        private void btn_opendownload_Click(object sender, RoutedEventArgs e)
        {

            string filePath = Directory.GetCurrentDirectory() + "\\shopdown\\";
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            }
            catch { }
            System.Diagnostics.Process.Start(filePath);
        }
    }
}
