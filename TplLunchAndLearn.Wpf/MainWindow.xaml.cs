using System;
using System.Collections.Generic;
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



        //--------------------------------------------------



        #region Before TPL
        private void SynchronousBlockCodeMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "Synchronous Test");
            this.Message = new WebClient().DownloadString(url);
        }

        private void AsynchronousProgrammingModelMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "APM Test");
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.BeginGetResponse(
                new AsyncCallback(
                    AsynchronousProgrammingModelCallback),
                    request);
        }

        private void AsynchronousProgrammingModelCallback(IAsyncResult asyncResult)
        {
            var request = asyncResult.AsyncState as HttpWebRequest;
            var response = request.EndGetResponse(asyncResult);
            var responseStream = response.GetResponseStream();
            this.Message = new StreamReader(responseStream).ReadToEnd();
        }

        private void EventBasedAsynchronousPatternMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "EAP Test");
            var client = new WebClient();
            client.DownloadStringCompleted += (sender1, e1) =>
                {
                    this.Message = e1.Result;
                };
            client.DownloadStringAsync(new Uri(url));
        }

        private void BackgroundWorkerMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (sender1, e1) =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(5), "BackgroundWorker Test");
                    var result = new WebClient().DownloadString(url);
                    e1.Result = result;
                };
            worker.RunWorkerCompleted += (sender1, e1) =>
                {
                    this.Message = e1.Result.ToString();
                };
            worker.RunWorkerAsync();
        }
        
        private void ThreadPoolMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(obj =>
            {
                var url = BuildUrl(TimeSpan.FromSeconds(5), "ThreadPool Test");
                var result = new WebClient().DownloadString(url);

                Dispatcher.BeginInvoke(new Action<string>(message =>
                {
                    this.Message = message;
                }), result);
            }));
        }
        #endregion

        #region Task Parallelism
        private void TaskRunMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // StartNew is the .NET 4 way of starting a new task.
            // It is essentially the same thing as Task.Run

            //Task.Factory.StartNew(() => { });

            Task.Run(() =>
                    {
                        var url = BuildUrl(TimeSpan.FromSeconds(3), "TaskRun Test");
                        return new WebClient().DownloadString(url);
                    })
                .ContinueWith(t =>
                    {
                        this.Message = t.Result;
                    });
        }

        private void TaskWaitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var task = new Task<string>(() =>
                {
                    var url = BuildUrl(TimeSpan.FromSeconds(3), "asdf");
                    return new WebClient().DownloadString(url);
                });
            task.Start();
            task.Wait();
            this.Message = task.Result;
        }

        private void StronglyTypedInformationPassingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Run(() =>
                    {
                        return new List<char>() { 'H', 'O', 'F', 'F' };
                    })
                .ContinueWith(t =>
                    {
                        return string.Join(string.Empty, t.Result);
                    })
                .ContinueWith(t =>
                    {
                        return this.Message = t.Result;
                    }, uiScheduler);
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
                    this.Message += "!";
                });

            var tasks = new Task[]
                {
                    Task.Run(() => downloadLetter(6, "F")).ContinueWith(appendLetter),
                    Task.Run(() => downloadLetter(4, "O")).ContinueWith(appendLetter),
                    Task.Run(() => downloadLetter(5, "F")).ContinueWith(appendLetter),
                    Task.Run(() => downloadLetter(2, "H")).ContinueWith(appendLetter)
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
                    return this.Message += "!";
                });

            var tasks = new Task[]
                {
                    Task.Run(() => downloadLetter(6, "F")).ContinueWith(appendLetter),
                    Task.Run(() => downloadLetter(4, "O")).ContinueWith(appendLetter),
                    Task.Run(() => downloadLetter(5, "F")).ContinueWith(appendLetter),
                    Task.Run(() => downloadLetter(2, "H")).ContinueWith(appendLetter)
                };

            Task.Factory.ContinueWhenAll<string>(tasks, continuation);
        }
        #endregion

        #region async/await
        private async void AsyncAwaitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "async/await Test");
            this.Message = await new WebClient().DownloadStringTaskAsync(url);
        }

        private async void AsyncAwaitTaskDelayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            this.Message = "Delayed two seconds";
        }
        #endregion

        #region Exceptions
        private void UnhandledUnobservedExceptionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
                {
                    throw new Exception("A sample exception");
                });
        }

        private void TaskTryCatchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    throw new Exception("A sample exception");
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        throw new Exception("Unobserved exception encountered", ex);
                    }));
                }
            });
        }

        private void TaskExceptionCheckMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
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
                    });
        }

        private void OnlyOnFaultedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
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
                    }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async void AsyncAwaitExceptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                    {
                        throw new Exception("A sample exception");
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Unobserved exception encountered", ex);
            }
        }

        private void TaskWaitTryCatchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var exceptionalTask = new Task(() =>
                    {
                        throw new Exception("A sample exception");
                    });

                exceptionalTask.Start();

                try
                {
                    // blocking call, allows exceptions to be caught using try/catch
                    exceptionalTask.Wait();
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        throw new Exception("Exception encountered", ex);
                    }));
                }
            });
        }
        #endregion
    }
}
