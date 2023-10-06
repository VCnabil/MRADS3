using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace MRADS2.Controls
{
    class TimeTextBox : Border, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(DateTime), typeof(TimeTextBox),
            new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimeChanged));

        private static void TimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.TimeBind.Time = (DateTime)e.NewValue;
        }

        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty MillisecondFieldProperty = DependencyProperty.Register("MillisecondField", typeof(bool), typeof(TimeTextBox),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None, MillisecondFieldChanged));

        private static void MillisecondFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.Init();
        }

        public bool MillisecondField
        {
            get { return (bool)GetValue(MillisecondFieldProperty); }
            set { SetValue(MillisecondFieldProperty, value); }
        }

        public enum SelectionOption { Date, Time, DateTime };

        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(SelectionOption), typeof(TimeTextBox),
            new FrameworkPropertyMetadata(SelectionOption.DateTime, FrameworkPropertyMetadataOptions.None, SelectionChanged));

        private static void SelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.Init();
        }

        public SelectionOption Selection
        {
            get { return (SelectionOption)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(TimeTextBox),
           new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.None, FontSizeChanged));

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.Init();
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(TimeTextBox),
           new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), FrameworkPropertyMetadataOptions.None, FontFamilyChanged));

        private static void FontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.Init();
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly DependencyProperty MaxTimeProperty = DependencyProperty.Register("MaxTime", typeof(DateTime), typeof(TimeTextBox),
            new FrameworkPropertyMetadata(DateTime.MaxValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, MaxTimeChanged));

        private static void MaxTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.TimeBind.MaxTime = (DateTime)e.NewValue;
        }

        public DateTime MaxTime
        {
            get { return (DateTime)GetValue(MaxTimeProperty); }
            set { SetValue(MaxTimeProperty, value); }
        }

        public static readonly DependencyProperty MinTimeProperty = DependencyProperty.Register("MinTime", typeof(DateTime), typeof(TimeTextBox),
            new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, MinTimeChanged));

        private static void MinTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TimeTextBox)d;

            tb.TimeBind.MinTime = (DateTime)e.NewValue;
        }

        public DateTime MinTime
        {
            get { return (DateTime)GetValue(MinTimeProperty); }
            set { SetValue(MinTimeProperty, value); }
        }

        TextBox year, month, day, hour, min, sec, ms;
        Grid grid;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class DTBind : INotifyPropertyChanged
        {
            public DateTime MinTime, MaxTime;

            public DateTime Time
            {
                get => dt;
                
                set
                {
                    UpdateDT(value);
                }
            }

            DateTime dt;
            Action<DateTime> dtchangecb;

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public string Year
            {
                get => dt.Year.ToString("0000");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Year);
                    
                    try
                    {
                        newdt = new DateTime(val, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            public string Month
            {
                get => dt.Month.ToString("00");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Month);
                    if (val < 1 || val > 12)
                        return;

                    try
                    {
                        newdt = new DateTime(dt.Year, val, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            public string Day
            {
                get => dt.Day.ToString("00");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Month);
                    if (val < 1 || val > 31)
                        return;

                    try
                    {
                        newdt = new DateTime(dt.Year, dt.Month, val, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            public string Hour
            {
                get => dt.Hour.ToString("00");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Hour);
                    if (val > 23)
                        return;

                    try
                    {
                        newdt = new DateTime(dt.Year, dt.Month, dt.Day, val, dt.Minute, dt.Second, dt.Millisecond);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            public string Minute
            {
                get => dt.Minute.ToString("00");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Minute);
                    if (val > 59)
                        return;

                    try
                    {
                        newdt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, val, dt.Second, dt.Millisecond);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            public string Second
            {
                get => dt.Second.ToString("00");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Second);
                    if (val > 59)
                        return;

                    try
                    {
                        newdt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, val, dt.Millisecond);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            public string Millisecond
            {
                get => dt.Millisecond.ToString("000");

                set
                {
                    DateTime newdt;

                    var val = ParseVal(value, dt.Millisecond);

                    try
                    {
                        newdt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, val);
                    }
                    catch
                    {
                        return;
                    }

                    UpdateDT(newdt);
                }
            }

            void UpdateDT(DateTime newdt)
            {
                if (newdt >= MinTime && newdt <= MaxTime)
                    dt = newdt;

                OnPropertyChanged(nameof(Year));
                OnPropertyChanged(nameof(Month));
                OnPropertyChanged(nameof(Day));
                OnPropertyChanged(nameof(Hour));
                OnPropertyChanged(nameof(Minute));
                OnPropertyChanged(nameof(Second));
                OnPropertyChanged(nameof(Millisecond));

                dtchangecb(dt);
            }

            int ParseVal(string val, int oldval)
            {
                int newval;

                if (!int.TryParse(val, out newval))
                    return (oldval);

                return (newval);
            }

            public DTBind(DateTime initval, DateTime min, DateTime max, Action<DateTime> dtchangecb)
            {
                this.dtchangecb = dtchangecb;

                MinTime = min;
                MaxTime = max;

                UpdateDT(initval);
            }
        }

        public DTBind TimeBind { get; set; }

        public TimeTextBox()
        {
            TimeBind = new DTBind(Time, MinTime, MaxTime, dt =>
            {
                Time = dt;

                OnPropertyChanged(nameof(Time));
            });

            BorderBrush = Brushes.Gray;
            BorderThickness = new Thickness(1);
            Padding = new Thickness(5);

            grid = new Grid();
            grid.Margin = new Thickness(0);

            Child = grid;

            Init();
        }

        void Init()
        {
            int col = 0;

            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            if (Selection == SelectionOption.Date || Selection == SelectionOption.DateTime)
            {
                InitTB(ref year, col++, "TimeBind.Year", 4);
                InitSep("-", col++);
                InitTB(ref month, col++, "TimeBind.Month");
                InitSep("-", col++);
                InitTB(ref day, col++, "TimeBind.Day");
            }

            if (Selection == SelectionOption.DateTime)
                InitSep(" ", col++);

            if (Selection == SelectionOption.Time || Selection == SelectionOption.DateTime)
            {
                InitTB(ref hour, col++, "TimeBind.Hour");
                InitSep(":", col++);
                InitTB(ref min, col++, "TimeBind.Minute");
                InitSep(":", col++);
                InitTB(ref sec, col++, "TimeBind.Second");

                if (MillisecondField)
                {
                    InitSep(".", col++);
                    InitTB(ref ms, col++, "TimeBind.Millisecond", 3);
                }
            }
        }

        void InitTB(ref TextBox box, int col, string binding, int maxlen = 2)
        {
            box = new TextBox();
            //box.MaxLength = maxlen;
            box.FontSize = FontSize;
            box.FontFamily = FontFamily;
            box.TextChanged += (sender, e) => TextChanged((TextBox)sender, maxlen, e);
            box.KeyDown += Box_KeyDown;
            box.PreviewKeyDown += Box_PreviewKeyDown;
            box.BorderThickness = new Thickness(0);
            box.VerticalContentAlignment = VerticalAlignment.Center;
            box.Margin = new Thickness(0);
            box.Padding = new Thickness(0);

            PropertyInfo textEditorProperty = typeof(TextBox).GetProperty(
                  "TextEditor", BindingFlags.NonPublic | BindingFlags.Instance);

            object textEditor = textEditorProperty.GetValue(box, null);

            // set _OvertypeMode on the TextEditor
            PropertyInfo overtypeModeProperty = textEditor.GetType().GetProperty(
                           "_OvertypeMode", BindingFlags.NonPublic | BindingFlags.Instance);

            overtypeModeProperty.SetValue(textEditor, true, null);

            box.SetBinding(TextBox.TextProperty, new Binding() { Source = this, Path = new PropertyPath(binding) });

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            box.SetValue(Grid.ColumnProperty, col);

            grid.Children.Add(box);
        }

        private void Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
                e.Handled = true;
        }

        private void Box_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        void TextChanged(TextBox box, int nchars, TextChangedEventArgs e)
        {
            int caretloc = box.CaretIndex;

            if (box.Text.Length < nchars)
                box.Text = box.Text.Substring(0, caretloc) + new string('0', nchars - box.Text.Length) + box.Text.Substring(caretloc);
            else if (box.Text.Length > nchars)
            {
                string nt = box.Text.Substring(nchars);

                box.Text = box.Text.Substring(0, nchars);

                if (caretloc >= nchars)
                {
                    box.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                    var bx = grid.Children.Cast<UIElement>().SingleOrDefault(a => a.IsFocused);
                    if (bx != null)
                    {
                        ((TextBox)bx).Text = nt + ((TextBox)bx).Text;

                        ((TextBox)bx).CaretIndex = nt.Length;

                        return;
                    }
                }
            }

            box.CaretIndex = caretloc;
        }

        void InitSep(string v, int col)
        {
            Label label;

            label = new Label();
            label.Content = v;
            label.FontSize = FontSize;
            label.FontFamily = FontFamily;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.Background = Brushes.White;
            label.Margin = new Thickness(0);
            label.Padding = new Thickness(0);

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            label.SetValue(Grid.ColumnProperty, col);

            grid.Children.Add(label);
        }
    }
}
