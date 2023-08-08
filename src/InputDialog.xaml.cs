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
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string data = "")
        {
            InitializeComponent();
            this.txt1.Text = data;
            this.Loaded += InputDialog_Loaded;
        }

        private void InputDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() => { txt1.Focus(); }));
        }

        public string value { get; set; }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            value = this.txt1.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
