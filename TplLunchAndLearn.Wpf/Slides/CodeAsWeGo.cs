using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

/// <summary>
/// Just to hold the code that we will write during the presentation
/// </summary>
public class CodeAsWeGo
{
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
        var task = Task.Run(() =>
        {
            var url = BuildUrl(TimeSpan.FromSeconds(5), "TaskWait Test");
            return new WebClient().DownloadString(url);
        });

        task.Wait(); // Blocking call!!

        var result = task.Result;
        this.Message = result;
    }

    private void StronglyTypedInformationPassingMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        Task.Run(() => new List<char>() { 'H', 'O', 'F', 'F' })
            .ContinueWith(t => string.Join(string.Empty, t.Result))
            .ContinueWith(t => this.Message = t.Result, uiScheduler);
    }
}