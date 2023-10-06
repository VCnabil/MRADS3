using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MRADS2.Panels;

namespace MRADS2.Controls
{
    class DataBox : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DataBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));
        public object Value
        {
            get { return GetValue(ValueProperty); }

            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty FloatFormatProperty = DependencyProperty.Register("FloatFormat", typeof(string), typeof(DataBox),
            new FrameworkPropertyMetadata("0.000", FrameworkPropertyMetadataOptions.AffectsRender));
        public string FloatFormat
        {
            get { return (string)GetValue(FloatFormatProperty); }

            set { SetValue(FloatFormatProperty, value); }
        }

        public static readonly DependencyProperty InvertBoolProperty = DependencyProperty.Register("InvertBool", typeof(bool), typeof(DataBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool InvertBool
        {
            get { return (bool)GetValue(InvertBoolProperty); }

            set { SetValue(InvertBoolProperty, value); }
        }

        bool drawbool = false;
        bool registeredforplotchange = false;

        DpiScale dpi;

        VariableVM[] variables = null;
        MRADSDataVM datavm = null;

        Typeface typeface = null;

        static DataBox()
        {
            PaddingProperty.OverrideMetadata(typeof(DataBox), new FrameworkPropertyMetadata(new Thickness(5, 5, 5, 5)));
        }

        public DataBox()
        {
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            Initialized += DataBox_Initialized;
            MouseDown += DataBox_MouseDown;
            ContextMenuOpening += DataBox_ContextMenuOpening;
        }

        private void DataBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (datavm == null)
                e.Handled = true;

            if (variables == null)
            {
                e.Handled = true;

                return;
            }

            UpdateContextMenu(!variables.All(a => a.PlotState.Plotting));
        }

        private void DataBox_Initialized(object sender, EventArgs e)
        {
            var exp = GetBindingExpression(ValueProperty);

            var bindvar = exp.ResolvedSource;

            if (bindvar is not VariableVMProvider)
                return;

            variables = ((VariableVMProvider)bindvar).Variables;

            drawbool = (bindvar is BindVariable || bindvar is BindVariable<bool>) && variables.All(a => a.Definition.Type == MRADSVariableDefinition.VarType.Bool);

            var bp = GetParent<BasePanel>();

            if (bp != null)
                datavm = bp.VMData;

            if (datavm != null && variables != null)
            {
                if (!registeredforplotchange)
                {
                    foreach (var v in variables)
                        datavm.RegisterPlotChangeCB(v, (_, _) => InvalidateVisual());
                }

                registeredforplotchange = true;

                UpdateContextMenu(true);
            }
        }

        public void SetDataVM(MRADSDataVM datavm)
        {
            this.datavm = datavm;

            if (datavm != null && variables != null)
            {
                if (!registeredforplotchange)
                {
                    foreach (var v in variables)
                        datavm.RegisterPlotChangeCB(v, (_, _) => InvalidateVisual());
                }

                registeredforplotchange = true;

                UpdateContextMenu(true);
            }
        }

        private void DataBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (datavm == null || variables == null || e.ClickCount < 2 || e.ChangedButton != System.Windows.Input.MouseButton.Left)
                return;

            if (!variables.All(a => a.PlotState.Plotting))
                datavm.AddPlots(variables);
            else
            {
                foreach (var v in variables)
                    datavm.RemovePlot(v);
            }

            PlotUpdate();
        }

        T GetParent<T>()
        {
            FrameworkElement el = this;

            while (el.Parent != null)
            {
                if (el.Parent is T)
                    return ((T)(dynamic)el.Parent);

                if (!(el.Parent is FrameworkElement))
                    break;

                el = (FrameworkElement)el.Parent;
            }

            return (default(T));
        }

        void PlotUpdate()
        {
            InvalidateVisual();
        }

        void UpdateContextMenu(bool addoption)
        {
            ContextMenu cm;
            MenuItem mi;

            cm = new ContextMenu();

            ContextMenu = cm;

            if (!addoption)
            {
                mi = new MenuItem() { Header = "Remove Plot" };
                mi.Click += Mi_RemovePlot;
                cm.Items.Add(mi);

                return;
            }

            mi = new MenuItem() { Header = "Add to New Chart" };
            mi.Click += Mi_AddPlot;
            cm.Items.Add(mi);

            HashSet<string> charts = new HashSet<string>();

            foreach (var v in variables)
                charts = charts.Concat(datavm.CompatibleCharts(v)).ToHashSet();

            if (charts.Count() == 0)
                return;

            mi = new MenuItem() { Header = "Add to Existing Chart" };

            foreach (var s in charts)
            {
                MenuItem mi2 = new MenuItem() { Header = s };
                mi2.Click += Mi2_AddPlotToChart;
                mi.Items.Add(mi2);
            }

            cm.Items.Add(mi);
        }

        private void Mi2_AddPlotToChart(object sender, RoutedEventArgs e)
        {
            datavm.AddPlots(variables, (string)((MenuItem)sender).Header);
        }

        private void Mi_AddPlot(object sender, RoutedEventArgs e)
        {
            datavm.AddPlots(variables);

            PlotUpdate();
        }

        private void Mi_RemovePlot(object sender, RoutedEventArgs e)
        {
            foreach (var v in variables)
                datavm.RemovePlot(v);

            PlotUpdate();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            dpi = VisualTreeHelper.GetDpi(this);
            typeface = new Typeface(FontFamily.Source);

            Size ret;

            ret.Height = FontFamily.LineSpacing * FontSize + Padding.Top + Padding.Bottom + 2;
            ret.Width = MinWidth;

            return (ret);
        }

        protected override void OnRender(DrawingContext dc)
        {
            string text = null;
            Pen pen;

            if (variables != null && variables.Any(a => a.PlotState.Plotting))
                pen = new Pen(new SolidColorBrush(datavm.GetPlotColor(variables.First(a => a.PlotState.Plotting))), 3);
            else
                pen = new Pen(Foreground, 1);

            if (drawbool)
            {
                DrawBool(dc, pen, Value);

                return;
            }

            if (Value is int)
                text = ((int)Value).ToString();
            else if (Value is double)
                text = ((double)Value).ToString(FloatFormat);
            else if (Value is string)
                text = (string)Value;

            FormattedText t = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, dpi.PixelsPerDip);

            dc.DrawRectangle(Brushes.Transparent, pen, new Rect(0, 0, ActualWidth, ActualHeight));

            t.TextAlignment = TextAlignment.Center;

            dc.DrawText(t, new Point(ActualWidth / 2, (ActualHeight - t.Height) / 2));
        }

        void DrawBool(DrawingContext dc, Pen pen, object value)
        {
            dc.DrawEllipse(Brushes.Transparent, pen, new Point(ActualWidth / 2, ActualHeight / 2), ActualHeight / 4, ActualHeight / 4);

            if (!variables.All(a => a.Set))
                dc.DrawLine(new Pen(Brushes.Black, 1), new Point(ActualWidth / 2 - ActualHeight / 8, ActualHeight / 2), new Point(ActualWidth / 2 + ActualHeight / 8, ActualHeight / 2));
            else if (value is bool && (((bool)value && !InvertBool) || (!(bool)value && InvertBool)))
                dc.DrawEllipse(Foreground, null, new Point(ActualWidth / 2, ActualHeight / 2), ActualHeight / 4 - 2, ActualHeight / 4 - 2);
        }
    }
}
