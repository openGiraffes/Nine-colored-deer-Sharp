using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nine_colored_deer_Sharp.Converts
{
    public class MulitImageCacheConverter : IMultiValueConverter
    {
        public static object imagecachelock = new object();
        public static void clearCache()
        {
            lock (imagecachelock)
            {
                var enmuer = imageCacheList.GetEnumerator();
                while (enmuer.MoveNext())
                {
                    try
                    {
                        var now = enmuer.Current.Key;
                        string filepath = Path.Combine(dir, now);
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                try
                {
                    //完全清除缓存
                    DirectoryInfo di = new DirectoryInfo(dir);
                    di.Delete(true);
                    di.Create();
                }
                catch (Exception ex)
                {

                }
                imageCacheList.Clear();
            }
        }

        public static void clearOneCache(string url)
        {
            string imgName = UserMd5(url);
            lock (imagecachelock)
            {
                try
                {
                    string filepath = Path.Combine(dir, imgName);
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                    imageCacheList.Remove(imgName);
                }
                catch (Exception ex)
                {

                }
            }
        }

        static string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符

                pwd = pwd + s[i].ToString("x2");

            }
            return pwd;
        }

        public static bool IsUserVisible(UIElement element)
        {
            if (!element.IsVisible)
                return false;
            var container = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (container == null) throw new ArgumentNullException("container");

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.RenderSize.Width, element.RenderSize.Height));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.IntersectsWith(bounds);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values == null)
                    return null;
                if (values.Length == 2)
                {
                    Image selfimage = values[1] as Image;
                    //if (IsUserVisible(selfimage) == false)
                    //{
                    //    return null;
                    //} 

                    string url = values[0]?.ToString();
                    if (url == null)
                    {
                        return null;
                    }
                    string imgName = UserMd5(url);
                    var filename = Path.Combine(MulitImageCacheConverter.dir, imgName + ".jpg");
                    lock (MulitImageCacheConverter.imagecachelock)
                    {
                        ImageSource outimg = null;
                        if (imageCacheList.TryGetValue(filename, out outimg))
                        {
                            selfimage.Source = outimg;
                            return null;
                        }
                    }
                    Task.Run(() =>
                    {
                        readImg(url, imgName, selfimage);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), "_converter_");
                return null;
            }
            //return ConvertInner(value, targetType, parameter, culture);
            return null;
        }

        public static Dictionary<string, ImageSource> imageCacheList = new Dictionary<string, ImageSource>();



        public static string dir = "";

        private static int imgCount = 3000;
        private static int dirGloup = 50;
        public static void CacheInit(string dir, int imgCount, int dirGloup)
        {
            MulitImageCacheConverter.dir = dir;
            MulitImageCacheConverter.imgCount = imgCount;
            MulitImageCacheConverter.dirGloup = dirGloup;
            Directory.CreateDirectory(dir);
        }

        public static string GetCashPath(string url, int gid, bool del = false)
        {
            return url;
        }

        static Semaphore semaphore = new Semaphore(5, 10);
        public static void readImg(string uri, string filename, Image selfimage, bool defaultnull = true)
        {
            if (semaphore.WaitOne(5000, false))
            {
                try
                {
                    filename = Path.Combine(MulitImageCacheConverter.dir, filename + ".jpg");
                    if (File.Exists(filename))
                    {
                        var filecontent = File.ReadAllBytes(filename);
                        if (filecontent.Length > 10 && filecontent[0] != 0 && filecontent[1] != 0 && filecontent[2] != 0 && filecontent[3] != 0)
                        {
                            //文件无异常
                            using (System.IO.MemoryStream memorystream = new System.IO.MemoryStream(filecontent))
                            {
                                memorystream.Seek(0, SeekOrigin.Begin);
                                System.Windows.Media.Imaging.BitmapImage bs = new System.Windows.Media.Imaging.BitmapImage();
                                bs.BeginInit();
                                // (\w+).CreateOptions = BitmapCreateOptions.DelayCreation;
                                bs.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                bs.StreamSource = memorystream;
                                bs.EndInit();
                                bs.Freeze();
                                //memorystream.Close();
                                if (selfimage != null)
                                {
                                    Application.Current?.Dispatcher?.Invoke(() =>
                                    {
                                        selfimage.Source = bs;
                                    });
                                }

                                lock (MulitImageCacheConverter.imagecachelock)
                                {
                                    imageCacheList[filename] = bs;
                                }
                                return;
                            }
                        }
                        else
                        {
                            //文件异常， 删除源文件
                            try
                            {
                                lock (imagecachelock)
                                {
                                    File.Delete(filename);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString(), "_converter_");
                            }
                        }

                    }

                    //Application.Current?.Dispatcher?.Invoke(() =>
                    //{
                    //    //这里清空图片的缓存
                    //    selfimage.Source = null;
                    //});

                    try
                    {

                        using (Stream networkstream = GetStream(uri))
                        {
                            if (networkstream != null)
                            {
                                using (System.IO.MemoryStream memorystream = new System.IO.MemoryStream())
                                {
                                    networkstream.CopyTo(memorystream);
                                    memorystream.Seek(0, SeekOrigin.Begin);

                                    var bytes = memorystream.ToArray();
                                    try
                                    {
                                        lock (imagecachelock)
                                        {
                                            File.WriteAllBytes(filename, bytes);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString(), "_converter_");
                                    }

                                    memorystream.Seek(0, SeekOrigin.Begin);
                                    System.Windows.Media.Imaging.BitmapImage bs = new System.Windows.Media.Imaging.BitmapImage();
                                    bs.BeginInit();

                                    // (\w+).CreateOptions = BitmapCreateOptions.DelayCreation;
                                    bs.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                    bs.StreamSource = memorystream;
                                    bs.EndInit();
                                    bs.Freeze();
                                    if (selfimage != null)
                                    {
                                        Application.Current?.Dispatcher?.Invoke(() =>
                                        {
                                            selfimage.Source = bs;
                                        });
                                    }
                                    lock (MulitImageCacheConverter.imagecachelock)
                                    {
                                        imageCacheList[filename] = bs;
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                if (defaultnull)
                                {
                                    if (selfimage != null)
                                    {
                                        Application.Current?.Dispatcher?.Invoke(() =>
                                        {
                                            selfimage.Source = null;
                                        });
                                    }
                                    lock (MulitImageCacheConverter.imagecachelock)
                                    {
                                        imageCacheList[filename] = null;
                                    }
                                }
                                //return null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString(), "_converter_");
                        if (defaultnull)
                        {
                            if (selfimage != null)
                            {
                                Application.Current?.Dispatcher?.Invoke(() =>
                                {
                                    selfimage.Source = null;
                                });
                            }
                            lock (MulitImageCacheConverter.imagecachelock)
                            {
                                imageCacheList[filename] = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString(), "_converter_");
                    if (defaultnull)
                    {
                        if (selfimage != null)
                        {
                            Application.Current?.Dispatcher?.Invoke(() =>
                            {
                                selfimage.Source = null;
                            });
                        }
                        lock (MulitImageCacheConverter.imagecachelock)
                        {
                            imageCacheList[filename] = null;
                        }
                    }
                    return;
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Stream GetStream(string url)
        {
            try
            {
                EasyHttp.Http.HttpClient httpClient = new EasyHttp.Http.HttpClient();
                for (int i = 0; i < 3; i++)
                {
                    //url = url.Replace("/./", "/");
                    httpClient.Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
                    httpClient.Request.Timeout = 120000;
                    httpClient.Request.UserAgent = "Mozilla/5.0 (Mobile; GoFlip2; rv:48.0) Gecko/48.0 Firefox/48.0 KAIOS/2.5";
                    httpClient.Request.AcceptEncoding = "gzip, deflate, br";
                    httpClient.StreamResponse = true;
                    //httpClient.Request.KeepAlive = true;
                    //httpClient.Request.AddExtraHeader("Pragma", "no-cache");
                    //httpClient.Request.AddExtraHeader("Cache-Control", "no-cache");
                    //httpClient.Request.AddExtraHeader("Upgrade-Insecure-Requests", "1"); 
                    var res = httpClient.Get(url);
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        return res.ResponseStream;
                    }
                    else if (res.StatusCode == HttpStatusCode.NotFound || res.StatusCode == HttpStatusCode.Unauthorized
                        || res.StatusCode == HttpStatusCode.Forbidden || res.StatusCode == HttpStatusCode.BadGateway
                        || res.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return null;
                    }
                    else
                    {
                        continue;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex.ToString(), "_HttpHelper_");
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
