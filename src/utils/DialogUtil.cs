using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nine_colored_deer_Sharp.utils
{
    public class DialogUtil
    {

        public static void success(Grid GrdContainer, string msg, int second = 3)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                CancellationToken token = cts.Token;
                if (lastTask != null)
                {
                    //取消上一个线程
                    cts.Cancel();
                    cts.Dispose();
                    cts = new CancellationTokenSource();
                    token = cts.Token;
                    cts.Token.Register(() =>
                    {
                        Console.WriteLine("Grid删除线程退出");
                    });
                }
                Border border = new Border();
                border.Padding = new Thickness(10);
                border.Background = new SolidColorBrush(Colors.Black);
                border.VerticalAlignment = VerticalAlignment.Center;
                border.HorizontalAlignment = HorizontalAlignment.Center;
                border.Margin = new Thickness(0, 0, 0, 200);
                border.CornerRadius = new CornerRadius(5);
                border.BorderBrush = new SolidColorBrush(Colors.LightGray); ;
                border.Background = new SolidColorBrush(Colors.White);
                border.Height = 50;
                border.BorderThickness = new Thickness(1, 1, 1, 1);
                //border.BitmapEffect = new DropShadowBitmapEffect() { Softness = 0.8, Opacity = 0.8, ShadowDepth = 5, Direction = 300 };

                Grid gd = new Grid() { Background = new SolidColorBrush(Colors.White) };
                border.Child = gd;
                StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock textBlock = new TextBlock()
                {
                    Foreground = new SolidColorBrush(Colors.Gray),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 20,
                    Text = msg
                };
                var url = new Uri("pack://application:,,,/images/success.png", UriKind.RelativeOrAbsolute);
                var stream = Application.GetResourceStream(url).Stream;
                Image image = new Image();

                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    bitmapImage.BeginInit();
                    // (\w+).CreateOptions = BitmapCreateOptions.DelayCreation;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    stream.Close();
                }

                image.Source = bitmapImage;
                image.UseLayoutRounding = true;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                gd.Children.Add(stackPanel);
                stackPanel.Children.Add(image);
                stackPanel.Children.Add(textBlock);
                GrdContainer.Children.Clear();
                GrdContainer.Children.Add(border);
                AnimationHelper.SetBeginTimeSeconds(border, 0);
                AnimationHelper.SetDurationSeconds(border, 0.5);
                AnimationHelper.SetFadeIn(border, true);
                AnimationHelper.SetSlideInFromTop(border, true);

                var tokentemp = token;
                lastTask = Task.Delay(TimeSpan.FromSeconds(second)).ContinueWith((t) =>
                {
                    if (tokentemp.IsCancellationRequested)
                    {
                        return;
                    }
                    GrdContainer.Dispatcher.Invoke(() =>
                    {
                        if (tokentemp.IsCancellationRequested)
                        {
                            return;
                        }
                        GrdContainer.Children.Clear();
                    });
                }, tokentemp);
            });
        }

        private static Task lastTask = null;
        static CancellationTokenSource cts = new CancellationTokenSource();

        static DialogUtil()
        {
            cts.Token.Register(() =>
            {
                Console.WriteLine("Grid删除线程退出");
            });
        }

        public static void info(Grid GrdContainer, string msg, int second = 3, double marginBottom = 200)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                CancellationToken token = cts.Token;
                if (lastTask != null)
                {
                    //取消上一个线程
                    cts.Cancel();
                    cts.Dispose();
                    cts = new CancellationTokenSource();
                    token = cts.Token;
                    cts.Token.Register(() =>
                    {
                        Console.WriteLine("Grid删除线程退出");
                    });
                }
                Border border = new Border();
                border.Padding = new Thickness(10);
                border.Background = new SolidColorBrush(Colors.Black);
                border.VerticalAlignment = VerticalAlignment.Center;
                border.HorizontalAlignment = HorizontalAlignment.Center;
                border.Margin = new Thickness(0, 0, 0, marginBottom);
                border.CornerRadius = new CornerRadius(5);
                border.Height = 50;
                TextBlock textBlock = new TextBlock()
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 20,
                    Text = msg
                };

                border.Child = textBlock;
                GrdContainer.Children.Clear();
                GrdContainer.Children.Add(border);
                AnimationHelper.SetBeginTimeSeconds(border, 0);
                AnimationHelper.SetDurationSeconds(border, 0.5);
                AnimationHelper.SetFadeIn(border, true);
                AnimationHelper.SetSlideInFromTop(border, true);
                var tokentemp = token;
                lastTask = Task.Delay(TimeSpan.FromSeconds(second)).ContinueWith(t =>
                {
                    if (tokentemp.IsCancellationRequested)
                    {
                        return;
                    }
                    GrdContainer.Dispatcher.Invoke(() =>
                    {
                        if (tokentemp.IsCancellationRequested)
                        {
                            return;
                        }
                        GrdContainer.Children.Clear();
                    });
                }, tokentemp);
            });
        }
    }
}
