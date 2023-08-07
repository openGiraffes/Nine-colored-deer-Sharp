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
        public void AddOutput(string line)
        {
            if (FileList == null)
            {
                FileList = new List<FileItem>();
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
                FileList.Add(fileItem2);
                return;
            }

            //dr-xr-xr-x root     root              1970-02-23 00:42 acct
            FileItem fileItem = new FileItem();
            string[] info = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string start = line[0].ToString();
            if (start == "d")
            {
                // is dir
                fileItem.name = string.Join(" ", info.Skip(5).ToArray());
                fileItem.isLink = false;
                fileItem.isDirectory = true;
                fileItem.parent = null;
                fileItem.detail = line;

                fileItem.size = null;
            }
            else if (start == "l")
            {
                // is link
                fileItem.name = info[5];
                fileItem.isLink = true;
                fileItem.isDirectory = false;
                fileItem.parent = null;
                fileItem.size = null;
                fileItem.detail = line;
            }
            else if (start == "-")
            {
                //is file
                fileItem.name = string.Join(" ", info.Skip(6).ToArray());
                fileItem.isLink = false;
                fileItem.isDirectory = false;
                fileItem.parent = null;
                fileItem.detail = line;
                fileItem.size = info[3];
            }
            else
            {
                Console.WriteLine("unknow data : " + line);
            }

            FileList.Add(fileItem);
        }

        public void Flush()
        {
            isCompleted = true;
        }
    }
}
