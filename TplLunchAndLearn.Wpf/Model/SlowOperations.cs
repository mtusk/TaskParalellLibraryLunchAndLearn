using System;
using System.IO;
using System.Net;

namespace TplLunchAndLearn.Model
{
    public static class SlowOperations
    {
        public static string ServerPortNumber = "38379";

        private static string BuildUrl(string portNumber, TimeSpan delay, string message)
        {
            return string.Format("http://localhost:{0}/home/SlowEcho?delay={1}&input={2}",
                portNumber,
                delay,
                Uri.EscapeDataString(message));
        }

        public static string WebClientDownloadString(string message)
        {
            var url = BuildUrl(ServerPortNumber, TimeSpan.FromSeconds(5), message);
            return new WebClient().DownloadString(url);
        }

        #region Asynchronous Programming Model (APM)
        public static void AsynchronousProgrammingModel(Action<string> callback)
        {
            var url = BuildUrl(ServerPortNumber, TimeSpan.FromSeconds(5), "APM Test");
            var request = WebRequest.Create(url) as HttpWebRequest;
            var state = new WebRequestState()
                {
                    Request = request,
                    Callback = callback
                };
            request.BeginGetResponse(new AsyncCallback(AsynchronousProgrammingModelCallback), state);
        }

        private static void AsynchronousProgrammingModelCallback(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as WebRequestState;
            var response = state.Request.EndGetResponse(asyncResult);
            var responseStream = response.GetResponseStream();
            var result = new StreamReader(responseStream).ReadToEnd();
            state.Callback(result);
        }

        private class WebRequestState
        {
            public HttpWebRequest Request { get; set; }
            public Action<string> Callback { get; set; }
        }
        #endregion
    }
}
