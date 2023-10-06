using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using MRADS2.Controls;

namespace MRADS2
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutWindow : ContentWindow
    {
        public string Version => $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";

        public AboutWindow()
        {
            InitializeComponent();

            TopElement.MouseDown += AboutWindow_MouseDown;
        }

        private void AboutWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
