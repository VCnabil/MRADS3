using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MRADS2.Controls
{
    public class ContentWindow : Window
    {
        Button closebtn;

        public static readonly DependencyProperty CloseBoxProperty = DependencyProperty.Register("CloseBox", typeof(bool), typeof(ContentWindow), new PropertyMetadata(true));
        public bool CloseBox
        {
            get { return (bool)GetValue(CloseBoxProperty); }
            set
            {
                SetValue(CloseBoxProperty, value);

                if (value)
                    closebtn.Visibility = Visibility.Visible;
                else
                    closebtn.Visibility = Visibility.Hidden;
            }
        }

        public UIElement TopElement => ((ContentPanel)Content).Element;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            WindowStyle = WindowStyle.None;

            ContentPanel panel = new ContentPanel(true);
            panel.SetBinding(ContentPanel.TitleProperty, new Binding() { Source = this, Path = new PropertyPath(nameof(Title)) });

            panel.Initialize((UIElement)Content);

            Content = panel;

            panel.TitleBarMouseDown += Panel_TitleBarMouseDown;

            closebtn = new Button();

            closebtn.Width = 40;
            closebtn.HorizontalAlignment = HorizontalAlignment.Right;
            closebtn.VerticalAlignment = VerticalAlignment.Stretch;
            closebtn.Click += Closebtn_Click;
            closebtn.Style = (Style)Application.Current.FindResource("ContentWindowClose");

            if (CloseBox)
                closebtn.Visibility = Visibility.Visible;
            else
                closebtn.Visibility = Visibility.Hidden;

            panel.LayoutGrid.Children.Add(closebtn);
        }

        private void Closebtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Panel_TitleBarMouseDown(ContentPanel obj)
        {
            DragMove();
        }
    }
}
