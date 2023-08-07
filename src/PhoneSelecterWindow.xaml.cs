using Nine_colored_deer_Sharp.utils;
using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Nine_colored_deer_Sharp
{
    /// <summary>
    /// PhoneSelecterWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PhoneSelecterWindow : Window
    {
        public bool isOk = false;
        public DeviceData selectedDevice { get; set; } = null;
        List<DeviceData> devices;
        public PhoneSelecterWindow(List<DeviceData> devices)
        {
            InitializeComponent();
            isOk = false;
            this.devices = devices;
            this.Loaded += PhoneSelecterWindow_Loaded;
            this.Closed += PhoneSelecterWindow_Closed;
        }

        private void PhoneSelecterWindow_Closed(object sender, EventArgs e)
        {
            isOk = true;
        }

        private void PhoneSelecterWindow_Loaded(object sender, RoutedEventArgs e)
        {
            items.ItemsSource = devices;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                DialogUtil.info(grid_info, "请选择一个手机！");
                return;
            }
            selectedDevice = devices.Where(p => p.Serial == tag).FirstOrDefault();
            isOk = true;
            this.Close();
        }

        private void btn_cancle_Click(object sender, RoutedEventArgs e)
        {
            isOk = true;
            this.Close();
        }

        string tag = "";

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            tag = (sender as RadioButton)?.Tag?.ToString();

        }
    }
}
