using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MRADS2.Controls
{
    class ContentPanel : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ContentPanel));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public UIElement Element { get; private set; }
        public Grid LayoutGrid { get; private set; }

        public event Action<ContentPanel> TitleBarMouseDown;
        public event Action<ContentPanel> TitleBarMouseUp;

        bool delayedinit;

        public ContentPanel()
        {
        }

        public ContentPanel(bool delayedinit = false)
        {
            this.delayedinit = delayedinit;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!delayedinit)
                Initialize((UIElement)Content);
        }

        public void Initialize(UIElement element)
        {
            Element = element;

            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x5D, 0x6B, 0x99));

            Grid grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            Label label = new Label();
            label.SetBinding(ContentProperty, new Binding() { Path = new PropertyPath(nameof(Title)), Source = this });
            label.Foreground = Brushes.White;
            label.Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x4F, 0x81));
            label.MouseDown += Label_MouseDown;
            label.MouseUp += Label_MouseUp;

            ContentControl cc = new ContentControl();
            cc.Content = Element;
            cc.SetValue(Grid.RowProperty, 1);

            grid.Children.Add(label);
            grid.Children.Add(cc);

            border.Child = grid;

            Content = border;

            LayoutGrid = grid;
        }

        private void Label_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TitleBarMouseUp?.Invoke(this);
        }

        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TitleBarMouseDown?.Invoke(this);
        }
    }
}
