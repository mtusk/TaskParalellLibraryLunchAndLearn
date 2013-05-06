using System;
using System.ComponentModel;
using System.IO;
using System.Net;
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

        #region Web Helpers
        public static string ServerPortNumber = "38379";

        public static string BuildUrl(TimeSpan delay, string message)
        {
            return string.Format("http://localhost:{0}/home/SlowEcho?delay={1}&input={2}",
                ServerPortNumber,
                delay,
                Uri.EscapeDataString(message));
        }
        #endregion



        #region Before TPL
        private void SynchronousBlockCodeMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "Synchronous Test");
            var result = new WebClient().DownloadString(url);
            this.Message = result;
        }

        private void AsynchronousProgrammingModelMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "APM Test");
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.BeginGetResponse(new AsyncCallback(AsynchronousProgrammingModelCallback), request);
        }

        private void AsynchronousProgrammingModelCallback(IAsyncResult asyncResult)
        {
            var request = asyncResult.AsyncState as HttpWebRequest;
            var response = request.EndGetResponse(asyncResult);
            var responseStream = response.GetResponseStream();
            var result = new StreamReader(responseStream).ReadToEnd();
            this.Message = result;
        }

        private void EventBasedAsynchronousPatternMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "EAP Test");
            var client = new WebClient();
            client.DownloadStringCompleted += EventBasedAsynchronousPattern_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri(url));
        }

        private void EventBasedAsynchronousPattern_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            var result = e.Result;
            this.Message = result;
        }

        private void BackgroundWorkerMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += BackgroundWorkerMenuItem_DoWork;
            worker.RunWorkerCompleted += BackgroundWorkerMenuItem_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void BackgroundWorkerMenuItem_DoWork(object sender, DoWorkEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "BackgroundWorker Test");
            var result = new WebClient().DownloadString(url);
            e.Result = result;
        }

        private void BackgroundWorkerMenuItem_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result.ToString();
            this.Message = result;
        }

        private void ThreadPoolMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var action = new Action<object>(obj =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(5), "ThreadPool Test");
                    var result = new WebClient().DownloadString(url);

                    Dispatcher.BeginInvoke(new Action<string>(message =>
                        {
                            this.Message = message;
                        }), result);
                });

            ThreadPool.QueueUserWorkItem(new WaitCallback(action));
        }
        #endregion

        #region With TPL
        private void TaskRunMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskAction = new Func<string>(() =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(5), "TaskRun Test");
                    var result = new WebClient().DownloadString(url);
                    return result;
                });
            var continuation = new Action<Task<string>>(t =>
                {
                    this.Message = t.Result;
                });

            Task.Run(taskAction)
                .ContinueWith(continuation, uiScheduler);
        }

        private void TaskWaitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Factory.StartNew(() =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(5), "TaskWait Test");
                    return new WebClient().DownloadString(url);
                });

            task.Wait(); // Blocking call!!

            var result = task.Result;
            this.Message = result;
        }

        private void ContinueWhenAnyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var downloadLetter = new Func<int, string, string>((numberOfSeconds, input) =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(numberOfSeconds), input);
                    return new WebClient().DownloadString(url);
                });
            var appendLetter = new Func<Task<string>, string>(t =>
                {
                    this.Message = string.Format("{0}{1}", this.Message, t.Result);
                    return t.Result;
                });
            var continuation = new Action<Task>(t =>
                {
                    this.Message = string.Format("{0}!", this.Message);
                });

            var tasks = new Task[]
                {
                    Task.Factory.StartNew(() => downloadLetter(6, "F" )).ContinueWith(appendLetter),
                    Task.Factory.StartNew(() => downloadLetter(4, "O" )).ContinueWith(appendLetter),
                    Task.Factory.StartNew(() => downloadLetter(5, "F" )).ContinueWith(appendLetter),
                    Task.Factory.StartNew(() => downloadLetter(2, "H" )).ContinueWith(appendLetter)
                };

            Task.Factory.ContinueWhenAny(tasks, continuation);
        }

        private void ContinueWhenAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var downloadLetter = new Func<int, string, string>((numberOfSeconds, input) =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(numberOfSeconds), input);
                    return new WebClient().DownloadString(url);
                });
            var appendLetter = new Func<Task<string>, string>(t =>
                {
                    this.Message = string.Format("{0}{1}", this.Message, t.Result);
                    return t.Result;
                });
            var continuation = new Func<Task[], string>(t =>
                {
                    this.Message = string.Format("{0}!", this.Message);
                    return this.Message;
                });

            var tasks = new Task[]
                {
                    Task.Factory.StartNew(() => downloadLetter(6, "F" )).ContinueWith(appendLetter),
                    Task.Factory.StartNew(() => downloadLetter(4, "O" )).ContinueWith(appendLetter),
                    Task.Factory.StartNew(() => downloadLetter(5, "F" )).ContinueWith(appendLetter),
                    Task.Factory.StartNew(() => downloadLetter(2, "H" )).ContinueWith(appendLetter)
                };

            Task.Factory.ContinueWhenAll<string>(tasks, continuation);
        }
        #endregion

        #region async/await
        private async void AsyncAwaitExceptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    throw new Exception("Hoff-ception");
                });
            }
            catch (Exception ex)
            {
                this.Message = ex.Message;
            }
        }
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

            var exceptionAction = new Action(() =>
                {
                    throw new Exception("A sample exception");
                });
            var continuation = new Action<Task>(t =>
                {
                    if (t.Exception != null)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                            {
                                throw new Exception("Unobserved exception encountered", t.Exception);
                            }));
                    }
                });

            Task.Factory
                .StartNew(exceptionAction)
                .ContinueWith(continuation,
                    cancellationToken,
                    TaskContinuationOptions.OnlyOnFaulted,
                    uiScheduler);
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
    }
}
