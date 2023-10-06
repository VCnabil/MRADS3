using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using Microsoft.Win32;
using MRADS2.Ships;
using MRADS2.Ships.StandardShip;

namespace MRADS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        MRADSData data = null;
        MRADSDataVM datavm = null;

        public bool FileOpen => data != null;

        public object PlayStopButtonContent
        {
            get
            {
                if (timer.IsEnabled)
                    return (Resources["StopButtonContent"]);
                else
                    return (Resources["PlayButtonContent"]);
            }
        }

        public bool ShowVoltagesAndCurrents => datavm != null && datavm.ScalingMode;

        public bool ShowVoltagesAndCurrentsEnabled => datavm != null;

        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            // Create the default application data directory if it doesn't exist
            if (!Directory.Exists(MRADSSettings.AppData))
            {
                try
                {
                    Directory.CreateDirectory(MRADSSettings.AppData);
                }
                catch
                {
                }
            }
            
            // Update timer is set to a 10 ms tick rate
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
        }

        /// <summary>
        /// Close the currently opened file (if there is one)
        /// </summary>
        void CloseFile()
        {
            if (data == null)
                return;

            PlayEnable(false);

            foreach (var ti in tabPanels.Items)
            {
                // Removes some WPF binding errors
                ((TabItem)ti).Template = null;

                // Cleans up stale bindings that could cause a memory leak
                ((Panels.BasePanel)((TabItem)ti).Content).Close();
            }

            tabPanels.Items.Clear();

            DataContext = null;

            datavm.Close();
            datavm = null;

            data.Source.Close();
            data = null;

            Title = "MRADS";

            OnPropertyChanged(nameof(FileOpen));
            OnPropertyChanged(nameof(ShowVoltagesAndCurrentsEnabled));
            OnPropertyChanged(nameof(ShowVoltagesAndCurrents));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseFile();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "MRADSDB Files (*.mradsdb)|*.mradsdb|All Files (*.*)|*.*";
            ofd.InitialDirectory = MRADSSettings.AppData;

            if (ofd.ShowDialog() == false)
                return;

            OpenFile(ofd.FileName);
        }

        void OpenFile(string filename)
        {
            MRADSDBSource source = null;

            source = new MRADSDBSource();

            try
            {
                source.OpenFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open file\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                source?.Close();

                return;
            }

            // Attempt to identify the data's ship
            var ship = MRADSShip.GetShip(source);
            if (ship == null)
            {
                MessageBox.Show("Unrecognized ship", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                source.Close();

                return;
            }

            CloseFile();

            source.SetShip(ship);

            data = new MRADSData(source);

            data.SetShip(ship);
            datavm = new MRADSDataVM(data, Charts);

            ship.SetShipViewModel(datavm);

            DataContext = datavm;

            // Add the panel tabs for this ship
            foreach (var panel in ship.GetPanels(datavm))
                tabPanels.Items.Add(new TabItem() { Header = panel.HeaderText, Content = panel });

            Title = "MRADS - " + filename;

            OnPropertyChanged(nameof(FileOpen));
            OnPropertyChanged(nameof(ShowVoltagesAndCurrentsEnabled));
            OnPropertyChanged(nameof(ShowVoltagesAndCurrents));
        }

        private void ImportCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == false)
                return;

            ImportFile(ofd.FileName, new MRADSRawCSV());
        }

        private void ImportSD_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "KMF Files (*.kmf)|*.kmf|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == false)
                return;

            ImportFile(ofd.FileName, new MRADSKMFSource());
        }

        private void ImportReach_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "REC Files (*.rec)|*.rec|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == false)
                return;

            ImportFile(ofd.FileName, new MRADSReachSource());
        }

        void ImportFile(string filename, ImportSource source)
        {
            try
            {
                source.OpenFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open file\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                source.Close();

                return;
            }

            var ship = MRADSShip.GetShip(source);
            if (ship == null)
            {
                MessageBox.Show("Unrecognized ship", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                source.Close();

                return;
            }

            ImportWindow import = new ImportWindow(source);
            import.Owner = this;

            import.OutputFileName = System.IO.Path.GetFileNameWithoutExtension(filename) + ".mradsdb";

            if (import.ShowDialog() == false)
            {
                source.Close();

                return;
            }

            var importer = new MRADSDataImporter(source, ship);

            importer.StartTime = import.Start;
            importer.StopTime = import.End;

            // Create a progress window to display while the data is imported
            var progress = new ProgressWindow(() => importer.WriteFile(import.OutputFileNameFull));
            progress.Owner = this;
            progress.Title = "Importing Data";

            importer.ProgressCallback = (s, p) => ProgressCallback(true, progress, s, p);
            importer.CancelCallback = progress.Cancelled;

            progress.ShowDialog();

            source.Close();

            if (progress.Result == ProgressWindow.Results.Error)
            {
                MessageBox.Show("Failed to write file\n\n" + progress.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }
            else if (progress.Result == ProgressWindow.Results.Cancelled)
                return;

            OpenFile(import.OutputFileNameFull);
        }

        void ProgressCallback(bool import, ProgressWindow progresswin, MRADSDataImporter.ImportStatus status, double progress)
        {
            string statusstr;

            if (status == MRADSDataImporter.ImportStatus.Working)
                statusstr = import ? "Importing" : "Exporting";
            else
                statusstr = status.ToString();

            progresswin.UpdateStatus(statusstr, progress * 100, status == MRADSDataImporter.ImportStatus.Done);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var ev = (MRADSEvent)((ListBoxItem)sender).DataContext;

            // Move to the time indicated by the clicked-on event
            datavm.SetNextTime(ev.Time);

            if (ev.Variables.Length > 0)
            {
                string chartname = null;

                datavm.RemoveAllPlots();

                // Plot the variables associated with the event
                foreach (var v in ev.Variables)
                    chartname = datavm.AddPlot(datavm.GetVariable(v.Provider, v.Variable), chartname, false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ExportSelection_Click(object sender, RoutedEventArgs e)
        {
            var source = (MRADSDBSource)data.Source;

            // Called ImportWindow but it's actually being used to select a section of data to export
            ImportWindow import = new ImportWindow(source);
            import.Owner = this;

            import.OutputFileName = System.IO.Path.GetFileNameWithoutExtension(source.Filename) + " selection.mradsdb";
            import.Mode = ImportWindow.FunctionMode.Export;

            if (import.ShowDialog() == false)
                return;

            PlayEnable(false);

            // Data is "imported" from the currently loaded data file starting and stopping at the specified times
            var importer = new MRADSDataImporter(source, data.Ship);

            importer.StartTime = import.Start;
            importer.StopTime = import.End;

            var progress = new ProgressWindow(() => importer.WriteFile(import.OutputFileName));
            progress.Owner = this;
            progress.Title = "Exporting Data";

            importer.ProgressCallback = (s, p) => ProgressCallback(false, progress, s, p);
            importer.CancelCallback = progress.Cancelled;

            progress.ShowDialog();
        }

        private void ExportCAN_Click(object sender, RoutedEventArgs e)
        {
            var source = (MRADSDBSource)data.Source;

            ImportWindow import = new ImportWindow(source);
            import.Owner = this;

            import.OutputFileName = System.IO.Path.GetFileNameWithoutExtension(source.Filename) + ".csv";
            import.Mode = ImportWindow.FunctionMode.ExportCSV;

            if (import.ShowDialog() == false)
                return;

            PlayEnable(false);

            var importer = new MRADSDataImporter(source, data.Ship);

            importer.StartTime = import.Start;
            importer.StopTime = import.End;

            // Write the "imported" CAN messages to a CSV file
            var progress = new ProgressWindow(() => importer.WriteFileCANCSV(import.OutputFileName));
            progress.Owner = this;
            progress.Title = "Exporting Data";

            importer.ProgressCallback = (s, p) => ProgressCallback(false, progress, s, p);
            importer.CancelCallback = progress.Cancelled;

            progress.ShowDialog();
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            var source = (MRADSDBSource)data.Source;

            ImportWindow import = new ImportWindow(source);
            import.Owner = this;

            import.OutputFileName = System.IO.Path.GetFileNameWithoutExtension(source.Filename) + ".csv";
            import.Mode = ImportWindow.FunctionMode.ExportCSV;

            if (import.ShowDialog() == false)
                return;

            PlayEnable(false);

            var importer = new MRADSDataImporter(source, data.Ship);

            importer.StartTime = import.Start;
            importer.StopTime = import.End;

            var progress = new ProgressWindow(() => importer.WriteFileCSV(import.OutputFileName, data.Source));
            progress.Owner = this;
            progress.Title = "Exporting Data";

            importer.ProgressCallback = (s, p) => ProgressCallback(false, progress, s, p);
            importer.CancelCallback = progress.Cancelled;

            progress.ShowDialog();
        }

        DateTime lasttime;

        private void PlayStop_Click(object sender, RoutedEventArgs e)
        {
            PlayEnable(!timer.IsEnabled);
        }

        void PlayEnable(bool enable)
        {
            if (!enable)
            {
                timer.Stop();

                if (datavm != null)
                    datavm.SetCursorEnable(true);
            }
            else
            {
                lasttime = DateTime.Now;

                // Disable the cursor while in "playback" mode
                datavm.SetCursorEnable(false);

                timer.Start();
            }

            OnPropertyChanged(nameof(PlayStopButtonContent));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (datavm == null || datavm.Time == data.Source.End)
            {
                PlayEnable(false);

                return;
            }

            var now = DateTime.Now;

            // If data view model isn't updating move forward in time
            if (!datavm.Updating)
                datavm.SetNextTime(datavm.Time + (now - lasttime));

            lasttime = now;
        }

        private void ApplyScaling_Click(object sender, RoutedEventArgs e)
        {
            if (datavm != null)
            {
                datavm.ScalingMode = !datavm.ScalingMode;

                OnPropertyChanged(nameof(ShowVoltagesAndCurrents));
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutwindow = new AboutWindow();

            aboutwindow.Owner = this;

            aboutwindow.ShowDialog();
        }
    }

    /// <summary>
    /// A simple converter used to convert a Color to a SolidColorBrush
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Brushes.Black; // Default color

            System.Drawing.Color color = (System.Drawing.Color)value;

            return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
