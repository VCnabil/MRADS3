using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MRADS2.Controls
{
    public class LineChart : Canvas, INotifyPropertyChanged
    {
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memset(IntPtr dest, int c, int count);

        public Dictionary<string, DataPointCollection> Points { get; } = new Dictionary<string, DataPointCollection>();
        
        public enum PlotTypes { Logic, Integer, Float };
        public PlotTypes PlotType
        {
            get => plottype;
            set
            {
                plottype = value;

                UpdateYValues();

                if (axis != null)
                    axis.Redraw();

                Redraw();
            }
        }
        PlotTypes plottype;

        Thickness PlotMargin = new Thickness(50, 20, 0, 20);

        public DateTime XStart
        {
            get
            {
                return (start);
            }

            set
            {
                start = value;

                Redraw();
            }
        }

        public DateTime XEnd
        {
            get
            {
                return (end);
            }

            set
            {
                end = value;

                Redraw();
            }
        }

        DateTime start, end;

        public bool DrawingFreeze { get; set; } = false;

        public bool YAxisAutoScale
        {
            get => yaxisautoscale;
            set
            {
                yaxisautoscale = value;

                OnPropertyChanged();

                if (value)
                {
                    UpdateYValues();
                    axis.Redraw();
                }

                Redraw();
            }
        }

        bool yaxisautoscale = true;

        public string YMax
        {
            get => ymax.ToString(axis.LabelFormat);
            set
            {
                if (!double.TryParse(value, out ymax))
                    return;

                if (ymax < ymin)
                {
                    var t = ymin;
                    ymin = ymax;
                    ymax = t;

                    OnPropertyChanged(nameof(YMin));
                }

                OnPropertyChanged();

                YAxisAutoScale = false;

                UpdateYValues();
                axis.Redraw();
                Redraw();
            }
        }

        public string YMin
        {
            get => ymin.ToString(axis.LabelFormat);
            set
            {
                if (!double.TryParse(value, out ymin))
                    return;

                if (ymin > ymax)
                {
                    var t = ymin;
                    ymin = ymax;
                    ymax = t;

                    OnPropertyChanged(nameof(YMax));
                }

                OnPropertyChanged();

                YAxisAutoScale = false;

                UpdateYValues();
                axis.Redraw();
                Redraw();
            }
        }

        public Color PlotColor(string name) => axis.PlotColor(name);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public enum StatusEvents { RemovePlot };
        public event Action<LineChart, StatusEvents, object> OnStatusChange;

        public double PlotTimeout { get; set; } = 5;

        DpiScale dpi;
        Image plot;
        WriteableBitmap bmp;
        Axis axis;

        public LineChart(PlotTypes plottype, string chartname, Thickness margin)
        {
            PlotType = plottype;
            PlotMargin = margin;

            Background = Brushes.Transparent;

            axis = new Axis(PlotMargin, chartname);
            axis.IsHitTestVisible = false;
            Children.Add(axis);

            plot = new Image();
            SetLeft(plot, PlotMargin.Left);
            SetTop(plot, PlotMargin.Top);
            plot.IsHitTestVisible = false;
            SetZIndex(plot, 1);
            Children.Add(plot);

            DataContext = this;

            SizeChanged += LineChart_SizeChanged;

            MinHeight = PlotMargin.Top + PlotMargin.Bottom + 1;
            MinWidth = PlotMargin.Right + PlotMargin.Left + 1;
        }

        private void LineChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int width, height;

            axis.Height = e.NewSize.Height;
            axis.Width = e.NewSize.Width;

            plot.Width = e.NewSize.Width - (PlotMargin.Right + PlotMargin.Left);
            plot.Height = e.NewSize.Height - (PlotMargin.Top + PlotMargin.Bottom);

            width = (int)(dpi.DpiScaleX * plot.Width);
            height = (int)(dpi.DpiScaleY * plot.Height);

            bmp = new WriteableBitmap(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Bgra32, null);
            plot.Source = bmp;

            axis.Redraw();
            Redraw();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            dpi = VisualTreeHelper.GetDpi(this);

            return base.MeasureOverride(constraint);
        }

        void UpdateContextMenu(bool nosizechange)
        {
            ContextMenu cm = new ContextMenu();
            MenuItem mi;

            ContextMenu = cm;

            if (!nosizechange)
            {
                mi = new MenuItem() { Header = "Y-Axis Autoscale", IsCheckable = true };
                mi.SetBinding(MenuItem.IsCheckedProperty, nameof(YAxisAutoScale));

                cm.Items.Add(mi);

                mi = new MenuItem();
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock txtblock = new TextBlock() { Text = "Ymax", MinWidth = 50 };
                TextBox txtbox = new TextBox() { MinWidth = 50 };
                txtbox.KeyDown += YMax_KeyDown;
                txtbox.SetBinding(TextBox.TextProperty, nameof(YMax));

                sp.Children.Add(txtblock);
                sp.Children.Add(txtbox);

                mi.Header = sp;

                cm.Items.Add(mi);

                mi = new MenuItem();
                sp = new StackPanel() { Orientation = Orientation.Horizontal };
                txtblock = new TextBlock() { Text = "Ymin", MinWidth = 50 };
                txtbox = new TextBox() { MinWidth = 50 };
                txtbox.KeyDown += YMin_KeyDown;
                txtbox.SetBinding(TextBox.TextProperty, nameof(YMin));

                sp.Children.Add(txtblock);
                sp.Children.Add(txtbox);

                mi.Header = sp;
                cm.Items.Add(mi);

                cm.Items.Add(new Separator());
            }

            if (Points.Count > 0)
            {
                mi = new MenuItem() { Header = "Remove Plot" };

                foreach (var p in Points)
                {
                    var mi2 = new MenuItem() { Header = p.Key };
                    mi2.Click += PlotRemove_Click;
                    mi.Items.Add(mi2);
                }

                cm.Items.Add(mi);
            }

            mi = new MenuItem() { Header = "Remove Chart" };
            mi.Click += Chart_Remove;

            cm.Items.Add(mi);
        }

        private void PlotRemove_Click(object sender, RoutedEventArgs e)
        {
            StatusChange(StatusEvents.RemovePlot, (string)((MenuItem)sender).Header);
        }

        private void YMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                YMax = ((TextBox)sender).Text;
            }
        }

        private void YMin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                YMin = ((TextBox)sender).Text;
            }
        }

        private void Chart_Remove(object sender, RoutedEventArgs e)
        {
            foreach (var p in Points.Values)
                StatusChange(StatusEvents.RemovePlot, p.PlotName);
        }

        void StatusChange(StatusEvents statusevent, object o = null)
        {
            OnStatusChange?.Invoke(this, statusevent, o);
        }

        public void UpdateDataPoints(DataPointCollection points)
        {
            Points[points.PlotName] = points;

            UpdateYValues();
            axis.Redraw();
            Redraw();
        }

        public void AddPlot(string name, Color plotcolor)
        {
            Points[name] = new DataPointCollection(name);

            axis.AddPlot(name, plotcolor);

            axis.Redraw();

            UpdateContextMenu(PlotType == PlotTypes.Logic);
        }

        public void RemovePlot(string name)
        {
            Points.Remove(name);
            axis.RemovePlot(name);

            UpdateYValues();
            axis.Redraw();
            Redraw();

            UpdateContextMenu(PlotType == PlotTypes.Logic);
        }

        public void Redraw()
        {
            if (DrawingFreeze)
                return;

            DrawPlot();
        }

        double ymax = double.NaN, ymin = double.NaN;

        void UpdateYValues()
        {
            if (axis == null)
                return;

            var ydivisions = axis.Divisions;
            string yaxisfmt;

            if (YAxisAutoScale && Points.Values.Count() > 0)
            {
                if (PlotType == PlotTypes.Logic)
                {
                    ymax = 1;
                    ymin = 0;
                    yaxisfmt = "0";

                    ydivisions = 1;
                }
                else if (PlotType == PlotTypes.Integer)
                {
                    if (YAxisAutoScale || double.IsNaN(ymax))
                    {
                        ymax = Points.Values.Max(a => a.YMax);
                        ymin = Points.Values.Min(a => a.YMin);

                        if (ymax == ymin)
                            ymax += ydivisions;
                        else
                        {
                            int ycountsperdiv = (int)Math.Ceiling((ymax - ymin) / ydivisions);

                            ymax = ymin + ycountsperdiv * ydivisions;
                        }
                    }

                    yaxisfmt = "0";
                }
                else
                {
                    if (YAxisAutoScale || double.IsNaN(ymax))
                    {
                        ymax = Points.Values.Max(a => a.YMax);
                        ymin = Points.Values.Min(a => a.YMin);

                        if (ymax == ymin)
                        {
                            if (ymax == 0)
                            {
                                ymax = 1;
                                ymin = -1;
                            }
                            else
                            {
                                ymax *= 1.1;
                                ymin *= 0.9;

                            }
                        }
                    }

                    yaxisfmt = "0.00";
                }

                OnPropertyChanged(nameof(ymax));
                OnPropertyChanged(nameof(ymin));

                axis.Divisions = ydivisions;
                axis.LabelFormat = yaxisfmt;
            }

            axis.YMax = ymax;
            axis.YMin = ymin;
        }

        void DrawPlot()
        {
            if (ActualWidth == 0 || ActualHeight == 0)
                return;

            if (Points == null)
                return; 

            double drawwidth = bmp.PixelWidth, drawheight = bmp.PixelHeight;

            if (drawwidth < 0)
                drawwidth = 0;
            if (drawheight < 0)
                drawheight = 0;

            double xscale = (drawwidth - 1) / (XEnd - XStart).TotalSeconds;
            double yscale = (drawheight - 1) / (ymax - ymin);

            bmp.Lock();

            ClearBitmap(bmp);

            foreach (var plot in Points.Values)
            {
                if (plot.Data.Count == 0)
                    continue;

                var plotcolor = axis.PlotColor(plot.PlotName);

                Point pp = new Point(double.NaN, double.NaN), np;

                double nextx = 0, x, px = double.NaN;
                double ypmax = double.MinValue, ypmin = double.MaxValue, yplast = double.NaN;

                foreach (var p in plot.Data)
                {
                    x = (p.Time - XStart).Ticks / 10E6;

                    if (x <= px)
                        continue;

                    if (p.Value > ypmax)
                        ypmax = p.Value;
                    if (p.Value < ypmin)
                        ypmin = p.Value;

                    if (x < nextx)
                    {
                        yplast = p.Value;

                        px = x;

                        continue;
                    }

                    nextx = x + 1 / xscale;

                    np = new Point(x * xscale, drawheight - 1 - (p.Value - ymin) * yscale);

                    if (!double.IsNaN(px) && (x - px) > PlotTimeout)
                        pp = new Point(double.NaN, double.NaN);

                    if (!double.IsNaN(pp.X))
                    {
                        if (ypmin != ypmax)
                        {
                            DrawLine(bmp, new Point(pp.X, drawheight - 1 - (ypmin - ymin) * yscale),
                                new Point(pp.X, drawheight - (ypmax - ymin) * yscale), plotcolor);
                        }

                        if (!double.IsNaN(yplast))
                        {
                            double ny = drawheight - 1 - (yplast - ymin) * yscale;

                            DrawLine(bmp, pp, new Point(pp.X, ny), plotcolor);
                            pp.Y = ny;
                        }

                        if (PlotType != PlotTypes.Float)
                        {
                            var mp = new Point(np.X, pp.Y);

                            DrawLine(bmp, pp, mp, plotcolor);
                            DrawLine(bmp, mp, np, plotcolor);
                        }
                        else
                            DrawLine(bmp, pp, np, plotcolor);
                    }

                    ypmax = double.MinValue;
                    ypmin = double.MaxValue;
                    yplast = double.NaN;

                    pp = np;

                    px = x;
                }
            }

            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));

            bmp.Unlock();
        }

        static void ClearBitmap(WriteableBitmap bmp)
        {
            unsafe
            {
                memset(bmp.BackBuffer, 0, bmp.PixelWidth * bmp.PixelHeight * 4);
            }
        }

        static int abs(int v)
        {
            return (v < 0 ? -v : v);
        }

        static void DrawLine(WriteableBitmap bmp, Point p1, Point p2, Color c)
        {
            DrawLine(bmp, (int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, c);
        }

        static void DrawLine(WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color c)
        {
            int dx = abs(x2 - x1), dy = -abs(y2 - y1);
            int sx = (x1 < x2) ? 1 : -1, sy = (y1 < y2) ? 1 : -1;
            int err = dx + dy;
            int e2;
            int existingcolor;
            IntPtr ptr = bmp.BackBuffer;

            unsafe
            {
                while (true)
                {
                    if (x1 >= 0 && x1 < bmp.PixelWidth && y1 >= 0 && y1 < bmp.PixelHeight)
                    {
                        ptr = bmp.BackBuffer + y1 * bmp.BackBufferStride + x1 * 4;

                        int color = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
                        existingcolor = *(int*)ptr;

                        if (existingcolor != 0 && existingcolor != color)
                            *(int*)ptr = color | existingcolor;
                        else
                            *(int*)ptr = color;
                    }

                    if (x1 == x2 && y1 == y2)
                        break;

                    e2 = 2 * err;

                    if (e2 >= dy)
                    {
                        err += dy;
                        x1 += sx;
                    }
                    if (e2 <= dx)
                    {
                        err += dx;
                        y1 += sy;
                    }
                }
            }
        }
    }
    
    class Axis : Canvas
    {
        public double YMax;
        public double YMin;
        public int Divisions { get; set; } = 4;
        public Thickness PlotMargin { get; set; }
        public string LabelFormat { get; set; }
        public string ChartName { get; set; }

        Typeface typeface = new Typeface("Segoe UI"), typeface2 = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        DpiScale dpi;

        Dictionary<string, Color> plots = new Dictionary<string, Color>();

        public Color PlotColor(string name) => plots[name];

        public Axis(Thickness plotmargin, string chartname, params (string plotname, Color plotcolor)[] plots)
        {
            PlotMargin = plotmargin;
            ChartName = chartname;
            this.plots = plots.ToDictionary(a => a.plotname, a => a.plotcolor);
        }

        public void Redraw()
        {
            InvalidateVisual();
        }

        public void AddPlot(string name, Color color)
        {
            plots[name] = color;
        }

        public void RemovePlot(string name)
        {
            plots.Remove(name);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            dpi = VisualTreeHelper.GetDpi(this);

            return base.MeasureOverride(constraint);
        }

        protected override void OnRender(DrawingContext dc)
        {
            DrawAxis(dc, YMax, YMin, Divisions, LabelFormat, true);
        }

        //nabil
        void DrawAxis(DrawingContext dc, double ytop, double ybot, int ydivisions, string labelfmt, bool drawlabels = true)
        {
            Pen pen = new Pen(Brushes.Red, 1);
            Point yaxistop, yaxisbot, p1, p2;
            int tickwidth = 5, i;

            yaxistop.X = PlotMargin.Left;
            yaxistop.Y = PlotMargin.Top;

            yaxisbot.X = PlotMargin.Left;
            yaxisbot.Y = ActualHeight - PlotMargin.Bottom;

            dc.DrawLine(pen, yaxistop, yaxisbot);

            double ysteppix = (yaxisbot.Y - yaxistop.Y) / ydivisions, ystep = (ytop - ybot) / ydivisions;
            double cury = ytop;

            p1.X = yaxistop.X;
            p1.Y = yaxistop.Y;

            p2.X = yaxistop.X - tickwidth;
            p2.Y = yaxistop.Y;

            for (i = 0; i < ydivisions + 1; i++)
            {
                dc.DrawLine(pen, p1, p2);

                if (plots.Count > 0)
                    DrawYAxisLabel(dc, p2, cury.ToString(labelfmt));

                p1.Y += ysteppix;
                p2.Y = p1.Y;
                cury -= ystep;
            }

            if (plots.Count > 0)
                DrawPlotLabels(dc);
        }

        void DrawPlotLabels(DrawingContext dc)
        {
            int boxsize = 7, spacing = 4;
            Point loc = new Point(PlotMargin.Left, ActualHeight - PlotMargin.Bottom);

            FormattedText t = new FormattedText(ChartName, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface2, 12, Brushes.Black, dpi.PixelsPerDip);

            t.TextAlignment = TextAlignment.Left;

            dc.DrawText(t, loc);

            loc.X += spacing * 2 + t.Width;

            foreach (var p in plots)
            {
                t = new FormattedText(p.Key, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, dpi.PixelsPerDip);

                t.TextAlignment = TextAlignment.Left;

                dc.DrawRectangle(new SolidColorBrush(p.Value), new Pen(Brushes.Black, 1), new Rect(loc.X, loc.Y + t.Height / 2 - boxsize / 2, boxsize, boxsize));

                loc.X += boxsize + spacing;

                dc.DrawText(t, loc);

                loc.X += spacing * 2 + t.Width;
            }
        }

        void DrawYAxisLabel(DrawingContext dc, Point p, string text)
        {
            FormattedText t = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, dpi.PixelsPerDip);

            t.TextAlignment = TextAlignment.Right;
            p.X -= 5;
            p.Y -= t.Height / 2;

            dc.DrawText(t, p);
        }
    }

    public class DataPointCollection
    {
        public string PlotName;

        public List<DataPoint> Data { get; } = new List<DataPoint>();

        public DateTime StartTime => Data[0].Time;
        public DateTime EndTime => Data[Data.Count - 1].Time;

        public TimeSpan XSpan => EndTime - StartTime;
        public double YSpan => Data.Max(a => a.Value) - Data.Min(a => a.Value);

        public double YMax { get; private set; } = double.NegativeInfinity;
        public double YMin { get; private set; } = double.PositiveInfinity;

        public DataPointCollection(string plotname)
        {
            PlotName = plotname;
        }
        
        public void AddPoint(DateTime time, double value)
        {
            Data.Add(new DataPoint() { Time = time, Value = value });

            if (value < YMin)
                YMin = value;
            if (value > YMax)
                YMax = value;
        }
    }

    public struct DataPoint
    {
        public DateTime Time;
        public double Value;

        public override string ToString()
        {
            return ($"({Time.Ticks}, {Value})");
        }
    }
}
