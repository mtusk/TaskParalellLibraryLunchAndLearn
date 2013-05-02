using System;
using System.IO;
using System.Net;

namespace TplLunchAndLearn.Examples
{
    public class BeforeTpl
    {
        #region Synchronous

        #endregion

        #region Asynchronous Programming Model (APM)
        public static void AsynchronousProgrammingModel()
        {
            var url = string.Format("http://localhost:{0}/api/values", App.ServerPortNumber);
            var request = HttpWebRequest.CreateHttp(url);
            request.BeginGetResponse(new AsyncCallback(AsynchronousProgrammingModelCallback), request);
        }

        private static void AsynchronousProgrammingModelCallback(IAsyncResult asyncResult)
        {
            var request = asyncResult.AsyncState as HttpWebRequest;
            var response = request.EndGetResponse(asyncResult);
            var responseStream = response.GetResponseStream();
            var result = new StreamReader(responseStream).ReadToEnd();
        }
        #endregion
    }
}
