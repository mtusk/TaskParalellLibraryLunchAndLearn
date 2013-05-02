using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TplLunchAndLearn
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Properties
        private Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
            set
            {
                if (value != _exception)
                {
                    _exception = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region MenuItem Handlers
        private void SynchronousBlockCodeMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }

        private void AsynchronousProgrammingModelMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            throw new Exception("A sample exception");
            //BeforeTpl.AsynchronousProgrammingModel();
        }

        private void UnhandledUnobservedExceptionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
                {
                    throw new Exception("A sample exception");
                });
        }

        private void HandledUnobservedExceptionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(() =>
                    {
                        throw new Exception("A sample exception");
                    })
                .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            throw new Exception("Unobserved exception encountered", t.Exception);
                        }
                    }, uiContext);
        }
        #endregion

        private void ExceptionDialog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Exception = null;
        }
    }
}
