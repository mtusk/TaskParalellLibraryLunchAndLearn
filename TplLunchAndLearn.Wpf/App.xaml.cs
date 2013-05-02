using System.Windows;
using System.Windows.Threading;

namespace TplLunchAndLearn
{
    public partial class App : Application
    {
        public static string ServerPortNumber = "21958";

        // Not good practice... for demo purposes only
        protected override void OnStartup(StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledExceptionHandler;

            base.OnStartup(e);
        }

        // Not good practice... for demo purposes only
        private void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var window = Application.Current.MainWindow as MainWindow;
            window.Exception = e.Exception;
        }
    }
}
