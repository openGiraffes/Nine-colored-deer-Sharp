using KaiosMarketDownloader.Beans;
using KaiosMarketDownloader.utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KaiosMarketDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = OperateIniFile.ReadIniInt("setting", "threadCount", threadCount);
            checkBox1.Checked = OperateIniFile.ReadIniInt("setting", "ZengLiang", ZengLiang ? 1 : 0) == 1 ? true : false;

            checkBox2.Checked = OperateIniFile.ReadIniInt("setting", "V3", V3 ? 1 : 0) == 1 ? true : false;
        }
        private bool V3 = true;
        Thread thread = null;
        private void button1_Click(object sender, EventArgs e)
        {
            ZengLiang = checkBox1.Checked;
            threadCount = Convert.ToInt32(numericUpDown1.Value);
            V3 = checkBox2.Checked;
            OperateIniFile.WriteIniInt("setting", "threadCount", threadCount);
            OperateIniFile.WriteIniInt("setting", "ZengLiang", ZengLiang ? 1 : 0); 
            OperateIniFile.WriteIniInt("setting", "V3", V3 ? 1 : 0);

            if(V3)
            {
                KaiSton.settingsStr = KaiSton.V3Str;
                KaiSton.jsonSetting = JObject.Parse(KaiSton.settingsStr);
            }
            else {
                KaiSton.settingsStr = KaiSton.V2Str;
                KaiSton.jsonSetting = JObject.Parse(KaiSton.settingsStr);
            }

            if (button1.Text== "开始下崽")
            {
                button1.Enabled = false;
                thread = new Thread(DownloadThread);
                thread.IsBackground = true;
                thread.Start();
                button1.Text = "停止下崽";
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
                try
                {
                    if (thread != null)
                    {

                        thread.Abort();
                    }
                }catch(Exception ex)
                {

                }
                foreach(Thread t in threadlist)
                {
                    try
                    {
                        if (t != null)
                        {
                            t.Abort();
                        }
                    }
                    catch (Exception ex)
                    {

                    } 
                }
                threadlist.Clear();
                button1.Text = "开始下崽";
                button1.Enabled = true;
            }
        }

        private void Log(string msg)
        {
            msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss -> ") + msg + "\r\n";
            Console.WriteLine(msg);
            this.Invoke(new Action(() =>
            {
                txt_log.AppendText(msg);
                txt_log.SelectionStart = txt_log.Text.Length;
                txt_log.ScrollToCaret();

            }));
        }
        int threadCount = 5;
        int now = 0;
        int count = 0;
        private void UpdateLabel()
        {
            label1.Invoke(new Action(() =>
            {
                label1.Text = string.Format("当前{0}/共{1}", count - downlist.Count, count);
            }));
        }
        private object locker = new object();
        private void DownThread()
        {
            while (true)
            {
                try
                {
                    if (downlist.Count == 0)
                    {
                        try
                        {
                            lock(locker)
                            {  
                                threadlist.Remove(threadlist.First(p => p.Name == Thread.CurrentThread.Name));
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        if (threadlist.Count == 0)
                        {
                            Finished();
                        }
                        return;
                    }
                    KaiosStoneItem item = null;
                    lock (locker)
                    {
                       item = downlist.Dequeue();
                    } 
                    UpdateLabel();
                    int i = item.nowid;
                    string rename = item.rename;
                    string savename = item.savename;

                    for (int trycount = 1; trycount <= 5; trycount++)
                    {
                        try
                        {
                            Log("第"+Thread.CurrentThread.Name + "只母鸡，正在努力下第" + (i + 1) + "只崽,崽的名字叫：" + rename + "！");

                            var downlink = item.package_path;

                            var res = KaiSton.RequestDown("GET", downlink, "");

                            if (res.Length < 4096)
                            {
                                try
                                {
                                    string data = "";
                                    data = Encoding.UTF8.GetString(res);
                                    var jsonobj = JObject.Parse(data);
                                    if (jsonobj["code"].ToString()=="401")
                                    { 
                                        KaiSton.getKey();
                                        continue;
                                    } 
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            lock (locker){
                                File.WriteAllBytes(savename, res);
                            }

                            Log("第" + Thread.CurrentThread.Name + "只母鸡，" + "第" + (i + 1) + "只崽 " + rename + " 已经下完！");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log("第" + Thread.CurrentThread.Name + "只母鸡，"+"第" + (i + 1) + "只崽 " + rename + " 不肯出窝并说\"" + ex.Message + "\"，重试第" + trycount + "次！");
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        private void Finished()
        {
            Log("恭喜，全部下崽完成！！！");

            this.Invoke(new Action(() =>
            {
                button1.Text = "开始下崽";
            }));
        }
        Queue<KaiosStoneItem> downlist = new Queue<KaiosStoneItem>();
        List<Thread> threadlist = new List<Thread>();
        bool ZengLiang = false;
        private void DownloadThread()
        {
            try
            {
                Log("开始下崽..."); 
                KaiSton.getKey();
                Log("正在寻找母鸡...");
                var ret = "";

                if (V3)
                {
                    ret = KaiSton.Request("GET", "/v3.0/apps?software=KaiOS_3.1.0.0&locale=zh-CN", "");//&category=30&page_num=1&page_size=20 
                }
                else
                {

                    ret = KaiSton.Request("GET", "/v3.0/apps?software=KaiOS_2.5.4.1&locale=zh-CN", "");//&category=30&page_num=1&page_size=20 
                }
                var retjson = JObject.Parse(ret);
                var apps = retjson.ToString(Formatting.Indented);
                if(V3)
                { 
                    File.WriteAllText("appsdata_v3.json", apps);
                }
                else
                { 
                    File.WriteAllText("appsdata_v2.json", apps);
                }
                var allapps = JsonConvert.DeserializeObject<List<KaiosStoneItem>>(retjson["apps"].ToString());

                Log("母鸡已经找到，共有" + allapps.Count + "个崽，开始准备下崽...");
                string downloadpath = Directory.GetCurrentDirectory() + "\\eggs\\";
                if(V3)
                {
                    downloadpath = Directory.GetCurrentDirectory() + "\\eggs_v3\\";
                }
                else
                {
                    downloadpath = Directory.GetCurrentDirectory() + "\\eggs_v2\\";
                }

                try
                {

                    if (!Directory.Exists(downloadpath))
                    {
                        Directory.CreateDirectory(downloadpath);
                    }
                }
                catch(Exception ex)
                {

                }
                downlist.Clear();
                count = allapps.Count; 
                UpdateLabel();
                for (int i = 0;i<allapps.Count;i++)
                {
                    now = i + 1;

                    KaiosStoneItem item = allapps[i];
                    item.nowid = i;
                    string rename = item.name.Replace(" ", " ") + " " + item.version + ".zip";
                    rename = rename.Replace("\\", " ");

                    rename = rename.Replace("/", " ");

                    rename = rename.Replace(":", " ");

                    rename = rename.Replace("*", " ");

                    rename = rename.Replace("\"", " ");

                    rename = rename.Replace("<", " ");

                    rename = rename.Replace(">", " ");

                    rename = rename.Replace("|", " ");

                    rename = rename.Replace("?", " ");

                    var savename = downloadpath + "\\" + rename;
                    item.rename = rename;
                    item.savename = savename;
                    if (ZengLiang)
                    {
                        if (File.Exists(savename))
                        {
                            try
                            {
                                if (new FileInfo(savename).Length<4096)
                            { 
                                    string filecontent = File.ReadAllText(savename);
                                    string data = ""; 
                                    var jsonobj = JObject.Parse(filecontent);
                                    if (jsonobj["code"].ToString() == "401")
                                    {
                                        Log("当前是增量下崽，第" + (i + 1) + "只崽 " + rename + " 是坏崽，删除！");
                                        File.Delete(savename);
                                    }
                                    else
                                    {
                                        Log("当前是增量下崽，第" + (i + 1) + "只崽 " + rename + " 已经在窝里了！");
                                        continue;
                                    }
                                }
                                else
                                {
                                    Log("当前是增量下崽，第" + (i + 1) + "只崽 " + rename + " 已经在窝里了！");
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log("当前是增量下崽，第" + (i + 1) + "只崽 " + rename + " 已经在窝里了！");
                                continue;
                            } 
                        }
                    }
                    if(string.IsNullOrWhiteSpace(item.package_path))
                    {
                        Log("第" + (i + 1) + "只崽 " + rename + " 可能是云崽，不用下崽了！");
                        continue;
                    }
                    downlist.Enqueue(item);
                }

                for(int i=0;i<threadCount;i++)
                {
                    var threadnow = new Thread(DownThread);
                    threadnow.IsBackground = true;
                    threadnow.Start();
                    threadnow.Name = (i + 1).ToString();
                    threadlist.Add(threadnow);
                } 
            }
            catch(Exception ex)
            {
                Log("悲，崽崽难产了！！！" + ex.Message);
                this.Invoke(new Action(() =>
                {
                    button1.Text = "开始下崽";
                }));
            }
        }
    }
}
