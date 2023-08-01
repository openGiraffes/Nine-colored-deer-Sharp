using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Nine_colored_deer_Sharp.Helper
{
    public class kaiosHelper
    {
        Socket tcpClient;

        public byte[] withLen(string data)
        {
            data = data.Length + ":" + data;
            return Encoding.UTF8.GetBytes(data);
        }

        public kaiosHelper(string host = "127.0.0.1", int port = 6000)
        {
            tcpClient = new Socket(SocketType.Stream, ProtocolType.IP);
            tcpClient.Connect(host, port);


            List<byte> bytes = new List<byte>();

            byte[] buffer = new byte[1] { 0 };

            while (buffer[0] != 58)
            {
                tcpClient.Receive(buffer, 1, SocketFlags.None);
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

                if (buffer[0] != 58)
                {
                    bytes.Add(buffer[0]);
                }
            }

            size = int.Parse(Encoding.UTF8.GetString(bytes.ToArray()));

            buffer = new byte[size];

            tcpClient.Receive(buffer, size, SocketFlags.None);

            var jsondata = Encoding.UTF8.GetString(buffer);

            deviceActor = JObject.Parse(jsondata)["deviceActor"].ToString();

        }
        string deviceActor;
        string screenshotCmd = "{{\"type\":\"screenshotToDataURL\",\"to\":\"{0}\"}}";
        string listTabCmd = "{\"to\":\"root\",\"type\":\"listTabs\"}";
        string substring_cmd = "{{\"type\":\"substring\",\"start\":{0},\"end\":{1},\"to\":\"{2}\"}}";


        public byte[] getImage()
        {
            string cmd = string.Format(screenshotCmd, deviceActor);
            tcpClient.Send(withLen(cmd));


            var bytes = new List<byte>();

            var buffer = new byte[1];

            while (buffer[0] != 58)
            {
                tcpClient.Receive(buffer, 1, SocketFlags.None);
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
                tcpClient.Receive(buffer, 1, SocketFlags.None);
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
    }
}
