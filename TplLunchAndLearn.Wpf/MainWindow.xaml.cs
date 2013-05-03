using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TplLunchAndLearn.Model;

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
                    RaisePropertyChanged("Exception");
                }
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    RaisePropertyChanged("Message");
                }
            }
        }
        #endregion

        #region Event Handlers
        private void MessageDialog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Message = null;
        }

        private void ExceptionDialog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Exception = null;
        }
        #endregion

        #region Before TPL
        private void SynchronousBlockCodeMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }

        private void AsynchronousProgrammingModelMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            SlowOperations.AsynchronousProgrammingModel(result =>
                {
                    this.Message = result;
                });
        }
        #endregion

        #region With TPL
        private void TaskRunMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Run(() =>
                    {
                        return SlowOperations.WebClientDownloadString("TaskRun Test");
                    })
                .ContinueWith(t =>
                    {
                        this.Message = t.Result;
                    },
                uiScheduler);
        }

        private void TaskWaitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = new Task<string>(() =>
                {
                    return SlowOperations.WebClientDownloadString("TaskWait Test");
                });
            task.Start();
            task.Wait(); // Blocking call
            var result = task.Result;
            this.Message = result;
        }
        #endregion

        #region async/await
        #endregion

        #region Exceptions
        private void UnhandledUnobservedExceptionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                throw new Exception("A sample exception");
            });
        }

        private void OnlyOnFaultedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var cancellationToken = new CancellationToken();
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory
                .StartNew(() =>
                    {
                        throw new Exception("A sample exception");
                    })
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            throw new Exception("Unobserved exception encountered", t.Exception);
                        }));
                    }
                }, cancellationToken, TaskContinuationOptions.OnlyOnFaulted, uiScheduler);
        }

        private void UnobservedTaskExceptionEventMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //Task.Factory
            //    .StartNew(() =>
            //        {
            //            throw new Exception("A sample exception");
            //        })
            //    .ContinueWith(t =>
            //        {
            //            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            //        });
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
