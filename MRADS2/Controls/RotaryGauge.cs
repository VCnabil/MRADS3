using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MRADS2.Controls
{
    class RotaryGauge : Canvas
    {
        public static readonly DependencyProperty TickMarkAngleStartProperty = DependencyProperty.Register("TickMarkAngleStart", typeof(double), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(0.0, RenderPropChanged));

        public double TickMarkAngleStart
        {
            get { return (double)GetValue(TickMarkAngleStartProperty); }
            set { SetValue(TickMarkAngleStartProperty, value); }
        }

        public static readonly DependencyProperty TickMarkAngleEndProperty = DependencyProperty.Register("TickMarkAngleEnd", typeof(double), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(180.0, RenderPropChanged));
        public double TickMarkAngleEnd
        {
            get { return (double)GetValue(TickMarkAngleEndProperty); }
            set { SetValue(TickMarkAngleEndProperty, value); }
        }

        public static readonly DependencyProperty MajorTicksProperty = DependencyProperty.Register("MajorTicks", typeof(TickConfig), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(new TickConfig() { Count = 3, Length = 15, Width = 5 }, RenderPropChanged));
        public TickConfig MajorTicks
        {
            get { return (TickConfig)GetValue(MajorTicksProperty); }
            set { SetValue(MajorTicksProperty, value); }
        }

        public static readonly DependencyProperty MinorTicksProperty = DependencyProperty.Register("MinorTicks", typeof(TickConfig), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(new TickConfig() { Count = 3, Length = 9, Width = 4 }, RenderPropChanged));
        public TickConfig MinorTicks
        {
            get { return (TickConfig)GetValue(MinorTicksProperty); }
            set { SetValue(MinorTicksProperty, value); }
        }

        public static readonly DependencyProperty ZonesProperty;
        public RotaryGaugeZoneConfig[] Zones
        {
            get { return (RotaryGaugeZoneConfig[])GetValue(ZonesProperty); }
            set { SetValue(ZonesProperty, value); }
        }

        public static readonly DependencyProperty HubProperty = DependencyProperty.Register("Hub", typeof(HubConfig), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(new HubConfig() { Color = Colors.Black, Radius = 11 }, RenderPropChanged));
        public HubConfig Hub
        {
            get { return (HubConfig)GetValue(HubProperty); }
            set { SetValue(HubProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(0.0, RenderPropChanged));
        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(1000.0, RenderPropChanged));
        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(500.0, FrameworkPropertyMetadataOptions.AffectsRender, ValueChanged));

        public static readonly DependencyProperty NeedleProperty = DependencyProperty.Register("Needle", typeof(NeedleConfig), typeof(RotaryGauge),
            new FrameworkPropertyMetadata(new NeedleConfig() { LengthInside = 1, OverlapTicks = false, LengthOutside = 10, WidthBase = 6, WidthMiddle = 6, WidthEnd = 2, Color = Colors.Red }, RenderPropChanged));
        
        public static readonly DependencyProperty TickWidthProperty = DependencyProperty.Register("TickWidth", typeof(double), typeof(RotaryGauge), new PropertyMetadata(10.0));
        public double TickWidth
        {
            get { return (double)GetValue(TickWidthProperty); }
            set { SetValue(TickWidthProperty, value); }
        }
        public NeedleConfig Needle
        {
            get { return (NeedleConfig)GetValue(NeedleProperty); }
            set { SetValue(NeedleProperty, value); }
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RotaryGauge)d).UpdateNeedle();
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Radius { get; private set; }

        public Point Center { get; private set; }

        DrawingGroup statics, needle;

        public RotaryGauge()
        {
            statics = new DrawingGroup();
            needle = new DrawingGroup();
        }

        static RotaryGauge()
        {
            RotaryGaugeZoneConfig[] defaultzones = new RotaryGaugeZoneConfig[2];

            defaultzones[0] = new RotaryGaugeZoneConfig() { StartAngle = 0, EndAngle = 90, Length = 9, Radius = 1, Color = Colors.Lime };
            defaultzones[1] = new RotaryGaugeZoneConfig() { StartAngle = 90, EndAngle = 180, Length = 9, Radius = 1, Color = Colors.Red };

            ZonesProperty = DependencyProperty.Register("Zones", typeof(RotaryGaugeZoneConfig[]), typeof(RotaryGauge), new FrameworkPropertyMetadata(defaultzones, RenderPropChanged));
        }

        private static void RenderPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RotaryGauge)d).Update();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Update();

            base.OnRenderSizeChanged(sizeInfo);
        }

        void Update()
        {
            Point center;

            double x1 = Math.Cos(TickMarkAngleEnd * Math.PI / 180), x2 = Math.Cos(TickMarkAngleStart * Math.PI / 180);
            double y1 = Math.Sin(TickMarkAngleEnd * Math.PI / 180), y2 = Math.Sin(TickMarkAngleStart * Math.PI / 180);

            if (x1 < 0 && x2 < 0)
                center.X = ActualWidth;
            else if (x1 > 0 && x2 > 0)
                center.X = 0;
            else
                center.X = ActualWidth * Math.Abs(x1) / Math.Abs(x1 - x2);

            if (center.X < Hub.Radius)
                center.X = Hub.Radius;
            else if (center.X > (ActualWidth - Hub.Radius))
                center.X = ActualWidth - Hub.Radius;

            if (y1 < 0 && y2 < 0)
                center.Y = 0;
            else if (y1 > 0 && y2 > 0)
                center.Y = ActualHeight;
            else
                center.Y = ActualHeight - ActualHeight * Math.Abs(y1) / Math.Abs(y1 - y2);

            if (center.Y < Hub.Radius)
                center.Y = Hub.Radius;
            else if (center.Y > (ActualHeight - Hub.Radius))
                center.Y = ActualHeight - Hub.Radius;

            Center = center;

            double mindist = double.PositiveInfinity;
            double maxx = double.NegativeInfinity, maxy = double.NegativeInfinity, minx = double.PositiveInfinity, miny = double.PositiveInfinity;
            Point minpt;
            List<double> angles = new List<double>();
            bool clockwise = (TickMarkAngleEnd - TickMarkAngleStart) < 0;

            double[] cardinals = new double[] { -360, -270, -180, -90, 0, 90, 180, 270, 360 };
            int i;

            angles.Add(TickMarkAngleStart);

            for (i = 0; i < cardinals.Length; i++)
            {
                if (TickMarkAngleStart > cardinals[i] && TickMarkAngleStart < cardinals[i + 1])
                    break;
            }

            if (!clockwise)
                i++;

            while (true)
            {
                if ((clockwise && cardinals[i] < TickMarkAngleEnd) || (!clockwise && cardinals[i] > TickMarkAngleEnd))
                    break;

                angles.Add(cardinals[i]);

                if (clockwise)
                    i--;
                else
                    i++;
            }

            angles.Add(TickMarkAngleEnd);

            foreach (var a in angles)
            {
                double cos = Math.Cos(a * Math.PI / 180), sin = Math.Sin(a * Math.PI / 180);
                double v, d, dist;

                if (cos > maxx)
                    maxx = cos;
                if (cos < minx)
                    minx = cos;
                if (sin > maxy)
                    maxy = sin;
                if (sin < miny)
                    miny = sin;

                if (cos >= 0)
                    v = ActualWidth;
                else
                    v = 0;

                d = Center.Y - sin / cos * (v - Center.X);

                dist = Math.Sqrt(Math.Pow(v - Center.X, 2) + Math.Pow(d - Center.Y, 2));
                if (dist < mindist)
                {
                    mindist = dist;
                    minpt = new Point(v, d);
                }

                if (sin >= 0)
                    v = 0;
                else
                    v = ActualHeight;

                d = cos / sin * (Center.Y - v) + Center.X;

                dist = Math.Sqrt(Math.Pow(v - Center.Y, 2) + Math.Pow(d - Center.X, 2));
                if (dist < mindist)
                {
                    mindist = dist;
                    minpt = new Point(d, v);
                }
            }

            Radius = mindist;

            maxx = Radius * maxx + Center.X;
            minx = Radius * minx + Center.X;

            var t = maxy;
            maxy = Center.Y - Radius * miny;
            miny = Center.Y - Radius * t;

            if (Center.X > maxx)
                maxx = Center.X;
            if (Center.X < minx)
                minx = Center.X;
            if (Center.Y > maxy)
                maxy = Center.Y;
            if (Center.Y < miny)
                miny = Center.Y;

            center = new Point((ActualWidth - (maxx + minx)) / 2 + Center.X, (ActualHeight - (maxy + miny)) / 2 + Center.Y);

            if (center.X < Hub.Radius)
                center.X = Hub.Radius;
            else if (center.X > (ActualWidth - Hub.Radius))
                center.X = ActualWidth - Hub.Radius;

            if (center.Y < Hub.Radius)
                center.Y = Hub.Radius;
            else if (center.Y > (ActualHeight - Hub.Radius))
                center.Y = ActualHeight - Hub.Radius;

            Center = center;

            DrawStatics();
            DrawNeedle();
            UpdateNeedle();
        }

        void DrawStatics()
        {
            double angledelta;
            int i;

            var dc = statics.Open();

            foreach (var zone in Zones)
            {
                StreamGeometry strm = new StreamGeometry();
                Point start, end;
                double rotation = zone.EndAngle - zone.StartAngle;

                var dc2 = strm.Open();

                var r = zone.Radius * (Radius - zone.Length / 2);

                start.X = r * Math.Cos(zone.StartAngle * Math.PI / 180);
                start.Y = r * Math.Sin(zone.StartAngle * Math.PI / 180);

                end.X = r * Math.Cos(zone.EndAngle * Math.PI / 180);
                end.Y = r * Math.Sin(zone.EndAngle * Math.PI / 180);

                dc2.BeginFigure(Transform(start), false, false);
                dc2.ArcTo(Transform(end), new Size(r, r), 0, Math.Abs(rotation) > 180, rotation < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, true);

                dc2.Close();

                dc.DrawGeometry(null, new Pen(new SolidColorBrush(zone.Color), zone.Length), strm);
            }

            DrawTicks(dc, MajorTicks, TickMarkAngleStart, TickMarkAngleEnd);

            angledelta = (TickMarkAngleEnd - TickMarkAngleStart) / (MajorTicks.Count - 1);
            var minordelta = angledelta / (MinorTicks.Count + 1);

            for (i = 0; i < MajorTicks.Count - 1; i++)
                DrawTicks(dc, MinorTicks, i * angledelta + minordelta + TickMarkAngleStart, (i + 1) * angledelta - minordelta + TickMarkAngleStart);

            dc.DrawEllipse(new SolidColorBrush(Hub.Color), null, new Point(Center.X, Center.Y), Hub.Radius, Hub.Radius);

            dc.Close();
        }

        Point Transform(Point p)
        {
            p.X += ActualWidth / 2;
            p.Y = ActualHeight - (p.Y + ActualHeight / 2);

            p.X -= ActualWidth / 2 - Center.X;
            p.Y -= ActualHeight / 2 - Center.Y;

            return (p);
        }

        void DrawTicks(DrawingContext dc, TickConfig ticks, double anglestart, double angleend)
        {
            int i;
            double angle, angledelta;
            Point p1, p2;
            double dynamicWidth =ticks.MyTickWidth;
            Pen pen = new Pen(Brushes.Black, dynamicWidth);

            angledelta = (angleend - anglestart) / (ticks.Count - 1) * Math.PI / 180;
            angle = anglestart * Math.PI / 180;

            for (i = 0; i < ticks.Count; i++, angle += angledelta)
            {
                p1.X = Radius * Math.Cos(angle);
                p1.Y = Radius * Math.Sin(angle);

                p2.X = (Radius - ticks.Length) * Math.Cos(angle);
                p2.Y = (Radius - ticks.Length) * Math.Sin(angle);

                dc.DrawLine(pen, Transform(p1), Transform(p2));
            }
        }

        void DrawNeedle()
        {
            var dc = needle.Open();
            Point p;

            StreamGeometry strm = new StreamGeometry();

            var dc2 = strm.Open();

            double widthbase = Needle.WidthBase;
            double widthend = Needle.WidthEnd;
            double widthmiddle = Needle.WidthMiddle;
            double lengthoutside = Needle.LengthOutside;

            double lengthinside = Needle.LengthInside * Radius - lengthoutside - Hub.Radius - (Needle.OverlapTicks ? 0 : MajorTicks.Length);

            p = new Point(Center.X + Hub.Radius, Center.Y + widthbase / 2);

            dc2.BeginFigure(p, true, true);

            p.Y = Center.Y - widthbase / 2;
            dc2.LineTo(p, false, false);

            p.X += lengthinside;
            p.Y = Center.Y - widthmiddle / 2;
            dc2.LineTo(p, false, false);

            p.X += lengthoutside;
            p.Y = Center.Y - widthend / 2;
            dc2.LineTo(p, false, false);

            p.Y = Center.Y + widthend / 2;
            dc2.LineTo(p, false, false);

            p.X -= lengthoutside;
            p.Y = Center.Y + widthmiddle / 2;
            dc2.LineTo(p, false, false);

            p.X -= lengthinside;
            p.Y = Center.Y + widthbase / 2;
            dc2.LineTo(p, false, false);

            dc2.Close();

            dc.DrawGeometry(new SolidColorBrush(Needle.Color), null, strm);

            dc.Close();
        }

        void UpdateNeedle()
        {
            double angle = (TickMarkAngleEnd - TickMarkAngleStart) / (MaxValue - MinValue) * Value + TickMarkAngleStart;

            needle.Transform = new RotateTransform(-angle, Center.X, Center.Y);
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawDrawing(statics);
            dc.DrawDrawing(needle);
        }

        public class TickConfig : INotifyPropertyChanged
        {
            private double _myTickWidth;
            public double MyTickWidth
            {
                get { return _myTickWidth; }
                set
                {
                    if (_myTickWidth != value)
                    {
                        _myTickWidth = value;
                        OnPropertyChanged(nameof(MyTickWidth));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

       
            public int Count { get; set; }
            public double Length { get; set; }
            public double Width { get; set; }


        }

        public class HubConfig
        {
            public double Radius { get; set; }

            public Color Color { get; set; }
        }

        public class NeedleConfig
        {
            public double LengthInside { get; set; }
            public bool OverlapTicks { get; set; }
            public double LengthOutside { get; set; }

            public double WidthBase { get; set; }
            public double WidthMiddle { get; set; }
            public double WidthEnd { get; set; }

            public Color Color { get; set; }            
        }
    }

    public class RotaryGaugeZoneConfig
    {
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }

        public double Radius { get; set; }
        public double Length { get; set; }

        public Color Color { get; set; }
    }
}
