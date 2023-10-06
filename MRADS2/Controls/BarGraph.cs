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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MRADS2.Controls
{
    public class BarGraph : Canvas
    {
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(BarGraph),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(BarGraph),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(BarGraph),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceValueProperty));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        static object CoerceValueProperty(DependencyObject d, object value)
        {
            var bar = (BarGraph)d;
            var v = (double)value;

            if (v < bar.Min || v > bar.Max)
                return (DependencyProperty.UnsetValue);

            return (value);
        }

        public static readonly DependencyProperty PositiveColorProperty = DependencyProperty.Register("PositiveColor", typeof(Color), typeof(BarGraph),
            new FrameworkPropertyMetadata(Colors.Green, FrameworkPropertyMetadataOptions.AffectsRender));
        public Color PositiveColor
        {
            get { return (Color)GetValue(PositiveColorProperty); }
            set { SetValue(PositiveColorProperty, value); }
        }

        public static readonly DependencyProperty NegativeColorProperty = DependencyProperty.Register("NegativeColor", typeof(Color), typeof(BarGraph),
            new FrameworkPropertyMetadata(Colors.Red, FrameworkPropertyMetadataOptions.AffectsRender));
        public Color NegativeColor
        {
            get { return (Color)GetValue(NegativeColorProperty); }
            set { SetValue(NegativeColorProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(BarGraph),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender));
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty SplitBarProperty = DependencyProperty.Register("SplitBar", typeof(bool), typeof(BarGraph),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool SplitBar
        {
            get { return (bool)GetValue(SplitBarProperty); }
            set { SetValue(SplitBarProperty, value); }
        }

        public static readonly DependencyProperty ReverseProperty = DependencyProperty.Register("Reverse", typeof(bool), typeof(BarGraph),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool Reverse
        {
            get { return (bool)GetValue(ReverseProperty); }
            set { SetValue(ReverseProperty, value); }
        }

        public static readonly DependencyProperty SegmentedProperty = DependencyProperty.Register("Segmented", typeof(bool), typeof(BarGraph),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool Segmented
        {
            get { return (bool)GetValue(SegmentedProperty); }
            set { SetValue(SegmentedProperty, value); }
        }

        public static readonly DependencyProperty SegmentCountProperty = DependencyProperty.Register("SegmentCount", typeof(int), typeof(BarGraph),
            new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender));
        public int SegmentCount
        {
            get { return (int)GetValue(SegmentCountProperty); }
            set { SetValue(SegmentCountProperty, value); }
        }

        static BarGraph()
        {
            BackgroundProperty.OverrideMetadata(typeof(BarGraph), new FrameworkPropertyMetadata(Brushes.LightGray));
        }

        protected override void OnRender(DrawingContext dc)
        {
            Pen borderpen = new Pen(Brushes.Gray, 1.0);
            Point start, end;
            int i;
            double mid = ((Max - Min) / 2 + Min);
            Brush barcolor = new SolidColorBrush(SplitBar && Value < mid ? NegativeColor : PositiveColor);

            dc.DrawRectangle(Background, borderpen, new Rect(RenderSize));

            if (!double.IsNaN(Value))
            {
                if (SplitBar)
                {
                    var pct = (Value - mid) / ((Max - Min) / 2);

                    if (Orientation == Orientation.Vertical)
                    {
                        start = new Point(ActualWidth, ActualHeight / 2);
                        end = new Point(0, start.Y - start.Y * pct);
                    }
                    else
                    {
                        start = new Point(ActualWidth / 2, ActualHeight);
                        end = new Point(start.X + start.X * pct, 0);
                    }
                }
                else
                {
                    if (Orientation == Orientation.Vertical)
                    {
                        start = new Point(ActualWidth, Reverse ? 0 : ActualHeight);
                        end = new Point(0, start.Y + (Reverse ? 1 : -1) * ActualHeight * (Value - Min) / (Max - Min));
                    }
                    else
                    {
                        start = new Point(Reverse ? 0 : ActualWidth, ActualHeight);
                        end = new Point(start.X + (Reverse ? -1 : 1) * ActualWidth * (Value - Min) / (Max - Min), 0);
                    }
                }

                if (Segmented)
                {
                    double dx, dy;
                    Pen pen = new Pen(Brushes.Black, 1);
                    Point p1, p2;
                    int segs;

                    segs = (int)Math.Round(SegmentCount * (Value - Min) / (Max - Min));

                    dx = ActualWidth / SegmentCount;
                    dy = ActualHeight / SegmentCount;

                    p1 = start;

                    for (i = 0; i < SegmentCount; i++)
                    {
                        if (Orientation == Orientation.Vertical)
                            p2 = new Point(end.X, p1.Y + dy);
                        else
                            p2 = new Point(p1.X + dx, end.Y);

                        dc.DrawRectangle(i < segs ? barcolor : Background, pen, new Rect(p1, p2));

                        if (Orientation == Orientation.Vertical)
                            p1.Y = p2.Y;
                        else
                            p1.X = p1.X;
                    }
                }
                else
                    dc.DrawRectangle(barcolor, null, new Rect(start, end));
            }

            if (SplitBar)
            {
                if (Orientation == Orientation.Vertical)
                    dc.DrawLine(borderpen, new Point(0, ActualHeight / 2), start);
                else
                    dc.DrawLine(borderpen, new Point(ActualWidth / 2, 0), start);
            }
        }
    }
}
