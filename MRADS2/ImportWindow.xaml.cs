using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Microsoft.Win32;

namespace MRADS2
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window, INotifyPropertyChanged
    {
        public string OutputFileName
        {
            get => outputfilename;
            set
            {
                outputfilename = value;

                OnPropertyChanged();
            }
        }
        string outputfilename;

        public string OutputFileNameFull
        {
            get
            {
                if (Path.GetDirectoryName(outputfilename).Length == 0)
                    return (Path.Combine(MRADSSettings.AppData, outputfilename));
                else
                    return (outputfilename);
            }
        }

        public bool OnlyCaptureSelectedEvents
        {
            get => onlycaptureselectedevents;

            set
            {
                onlycaptureselectedevents = value;

                chkCapAll.IsEnabled = onlycaptureselectedevents;

                UpdateCheckBoxEnables();

                OnPropertyChanged();
            }
        }
        bool onlycaptureselectedevents = false;

        public DateTime StartTime
        {
            get => starttime;
            set
            {
                starttime = new DateTime(starttime.Year, starttime.Month, starttime.Day, value.Hour, value.Minute, value.Second);

                OnPropertyChanged();
            }
        }

        public DateTime EndTime
        {
            get => endtime;
            set
            {
                endtime = new DateTime(endtime.Year, endtime.Month, endtime.Day, value.Hour, value.Minute, value.Second);

                OnPropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => starttime;
            set
            {
                starttime = new DateTime(value.Year, value.Month, value.Day, starttime.Hour, starttime.Minute, starttime.Second);

                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => endtime;
            set
            {
                endtime = new DateTime(value.Year, value.Month, value.Day, endtime.Hour, endtime.Minute, endtime.Second);

                OnPropertyChanged();
            }
        }

        public DateTime Start => TimeZoneInfo.ConvertTimeToUtc(starttime, curtz);
        public DateTime End => TimeZoneInfo.ConvertTimeToUtc(endtime, curtz).AddSeconds(1);

        DateTime starttime, endtime;

        public enum FunctionMode { Import, Export, ExportCSV };

        public FunctionMode Mode
        {
            get => mode;
            set
            {
                mode = value;

                InitMode(mode);
            }
        }

        FunctionMode mode = FunctionMode.Import;

        TimeZoneInfo curtz;

        ImportSource source;

        public ImportWindow(ImportSource source)
        {
            InitializeComponent();

            this.source = source;

            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                cbTimeZone.Items.Add(tz);

            starttime = source.Start;
            endtime = source.End;

            cbTimeZone.SelectedItem = TimeZoneInfo.Local;

            DataContext = this;

            Loaded += ImportWindow_Loaded;
        }

        void InitMode(FunctionMode mode)
        {
            switch (mode)
            {
                case FunctionMode.Export:
                    btnImport.Content = "Export";
                    Title = "MRADS Data Export";

                    break;
                case FunctionMode.ExportCSV:
                    btnImport.Content = "Export";
                    Title = "MRADS CSV Data Export";

                    break;
                case FunctionMode.Import:
                    btnImport.Content = "Import";
                    Title = "MRADS Data Import";

                    break;
            }
        }

        private void ImportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeTimeZone(TimeZoneInfo.Local);

            UpdateTimes(starttime, endtime);
        }

        void UpdateTimes(DateTime start, DateTime end)
        {
            starttime = start;
            endtime = end;

            OnPropertyChanged(nameof(StartTime));
            OnPropertyChanged(nameof(EndTime));
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "MRADSDB Files (*.mradsdb)|*.mradsdb|All Files (*.*)|*.*";
            sfd.FileName = OutputFileName;
            sfd.AddExtension = true;
            sfd.InitialDirectory = MRADSSettings.AppData;

            if (sfd.ShowDialog() == false)
                return;

            OutputFileName = sfd.FileName;
        }

        private void chkCapAll_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCheckBoxEnables();
        }

        private void cbTimeZone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tz = (TimeZoneInfo)cbTimeZone.SelectedItem;

            ChangeTimeZone(tz);
        }

        void ChangeTimeZone(TimeZoneInfo newtz)
        {
            if (curtz == null)
                curtz = TimeZoneInfo.Utc;

            UpdateTimes(TimeZoneInfo.ConvertTime(starttime, curtz, newtz), TimeZoneInfo.ConvertTime(endtime, curtz, newtz));

            dateStart.DisplayDateStart = dateEnd.DisplayDateStart = TimeZoneInfo.ConvertTimeFromUtc(source.Start, newtz);
            dateStart.DisplayDateEnd = dateEnd.DisplayDateEnd = TimeZoneInfo.ConvertTimeFromUtc(source.End, newtz);

            curtz = newtz;
        }

        void UpdateCheckBoxEnables()
        {
            bool enabled = (chkCapAll.IsChecked == false) && OnlyCaptureSelectedEvents;

            chkCapFaults.IsEnabled = enabled;
            chkCapWarnings.IsEnabled = enabled;
            chkCapOnlineOffline.IsEnabled = enabled;
            chkCapStatus.IsEnabled = enabled;
        }
    }
}
