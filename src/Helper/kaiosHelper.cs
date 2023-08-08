using Newtonsoft.Json.Linq;
using Nine_colored_deer_Sharp.utils;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Nine_colored_deer_Sharp.Beans;
using System.Security.Policy;
using Newtonsoft.Json;
using System.Data;

namespace Nine_colored_deer_Sharp.Helper
{
    public class kaiosHelper
    {
        private object locker = new object();

        Socket tcpClient;

        public byte[] withLen(string data)
        {
            data = Encoding.UTF8.GetBytes(data).Length + ":" + data;
            return Encoding.UTF8.GetBytes(data);
        }
        string host;
        int port;
        public kaiosHelper(string host = "127.0.0.1", int port = 6000)
        {
            this.host = host;
            this.port = port;

            init();
        }
        private static DeviceData _nowdevice;
        public static DeviceData nowdevice
        {
            get
            {
                return _nowdevice;
            }
            set
            {
                _nowdevice = value;
                if (_nowdevice != null)
                {
                    var client = kaiosHelper.getAdbClient();
                    var device = _nowdevice;
                    if (device != null)
                    {
                        var Properties = client.GetProperties(device);
                        var model = Properties["ro.product.model"];
                        _nowdevice.Model = model;

                        var abi = Properties["ro.product.cpu.abi"];
                        var pl = Properties["ro.board.platform"];
                        var baseos = Properties["ro.build.version.base_os"];
                        var serial = Properties["ro.boot.serialno"];

                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            MainWindow.self.txt_Model.Content = model;

                            MainWindow.self.txt_cpu.Text = "CPU：" + pl;
                            MainWindow.self.txt_cpuabi.Text = "CPU架构：" + abi;
                            MainWindow.self.txt_baseos.Text = "系统版本：" + baseos;
                            MainWindow.self.txt_serial.Text = "序列号：" + serial;
                            //MainWindow.self.txt_kenral.Text = "内核版本：" + kenral;


                        });
                        Task.Run(() =>
                        {
                            getMemory();
                            getUpTime();
                            getKenralVersion();
                        });

                    }
                    else
                    {
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            MainWindow.self.txt_Model.Content = "无设备-点击刷新";
                        });
                    }
                }
            }
        }

        private static void getMemory()
        {
            var client = kaiosHelper.getAdbClient();
            var device = nowdevice;
            if (device != null)
            {
                client.ExecuteShellCommand(device, "df /sdcard", new OutPutReveiver());
            }
        }
        private static void getKenralVersion()
        {
            var client = kaiosHelper.getAdbClient();
            var device = nowdevice;
            if (device != null)
            {
                client.ExecuteShellCommand(device, "cat /proc/version", new OutPutReveiver());
            }
        }


        private static void getUpTime()
        {
            var client = kaiosHelper.getAdbClient();
            var device = nowdevice;
            if (device != null)
            {
                client.ExecuteShellCommand(device, "uptime", new OutPutReveiver());
            }
        }


        private static AdbClient client { get; set; }
        public static AdbClient getAdbClient()
        {
            if (client == null)
            {
                client = new AdbClient();

            }
            return client;
        }

        public static DeviceData getAdbDevice()
        {
            try
            {
                if (!AdbServer.Instance.GetStatus().IsRunning)
                {
                    AdbServer server = new AdbServer();
                    StartServerResult result = server.StartServer(Directory.GetCurrentDirectory() + "//adb//adb.exe", false);
                    if (result != StartServerResult.Started)
                    {

                    }
                }

                var adbclient = getAdbClient();
                var devices = adbclient.GetDevices().ToList();

                if (nowdevice != null)
                {
                    nowdevice = devices.Where(p => p.Serial == nowdevice.Serial).FirstOrDefault();
                    if (nowdevice?.State == DeviceState.Online)
                    {
                        return nowdevice;
                    }
                }
                devices.ForEach(p =>
                {
                    try
                    {
                        var props = adbclient.GetProperties(p);
                        p.Model = props["ro.product.manufacturer"] + " " + props["ro.product.model"] + " - " + p.State.ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                });

                if (devices == null || devices.Count == 0)
                {
                    nowdevice = null;
                    return null;
                }
                if (devices.Count == 1)
                {
                    nowdevice = devices[0];
                    return devices[0];
                }
                DeviceData device = null;
                PhoneSelecterWindow selectphopnewin = null;
                App.Current?.Dispatcher?.Invoke(() =>
                {
                    selectphopnewin = new PhoneSelecterWindow(devices);
                    selectphopnewin.Owner = MainWindow.self;
                    selectphopnewin.ShowDialog();
                });
                device = selectphopnewin.selectedDevice;
                nowdevice = device;
                return device;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void init()
        {
            lock (locker)
            {
                try
                {
                    if (!AdbServer.Instance.GetStatus().IsRunning)
                    {
                        AdbServer server = new AdbServer();
                        StartServerResult result = server.StartServer(Directory.GetCurrentDirectory() + "//adb//adb.exe", false);
                        if (result != StartServerResult.Started)
                        {

                        }
                    }
                    var adbclient = new AdbClient();
                    var device = kaiosHelper.getAdbDevice();
                    if (device != null)
                    {
                        try
                        {
                            adbclient.CreateForward(device, "tcp:6000", "localfilesystem:/data/local/debugger-socket", false);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    tcpClient = new Socket(SocketType.Stream, ProtocolType.IP);
                    tcpClient.Connect(host, port);

                    List<byte> bytes = new List<byte>();

                    byte[] buffer = new byte[1] { 0 };

                    while (buffer[0] != 58)
                    {
                        var tmp = tcpClient.Receive(buffer, 1, SocketFlags.None);
                        if (tmp <= 0)
                        {
                            break;
                        }
                        if (buffer[0] != 58)
                        {
                            bytes.Add(buffer[0]);
                        }
                    }

                    var size = int.Parse(Encoding.UTF8.GetString(bytes.ToArray()));

                    bytes = new List<byte>();

                    buffer = new byte[1024];

                    while (bytes.Count < size)
                    {
                        var readcount = tcpClient.Receive(buffer, buffer.Length, SocketFlags.None);

                        bytes.AddRange(buffer.Take(readcount).ToArray());
                    }

                    tcpClient.Send(withLen(listTabCmd));

                    Thread.Sleep(100);

                    bytes = new List<byte>();

                    buffer = new byte[1];

                    while (buffer[0] != 58)
                    {
                        var ret = tcpClient.Receive(buffer, 1, SocketFlags.None);
                        if (ret <= 0)
                        {
                            break;
                        }
                        if (buffer[0] != 58)
                        {
                            bytes.Add(buffer[0]);
                        }
                    }

                    size = int.Parse(Encoding.UTF8.GetString(bytes.ToArray()));

                    buffer = new byte[size];

                    tcpClient.Receive(buffer, size, SocketFlags.None);

                    var jsondata = JObject.Parse(Encoding.UTF8.GetString(buffer));

                    deviceActor = jsondata["deviceActor"]?.ToString();

                    webappsActor = jsondata["webappsActor"]?.ToString();

                }
                catch (Exception ex)
                {

                }
            }
        }
        string deviceActor;
        string webappsActor;
        string screenshotCmd = "{{\"type\":\"screenshotToDataURL\",\"to\":\"{0}\"}}";
        string listTabCmd = "{\"to\":\"root\",\"type\":\"listTabs\"}";
        string substring_cmd = "{{\"type\":\"substring\",\"start\":{0},\"end\":{1},\"to\":\"{2}\"}}";

        List<KaiosAppItem> allapps = null;

        public List<KaiosAppItem> getAllApps()
        {
            try
            {
                string ret = request("getAll");
                if (string.IsNullOrWhiteSpace(ret))
                {
                    return null;
                }
                var apps = JObject.Parse(ret)["apps"].ToString();
                var retdata = JsonConvert.DeserializeObject<List<KaiosAppItem>>(apps);
                allapps = retdata;
                return retdata;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public List<KaiosAppItem> getAllRunningApps()
        {
            try
            {
                string ret = request("listRunningApps");
                if (string.IsNullOrWhiteSpace(ret))
                {
                    return null;
                }
                var apps = JObject.Parse(ret)["apps"].ToString();
                var retdatastring = JsonConvert.DeserializeObject<List<string>>(apps);
                var retdata = new List<KaiosAppItem>();
                if (allapps != null)
                {
                    foreach (var item in retdatastring)
                    {
                        foreach (var app in allapps)
                        {
                            if (app.manifestURL == item)
                            {
                                retdata.Add(app);
                                break;
                            }
                        }
                    }
                }
                return retdata;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }



        public bool closeApp(string manifestURL)
        {
            try
            {
                string ret = request("close", new Dictionary<string, string>() { { "manifestURL", manifestURL } });
                if (string.IsNullOrWhiteSpace(ret))
                {
                    return false;
                }
                var apps = JObject.Parse(ret);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
        public bool launchApp(string manifestURL)
        {
            try
            {
                string ret = request("launch", new Dictionary<string, string>() { { "manifestURL", manifestURL } });
                if (string.IsNullOrWhiteSpace(ret))
                {
                    return false;
                }
                var apps = JObject.Parse(ret);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public bool uninstallApp(string manifestURL)
        {
            try
            {
                string ret = request("uninstall", new Dictionary<string, string>() { { "manifestURL", manifestURL } });
                if (string.IsNullOrWhiteSpace(ret))
                {
                    return false;
                }
                var apps = JObject.Parse(ret);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private string request(string data, Dictionary<string, string> extdata = null, string actor = "")
        {
            lock (locker)
            {
                try
                {
                    JObject json = JObject.Parse("{}");

                    if (string.IsNullOrWhiteSpace(actor))
                    {
                        json["to"] = webappsActor;
                    }
                    else
                    {
                        json["to"] = actor;
                    }
                    json["type"] = data;
                    if (extdata != null)
                    {
                        foreach (var keyvalue in extdata)
                        {
                            json[keyvalue.Key] = keyvalue.Value;
                        }
                    }

                    var jsonstr = json.ToString(Newtonsoft.Json.Formatting.None);
                    var datasend = withLen(jsonstr);
                    tcpClient.Send(datasend);
                    var bytes = new List<byte>();

                    var buffer = new byte[1];
                    int cnt = 0;
                    while (buffer[0] != 58)
                    {
                        var ret = tcpClient.Receive(buffer, 1, SocketFlags.None);
                        if (ret <= 0)
                        {
                            return "";
                            //cnt++;
                            ////Thread.Sleep(200);
                            //if (cnt >= 10)
                            //{
                            //    break;
                            //}
                        }
                        else
                        {
                            if (buffer[0] != 58)
                            {
                                bytes.Add(buffer[0]);
                            }
                        }
                    }
                    if (bytes.Count == 0 || bytes.All(p => p == 0))
                    {
                        return null;
                    }
                    int size = int.Parse(Encoding.UTF8.GetString(bytes.ToArray()));

                    bytes = new List<byte>();

                    buffer = new byte[1024 * 1024];

                    while (bytes.Count < size)
                    {
                        var readcount = tcpClient.Receive(buffer, buffer.Length, SocketFlags.None);

                        bytes.AddRange(buffer.Take(readcount).ToArray());
                    }

                    var buf = Encoding.UTF8.GetString(bytes.ToArray());
                    return buf;
                }
                catch (Exception ex)
                {
                    //init();
                    Console.WriteLine(ex.Message);
                }
                return null;
            }
        }

        public byte[] getImage()
        {
            lock (locker)
            {
                try
                {
                    string cmd = string.Format(screenshotCmd, deviceActor);
                    tcpClient.Send(withLen(cmd));


                    var bytes = new List<byte>();

                    var buffer = new byte[1];

                    while (buffer[0] != 58)
                    {
                        var ret = tcpClient.Receive(buffer, 1, SocketFlags.None);
                        if (ret <= 0)
                        {
                            break;
                        }
                        if (buffer[0] != 58)
                        {
                            bytes.Add(buffer[0]);
                        }
                    }
                    int size = int.Parse(Encoding.UTF8.GetString(bytes.ToArray()));


                    buffer = new byte[size];

                    tcpClient.Receive(buffer, size, SocketFlags.None);

                    var jsondata = Encoding.UTF8.GetString(buffer);
                    var image = JObject.Parse(jsondata)["value"];

                    var image_len = int.Parse(image["length"].ToString());

                    var actor = image["actor"].ToString();

                    cmd = string.Format(substring_cmd, 0, image_len, actor);

                    tcpClient.Send(withLen(cmd));


                    bytes = new List<byte>();

                    buffer = new byte[1];

                    while (buffer[0] != 58)
                    {
                        var ret = tcpClient.Receive(buffer, 1, SocketFlags.None);
                        if (ret <= 0)
                        {
                            break;
                        }
                        if (buffer[0] != 58)
                        {
                            bytes.Add(buffer[0]);
                        }
                    }
                    size = int.Parse(Encoding.UTF8.GetString(bytes.ToArray()));


                    bytes = new List<byte>();

                    buffer = new byte[1024 * 1024];

                    while (bytes.Count < size)
                    {
                        var readcount = tcpClient.Receive(buffer, buffer.Length, SocketFlags.None);

                        bytes.AddRange(buffer.Take(readcount).ToArray());
                    }

                    var buf = Encoding.UTF8.GetString(bytes.ToArray());

                    var image1 = JObject.Parse(buf)["substring"].ToString().Split(',')[1];
                    return Convert.FromBase64String(image1);
                }
                catch (Exception ex)
                {
                    init();
                    return null;
                }
            }
        }

        public static void DownloadFile(string remotefile, string localpath, IProgress<int> process)
        {
            AdbClient client = getAdbClient();
            var device = getAdbDevice();
            using (SyncService service = new SyncService(new AdbSocket(client.EndPoint), device))
            {
                using (Stream stream = File.OpenWrite(localpath))
                {
                    service.Pull(remotefile, stream, process, CancellationToken.None);
                }
            }
        }

        public static void UploadFile(string remotefile, string localpath, IProgress<int> process)
        {
            AdbClient client = getAdbClient();
            var device = getAdbDevice();
            using (SyncService service = new SyncService(new AdbSocket(client.EndPoint), device))
            {
                using (Stream stream = File.OpenRead(localpath))
                {
                    service.Push(stream, remotefile, 777, DateTimeOffset.Now, process, CancellationToken.None);
                }
            }
        }

        public static bool checkAdbStatus()
        {
            try
            {
                if (!AdbServer.Instance.GetStatus().IsRunning)
                {
                    AdbServer server = new AdbServer();
                    StartServerResult result = server.StartServer(Directory.GetCurrentDirectory() + "//adb//adb.exe", false);
                    if (result != StartServerResult.Started)
                    {
                        Console.WriteLine("Can't start adb server");
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool installApp(string filename)
        {
            lock (locker)
            {
                try
                {
                    //获取uploadActor
                    string ret = request("uploadPackage");
                    if (string.IsNullOrWhiteSpace(ret))
                    {
                        return false;
                    }

                    var datajson = JObject.Parse(ret);
                    var uploadActor = datajson["actor"].ToString();

                    int chunksize = 20480;
                    //var zipdataraw = File.ReadAllBytes(filename);
                    var zipdata = File.ReadAllBytes(filename);
                    ////上传数据
                    //string ret2 = request("chunk", new Dictionary<string, object>() { { "chunk", zipdata } }, uploadActor);
                    //if (string.IsNullOrWhiteSpace(ret2))
                    //{
                    //    return false;
                    //}

                    for (int i = 0; i < zipdata.Length; i += chunksize)
                    {

                        var datasend = "";
                        var datanow = zipdata.Skip(i).Take(chunksize).ToArray();
                        datasend = genMsg(datanow);
                        //上传数据
                        string ret2 = request("chunk", new Dictionary<string, string>() { { "chunk", datasend } }, uploadActor);
                        if (string.IsNullOrWhiteSpace(ret2))
                        {
                            return false;
                        }
                    }

                    //标记上传成功
                    string ret3 = request("done", null, uploadActor);
                    if (string.IsNullOrWhiteSpace(ret3))
                    {
                        return false;
                    }

                    //开始安装文件
                    string ret4 = request("install", new Dictionary<string, string>() { { "upload", uploadActor } ,{ "appId", Guid.NewGuid().ToString() }
            }, webappsActor);
                    if (string.IsNullOrWhiteSpace(ret4))
                    {
                        return false;
                    }

                    var appid = JObject.Parse(ret4)["appId"].ToString();

                    //删除文件
                    string ret5 = request("remove", null, uploadActor);
                    if (string.IsNullOrWhiteSpace(ret5))
                    {
                        return false;
                    }
                    var main = "app://" + appid + "/manifest.webapp";
                    this.launchApp(main);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return false;
            }
        }

        private string genMsg(byte[] datanow)
        {
            var ret = new StringBuilder();

            foreach (byte data in datanow)
            {
                switch (data)
                {
                    case 8:
                        ret.Append('\b');
                        break;
                    case 9:
                        ret.Append('\t');
                        break;
                    case 10:
                        ret.Append('\n');
                        break;
                    case 12:
                        ret.Append('\f');
                        break;
                    case 13:
                        ret.Append('\r');
                        break;
                    case 34:
                        ret.Append('\"');
                        break;
                    case 92:
                        ret.Append('\\');
                        break;
                    default:
                        if (data >= 32 && data <= 126)
                        {
                            ret.Append((char)data);
                        }
                        else
                        {
                            ret.Append((char)data);
                            //ret.Append((char)(0));

                            //ret.Append((char)(data >> 4));

                            //ret.Append((char)(data & 0x0f));
                        }
                        break;
                }
            }

            return ret.ToString();
        }
        //hextable  = "0123456789abcdef"
    }
}
