using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MRADS2.Controls;

namespace MRADS2
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : ContentWindow, INotifyPropertyChanged
    {
        public string Status { get; private set; }

        Task task;
        bool cancel = false;

        public double Progress { get; private set; }

        public enum Results { None, Success, Cancelled, Error };
        public Results Result = Results.None;

        public string ErrorMessage = null;

        public Action<object> ProgressCallback = null;

        SynchronizationContext sctx;

        public ProgressWindow(Action action)
        {
            InitializeComponent();

            DataContext = this;

            sctx = SynchronizationContext.Current;

            task = Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    Result = Results.Error;

                    sctx.Post(_ => Close(), null);
                }                
            });
        }

        public void UpdateStatus(string status, double progress, bool done = false)
        {
            sctx.Post(_ =>
            {
                Status = status;
                Progress = progress;

                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Progress));

                if (done)
                {
                    task.Wait();

                    if (cancel)
                        Result = Results.Cancelled;
                    else
                        Result = Results.Success;

                    Close();
                }
            }, null);
        }

        public bool Cancelled()
        {
            return (Dispatcher.Invoke(() => cancel));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancel = true;
        }
    }
}
