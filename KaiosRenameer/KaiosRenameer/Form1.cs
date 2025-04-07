using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace KaiosRenameer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string path = string.Empty;
        private void button1_Click(object sender, EventArgs e)
        {
            path = SelectPath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                path = Path.GetDirectoryName(path);
                Log("选择了文件夹：" + path);
            }
        }
        public void Log(string log)
        {
            try
            {
                log = DateTime.Now.ToString("HH:mm:ss") + ">>" + log + "\r\n";
                Invoke(new MethodInvoker(delegate ()
                {
                    txt_log.AppendText(log);
                    txt_log.ScrollToCaret();
                }));
            }
            catch { }
        }

        private string SelectPath()
        {
            string path = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Files (*.zip)|*.zip"
            };

            //var result = openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }

            return path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(path) == false)
            {
                Log("请选择正确的路径！");
                return;
            }
            string[] files = Directory.GetFiles(path);

            foreach (string f in files)
            {
                try
                {
                    Log("正在处理:" + f);

                    ZipFile zipFile = new ZipFile(f);



                    var zipEntry = zipFile.FindEntry("manifest.webapp", true);
                    if (zipEntry != -1)
                    {
                        MemoryStream ms = new MemoryStream();
                        Stream inputStream2 = zipFile.GetInputStream(zipEntry);

                        byte[] bArr2 = new byte[1600];
                        while (true)
                        {
                            int read2 = inputStream2.Read(bArr2, 0, 1600);
                            if (read2 <= 0)
                            {
                                break;
                            }
                            ms.Write(bArr2, 0, read2);
                        }
                        var arr = ms.ToArray();
                        string data = Encoding.UTF8.GetString(arr);

                        zipFile.Close();

                        JObject jobj = JObject.Parse(data);

                        string name = jobj["name"].ToString();
                        string version = jobj["version"].ToString();
                        /// \ : * " < > | ？
                        string rename = name.Replace(" ", " ") + " " + version + ".zip";
                        rename = rename.Replace("\\", " ");

                        rename = rename.Replace("/", " ");

                        rename = rename.Replace(":", " ");

                        rename = rename.Replace("*", " ");

                        rename = rename.Replace("\"", " ");

                        rename = rename.Replace("<", " ");

                        rename = rename.Replace(">", " ");

                        rename = rename.Replace("|", " ");

                        rename = rename.Replace("?", " ");

                        rename = path + "\\" + rename;

                        File.Move(f, rename);
                        Log("重命名为:" + rename);
                    }
                    else
                    {
                        zipEntry = zipFile.FindEntry("manifest.webmanifest", true);
                        if (zipEntry != -1)
                        {
                            MemoryStream ms = new MemoryStream();
                            Stream inputStream2 = zipFile.GetInputStream(zipEntry);

                            byte[] bArr2 = new byte[1600];
                            while (true)
                            {
                                int read2 = inputStream2.Read(bArr2, 0, 1600);
                                if (read2 <= 0)
                                {
                                    break;
                                }
                                ms.Write(bArr2, 0, read2);
                            }
                            var arr = ms.ToArray();
                            string data = Encoding.UTF8.GetString(arr);

                            zipFile.Close();

                            JObject jobj = JObject.Parse(data);

                            string name = jobj["name"].ToString();
                            string version = jobj["b2g_features"]["version"].ToString();
                            /// \ : * " < > | ？
                            string rename = name.Replace(" ", " ") + " " + version + ".zip";
                            rename = rename.Replace("\\", " ");

                            rename = rename.Replace("/", " ");

                            rename = rename.Replace(":", " ");

                            rename = rename.Replace("*", " ");

                            rename = rename.Replace("\"", " ");

                            rename = rename.Replace("<", " ");

                            rename = rename.Replace(">", " ");

                            rename = rename.Replace("|", " ");

                            rename = rename.Replace("?", " ");

                            rename = path + "\\" + rename;

                            File.Move(f, rename);
                            Log("重命名为:" + rename);
                        }
                        else
                        {
                            Log("不兼容的zip包：" + f);
                        }
                    }
                }

                catch (Exception ex)
                {
                    Log("处理:" + f + " 出错 " + ex.Message);
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(path) == false)
            {
                Log("请选择正确的路径！");
                return;
            }
            string[] files = Directory.GetFiles(path);

            foreach (string f1 in files)
            {
                try
                {
                    Log("正在处理:" + f1);

                    File.Copy(f1, f1 + "_kaios2");
                    var f = f1 + "_kaios2";

                    ZipFile zipFile = new ZipFile(f);

                    var zipEntry = zipFile.FindEntry("manifest.webmanifest", true);
                    if (zipEntry != -1)
                    {
                        MemoryStream ms = new MemoryStream();
                        Stream inputStream2 = zipFile.GetInputStream(zipEntry);

                        byte[] bArr2 = new byte[1600];
                        while (true)
                        {
                            int read2 = inputStream2.Read(bArr2, 0, 1600);
                            if (read2 <= 0)
                            {
                                break;
                            }
                            ms.Write(bArr2, 0, read2);
                        }
                        var arr = ms.ToArray();
                        string data = Encoding.UTF8.GetString(arr);

                        JObject jobj2 = JObject.Parse(data);
                        foreach (JProperty item in jobj2["b2g_features"])
                        {
                            jobj2[item.Name] = item.Value;
                        }
                        jobj2.Remove("b2g_features");
                        jobj2["launch_path"] = jobj2["start_url"];
                        jobj2.Remove("start_url");
                        var icons = jobj2["icons"];

                        var newicons = JObject.Parse("{}");

                        foreach (JObject item in icons)
                        {
                            var size = item["sizes"].ToString().Split('x')[0];
                            var src = item["src"].ToString();
                            newicons[size] = src;
                        }

                        jobj2["icons"] = newicons;



                        string data2 = jobj2.ToString();
                        File.WriteAllText("manifest_temp.webapp", data2);

                        zipFile.BeginUpdate();
                        zipFile.Delete("manifest.webmanifest");

                        //替换json 

                        zipFile.Add("manifest_temp.webapp", "manifest.webapp");

                        zipFile.CommitUpdate();
                        //
                        zipFile.Close();

                        JObject jobj = JObject.Parse(data);

                        string name = jobj["name"].ToString();
                        string version = jobj["b2g_features"]["version"].ToString();
                        /// \ : * " < > | ？
                        string rename = name.Replace(" ", " ") + " " + version + "_kaios2" + ".zip";
                        rename = rename.Replace("\\", " ");

                        rename = rename.Replace("/", " ");

                        rename = rename.Replace(":", " ");

                        rename = rename.Replace("*", " ");

                        rename = rename.Replace("\"", " ");

                        rename = rename.Replace("<", " ");

                        rename = rename.Replace(">", " ");

                        rename = rename.Replace("|", " ");

                        rename = rename.Replace("?", " ");

                        rename = path + "\\" + rename;

                        File.Move(f, rename);
                        Log("重命名为:" + rename);
                    }
                    else
                    {
                        Log("不兼容的zip包（不是kaios3软件）：" + f);
                    }
                }

                catch (Exception ex)
                {
                    Log("处理:" + f1 + " 出错 " + ex.Message);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(path) == false)
            {
                Log("请选择正确的路径！");
                return;
            }
            string[] files = Directory.GetFiles(path);

            foreach (string f1 in files)
            {
                try
                {
                    Log("正在处理:" + f1);

                    File.Copy(f1, f1 + "_kaios3");
                    var f = f1 + "_kaios3";

                    ZipFile zipFile = new ZipFile(f);

                    var zipEntry = zipFile.FindEntry("manifest.webapp", true);
                    if (zipEntry != -1)
                    {
                        MemoryStream ms = new MemoryStream();
                        Stream inputStream2 = zipFile.GetInputStream(zipEntry);

                        byte[] bArr2 = new byte[1600];
                        while (true)
                        {
                            int read2 = inputStream2.Read(bArr2, 0, 1600);
                            if (read2 <= 0)
                            {
                                break;
                            }
                            ms.Write(bArr2, 0, read2);
                        }
                        var arr = ms.ToArray();
                        string data = Encoding.UTF8.GetString(arr);

                        JObject jobj2 = JObject.Parse(data);

                        // Create b2g_features object and move relevant properties into it
                        var b2gFeatures = new JObject();

                        if (jobj2.ContainsKey("version"))
                        {
                            b2gFeatures["version"] = jobj2["version"];
                            jobj2.Remove("version");
                        }

                        if (jobj2.ContainsKey("developer"))
                        {
                            b2gFeatures["developer"] = jobj2["developer"];
                            jobj2.Remove("developer");
                        }

                        if (jobj2.ContainsKey("permissions"))
                        {
                            b2gFeatures["permissions"] = jobj2["permissions"];
                            jobj2.Remove("permissions");
                        }

                        // Add b2g_features to main object
                        jobj2["b2g_features"] = b2gFeatures;

                        // Convert launch_path to start_url
                        if (jobj2.ContainsKey("launch_path"))
                        {
                            jobj2["start_url"] = jobj2["launch_path"];
                            jobj2.Remove("launch_path");
                        }

                        // Convert icons format
                        if (jobj2.ContainsKey("icons"))
                        {
                            var oldIcons = jobj2["icons"] as JObject;
                            var newIcons = new JArray();

                            foreach (var icon in oldIcons)
                            {
                                var iconObj = new JObject();
                                iconObj["src"] = icon.Value.ToString();
                                iconObj["sizes"] = icon.Key + "x" + icon.Key;
                                newIcons.Add(iconObj);
                            }

                            jobj2["icons"] = newIcons;
                        }

                        string data2 = jobj2.ToString();
                        File.WriteAllText("manifest_temp.webmanifest", data2);

                        zipFile.BeginUpdate();
                        zipFile.Delete("manifest.webapp");

                        // Replace manifest
                        zipFile.Add("manifest_temp.webmanifest", "manifest.webmanifest");
                        zipFile.CommitUpdate();
                        zipFile.Close();

                        JObject jobj = JObject.Parse(data);

                        string name = jobj["name"].ToString();
                        string version = jobj2["b2g_features"]["version"]?.ToString() ?? "1.0.0";

                        /// \ : * " < > | ？
                        string rename = name.Replace(" ", " ") + " " + version + "_kaios3" + ".zip";
                        rename = rename.Replace("\\", " ");
                        rename = rename.Replace("/", " ");
                        rename = rename.Replace(":", " ");
                        rename = rename.Replace("*", " ");
                        rename = rename.Replace("\"", " ");
                        rename = rename.Replace("<", " ");
                        rename = rename.Replace(">", " ");
                        rename = rename.Replace("|", " ");
                        rename = rename.Replace("?", " ");

                        rename = path + "\\" + rename;

                        File.Move(f, rename);
                        Log("重命名为:" + rename);
                    }
                    else
                    {
                        Log("不兼容的zip包（不是kaios2软件）：" + f);
                    }
                }
                catch (Exception ex)
                {
                    Log("处理:" + f1 + " 出错 " + ex.Message);
                }
            }
        }
    }
}
