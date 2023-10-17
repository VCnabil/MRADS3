using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MRADS2.Controls
{
    class MultiChart : Canvas, PlotControl
    {
        List<ChartEntry> charts = new List<ChartEntry>();

        Thickness PlotMargin = new Thickness(50, 20, 30, 20);

        static readonly Color[] ColorList = new Color[]
        {
            Colors.Blue, Colors.Red, Colors.Green, Colors.Cyan, Colors.Violet
        };

        public Action<DateTime> CursorMoveHandler { get; set; } = null;
        public Action<string> RemovePlotCallback { get; set; } = null;

        public DateTime XStart
        {
            get
            {
                return (start);
            }

            set
            {
                start = value;

                foreach (var c in charts)
                    c.Chart.XStart = start;

                xaxis.XStart = start;
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

                foreach (var c in charts)
                    c.Chart.XEnd = end;

                xaxis.XEnd = end;
            }
        }

        DateTime start, end;

        public bool DrawingFreeze
        {
            get
            {
                return (drawingfreeze);
            }

            set
            {
                drawingfreeze = value;

                foreach (var c in charts)
                    c.Chart.DrawingFreeze = drawingfreeze;
            }
        }

        bool drawingfreeze = false;

        public DateTime CursorPosition
        {
            get
            {
                double width = ActualWidth - (PlotMargin.Left + PlotMargin.Right);

                return (XStart.AddTicks((long)((XEnd - XStart).Ticks / width * (cursorline.X1 - PlotMargin.Left))));
            }

            set
            {
                double width = ActualWidth - (PlotMargin.Left + PlotMargin.Right);

                var nx = width / (XEnd - XStart).Ticks * (value - XStart).Ticks;

                if (!double.IsNaN(nx))
                    cursorline.X1 = cursorline.X2 = nx + PlotMargin.Left;
            }
        }

        public bool CursorEnable { get; set; } = true;

        public IEnumerable<string> ChartNames => charts.Select(a => a.Name).Where(a => a != "");

        Grid grid;
        XAxis xaxis;
        Line cursorline;

        public MultiChart()
        {
            grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Binding binding = new Binding("ActualWidth");
            binding.Source = this;
            grid.SetBinding(WidthProperty, binding);

            binding = new Binding("ActualHeight");
            binding.Source = this;
            grid.SetBinding(HeightProperty, binding);

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20, GridUnitType.Pixel) });

            grid.LayoutUpdated += Grid_LayoutUpdated;

            Background = Brushes.Transparent;

            xaxis = new XAxis(PlotMargin);

            xaxis.SetValue(Grid.RowProperty, 0);

            grid.Children.Add(xaxis);

            Children.Add(grid);

            cursorline = new Line();
            cursorline.Stroke = Brushes.Black;
            cursorline.StrokeThickness = 1;
            cursorline.SetValue(ZIndexProperty, 1);
            cursorline.Visibility = Visibility.Visible;
            cursorline.IsHitTestVisible = false;

            Children.Add(cursorline);

            AddChart(LineChart.PlotTypes.Logic, "");
        }

        private void Grid_LayoutUpdated(object sender, EventArgs e)
        {
            if (cursorline.X1 < PlotMargin.Left)
                cursorline.X1 = cursorline.X2 = PlotMargin.Left;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            cursorline.X1 = ActualWidth / 2;
            cursorline.Y1 = 0;
            cursorline.X2 = ActualWidth / 2;
            cursorline.Y2 = ActualHeight - 20;
        }

        void ChartEventCallback(LineChart chart, LineChart.StatusEvents statusevent, object o)
        {
            if (statusevent == LineChart.StatusEvents.RemovePlot)
                RemovePlotCallback((string)o);
        }

        public ChartEntry AddChart(LineChart.PlotTypes plottype, string name = null)
        {
            if (charts.Count == 1 && charts[0].Name == "")
                RemoveChart("");

            var entry = new ChartEntry(grid, plottype, PlotMargin, name == null ? GetNextChartName() : name, grid.RowDefinitions.Count - 1);

            entry.Chart.DrawingFreeze = true;

            entry.Chart.XStart = XStart;
            entry.Chart.XEnd = XEnd;
            entry.Chart.OnStatusChange += ChartEventCallback;

            entry.Chart.DrawingFreeze = false;

            charts.Add(entry);

            xaxis.SetValue(Grid.RowProperty, charts.Count);

            return (entry);
        }

        public void RemoveChart(string name)
        {
            int i;

            for (i = 0; i < charts.Count; i++)
            {
                if (charts[i].Name == name)
                {
                    charts[i].RemoveFromGrid();
                    charts.RemoveAt(i);

                    break;
                }
            }

            for (; i < charts.Count; i++)
                charts[i].Row--;

            if (charts.Count == 0 && name != "")
                AddChart(LineChart.PlotTypes.Logic, "");

            xaxis.SetValue(Grid.RowProperty, charts.Count);
        }

        public ChartEntry AddPlot(string plotname, LineChart.PlotTypes plottype, Color plotcolor, string chartname = null)
        {
            ChartEntry entry;

            if (chartname == null)
                entry = AddChart(plottype);
            else
                entry = charts.SingleOrDefault(a => a.Name == chartname);

            entry.Plots.Add(plotname);
            entry.Chart.AddPlot(plotname, plotcolor);

            return (entry);
        }

        public void RemovePlot(string plotname, bool removeemptychart, string chartname = null)
        {
            ChartEntry[] entries;
            int i;

            if (chartname == null)
                entries = charts.Where(a => a.Plots.Contains(plotname)).ToArray();
            else
                entries = new ChartEntry[] { charts.SingleOrDefault(a => a.Name == chartname) };

            for (i = 0; i < entries.Length; i++)
            {
                entries[i].Plots.Remove(plotname);
                entries[i].Chart.RemovePlot(plotname);

                if (removeemptychart && entries[i].Plots.Count == 0)
                    RemoveChart(entries[i].Name);
            }
        }

        public LineChart GetChart(string name) => charts.SingleOrDefault(a => a.Name == name).Chart;

        public Color GetPlotColor(string name)
        {
            foreach (var c in charts)
            {
                if (c.Plots.Contains(name))
                    return (c.Chart.PlotColor(name));
            }

            throw new Exception("Plot not found");
        }

        public void SetData(string name, DataPointCollection points)
        {
            var chart = charts.Single(a => a.Plots.Contains(name));

            if (chart.Chart.PlotType == LineChart.PlotTypes.Logic)
                chart.Height = .5;

            chart.Chart.UpdateDataPoints(points);
        }

        public void Redraw()
        {
            xaxis.Redraw();

            foreach (var c in charts)
                c.Chart.Redraw();
        }

        public Color GetNextPlotColor()
        {
            int mincountidx = 0, mincount = int.MaxValue;
            int i;

            for (i = 0; i < ColorList.Length; i++)
            {
                int count = 0;

                foreach (var c in charts)
                {
                    foreach (var pn in c.Plots)
                    {
                        if (c.Chart.PlotColor(pn) == ColorList[i])
                            count++;
                    }
                }

                if (count == 0)
                    return (ColorList[i]);

                if (count < mincount)
                {
                    mincount = count;
                    mincountidx = i;
                }
            }

            return (ColorList[mincountidx]);
        }

        string GetNextChartName()
        {
            string ret;
            int i;

            for (i = 1; ; i++)
            {
                ret = $"Chart {i}";

                if (!charts.Any(a => ret == a.Name))
                    break;
            }

            return (ret);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (charts.Count == 0 || !CursorEnable)
                return;

            var mouseloc = e.GetPosition(this).X;

            if (mouseloc < PlotMargin.Left)
                mouseloc = PlotMargin.Left;
            else if (mouseloc > (ActualWidth - PlotMargin.Right))
                mouseloc = ActualWidth - PlotMargin.Right;

            cursorline.X1 = cursorline.X2 = mouseloc;

            if (CursorMoveHandler != null)
                CursorMoveHandler(CursorPosition);
        }
    }

    public class ChartEntry
    {
        public string Name { get; }

        public LineChart Chart { get; }

        public List<string> Plots { get; } = new List<string>();

        public int Row
        {
            get
            {
                return (row);
            }

            set
            {
                row = value;

                Chart.SetValue(Grid.RowProperty, Row);
            }
        }

        public double Height
        {
            get => rowdef.Height.Value;
            set => rowdef.Height = new GridLength(value, GridUnitType.Star);
        }

        int row;
        Grid grid;
        RowDefinition rowdef;

        public ChartEntry(Grid grid, LineChart.PlotTypes plottype, Thickness margin, string name, int row)
        {
            Chart = new LineChart(plottype, name, margin);

            Name = name;
            Row = row;

            this.grid = grid;

            rowdef = new RowDefinition();
            grid.RowDefinitions.Insert(grid.RowDefinitions.Count - 1, rowdef);

            grid.Children.Add(Chart);
        }

        public void RemoveFromGrid()
        {
            grid.Children.Remove(Chart);

            grid.RowDefinitions.Remove(rowdef);
        }
    }

    class XAxis : Canvas
    {
        Typeface typeface = new Typeface("Segoe UI");

        public DateTime XStart { get; set; }
        public DateTime XEnd { get; set; }

        public bool Visible
        {
            get
            {
                return (visible);
            }

            set
            {
                visible = value;

                Redraw();
            }
        }

        public int Divisions
        {
            get
            {
                return (divisions);
            }

            set
            {
                divisions = value;

                Redraw();
            }
        }

        public double SecondsPerDivision
        {
            get
            {
                return (secsperdiv);
            }

            set
            {
                secsperdiv = value;

                Redraw();
            }
        }

        bool visible = true;
        int divisions = 4;
        double secsperdiv = 15;

        Thickness PlotMargin;

        public XAxis(Thickness plotmargin)
        {
            PlotMargin = plotmargin;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (!visible)
            {
                base.OnRender(dc);

                return;
            }

            double width = ActualWidth - (PlotMargin.Left + PlotMargin.Right);
            Pen pen = new Pen(Brushes.Gray, 1);
            Point p1, p2;
            int tickwidth = 5;//NABZ

            p1.X = PlotMargin.Left;
            p1.Y = 0;

            p2.X = p1.X + width;
            p2.Y = 0;


            dc.DrawLine(pen, p1, p2);

            double xsteppix = width / divisions, xstep = secsperdiv;
            DateTime curx = RoundDown(XStart, TimeSpan.FromMinutes(secsperdiv));

            p1.X -= (XStart - curx).TotalSeconds * xsteppix / xstep;
            p2.X = p1.X;
            p2.Y = p1.Y + tickwidth;

            while (p1.X <= (PlotMargin.Left + width))
            {
                if (p1.X >= PlotMargin.Left)
                {
                    dc.DrawLine(pen, p1, p2);

                    DrawXAxisLabel(dc, p2, curx.ToString("HH:mm:ss"));
                }

                p1.X += xsteppix;
                p2.X = p1.X;

                curx = curx.AddSeconds(xstep);
            }
        }

        DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            return (new DateTime(dt.Ticks / d.Ticks * d.Ticks, dt.Kind));
        }

        void DrawXAxisLabel(DrawingContext dc, Point p, string text)
        {
            var dpi = VisualTreeHelper.GetDpi(this);

            FormattedText t = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black, dpi.PixelsPerDip);

            t.TextAlignment = TextAlignment.Center;

            dc.DrawText(t, p);
        }

        public void Redraw()
        {
            InvalidateVisual();
        }
    }
}
