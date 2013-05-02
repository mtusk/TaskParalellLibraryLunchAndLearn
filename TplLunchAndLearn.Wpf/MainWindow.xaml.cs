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

        private void ExceptionDialog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Exception = null;
        }

        #region MenuItem Handlers
        private void SynchronousBlockCodeMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            Task.Delay(TimeSpan.FromSeconds(5));
        }

        private void AsynchronousProgrammingModelMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
                    {
                        Task.Delay(TimeSpan.FromSeconds(2));
                    })
                .ContinueWith(t =>
                    {
                        throw new Exception("A sample exception");            
                    });
            

            //BeforeTpl.AsynchronousProgrammingModel();
        }
        #endregion
    }
}
