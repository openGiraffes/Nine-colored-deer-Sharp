using Nine_colored_deer_Sharp.Beans;
using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nine_colored_deer_Sharp.Helper
{
    internal class FileReceiver : IShellOutputReceiver
    {
        public bool isCompleted { get; set; } = false;
        public bool ParsesErrors => throw new NotImplementedException();
        public List<FileItem> FileList { get; set; }

        private List<FileItem> RealFileList { get; set; }
        private List<FileItem> PathList { get; set; }
        private List<FileItem> LinkList { get; set; }
        public string LineNameParser(string line)
        {
            var ret = line.Substring(line.IndexOf(":") + 4);
            return ret;
        }

        public void AddOutput(string line)
        {
            try
            {

                if (RealFileList == null)
                {
                    RealFileList = new List<FileItem>();
                }
                if (PathList == null)
                {
                    PathList = new List<FileItem>();
                }
                if (LinkList == null)
                {
                    LinkList = new List<FileItem>();
                }
                if (line.Contains("No such file or directory"))
                {
                    FileItem fileItem2 = new FileItem();
                    fileItem2.name = "找不到文件或目录！";
                    fileItem2.isLink = false;
                    fileItem2.isDirectory = false;
                    fileItem2.parent = null;
                    fileItem2.detail = line;
                    fileItem2.size = "";
                    RealFileList.Add(fileItem2);
                    return;
                }

                //dr-xr-xr-x root     root              1970-02-23 00:42 acct
                FileItem fileItem = new FileItem();
                string[] info = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string start = line[0].ToString();
                if (start == "d")
                {
                    // is dir
                    fileItem.name = LineNameParser(line);
                    fileItem.isLink = false;
                    fileItem.isDirectory = true;
                    fileItem.parent = null;
                    fileItem.detail = line;

                    fileItem.size = null;
                    PathList.Add(fileItem);
                }
                else if (start == "l")
                {
                    // is link
                    fileItem.name = LineNameParser(line);
                    fileItem.isLink = true;
                    fileItem.isDirectory = false;
                    fileItem.parent = null;
                    fileItem.size = null;
                    fileItem.detail = line;
                    LinkList.Add(fileItem);
                }
                else if (start == "-")
                {
                    //is file
                    fileItem.name = LineNameParser(line);
                    fileItem.isLink = false;
                    fileItem.isDirectory = false;
                    fileItem.parent = null;
                    fileItem.detail = line;
                    fileItem.size = info[3];
                    RealFileList.Add(fileItem);
                }
                else
                {
                    Console.WriteLine("unknow data : " + line);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Flush()
        {
            FileList = new List<FileItem>();
            if (PathList != null)
            {
                FileList.AddRange(PathList);
            }
            if (LinkList != null)
            {

                FileList.AddRange(LinkList);
            }

            if (RealFileList != null)
            {
                FileList.AddRange(RealFileList);
            }

            isCompleted = true;
        }
    }
}
