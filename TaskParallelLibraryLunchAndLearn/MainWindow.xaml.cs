using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TaskParallelLibraryLunchAndLearn
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Example 1
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
                {
                    Thread.Sleep(2000);
                });
        }
        #endregion
    }
}
