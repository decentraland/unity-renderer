using DCL;
using Sentry;
using UnityEngine;
using UnityEngine.Networking;

namespace MainScripts.DCL.Helpers.SentryUtils
{
    public class SentryWebRequestMonitor : IWebRequestMonitor
    {
        private const int DATA_LIMIT = 150;

        public DisposableTransaction TrackWebRequest(IWebRequestAsyncOperation webRequestOp, string endPointTemplate, string queryString = null,
            string data = null, bool finishTransactionOnWebRequestFinish = false)
        {
            var webRequest = webRequestOp.webRequest;
            var transaction = SentrySdk.StartTransaction(endPointTemplate, webRequest.method);

            SentrySdk.ConfigureScope(s =>
            {
                s.Transaction = transaction;

                s.Request = new Request
                {
                    Method = webRequest.method,
                    Url = webRequest.url,
                    QueryString = queryString,
                    Data = GetData(webRequest, data)
                };
            });

            void WebRequestOpCompleted(IWebRequestAsyncOperation webRequestAsyncOperation)
            {
                // if we don't set the span status manually it will be inferred from the errors happening while the transaction
                // is active: it's not correct

                // Due to possible repetitions webRequestAsyncOperation.webRequest can represent the web request that was created with the last attempt
                SetSpanStatus(transaction, webRequestAsyncOperation.webRequest);
                if (finishTransactionOnWebRequestFinish)
                    transaction.Finish();
            }

            webRequestOp.completed += WebRequestOpCompleted;
            return new DisposableTransaction(transaction);
        }

        private static void SetSpanStatus(ITransaction transaction, UnityWebRequest webRequest)
        {
            if (webRequest.WebRequestTimedOut())
                transaction.Status = SpanStatus.DeadlineExceeded;
            else
            {
                transaction.Status = webRequest.responseCode switch
                                     {
                                         200 => SpanStatus.Ok,
                                         404 => SpanStatus.NotFound,
                                         400 => SpanStatus.InvalidArgument,
                                         403 => SpanStatus.PermissionDenied,
                                         409 => SpanStatus.Aborted,
                                         500 => SpanStatus.InternalError,
                                         503 => SpanStatus.Unavailable,
                                         401 => SpanStatus.Unauthenticated,
                                         429 => SpanStatus.ResourceExhausted,
                                         504 => SpanStatus.DeadlineExceeded,
                                         _ => SpanStatus.UnknownError
                                     };
            }
        }

        private static object GetData(UnityWebRequest webRequest, string data)
        {
            if (data != null)
                return data.Length <= DATA_LIMIT ? data : data.Substring(0, DATA_LIMIT);

            return webRequest.uploadHandler?.data;
        }

        public void Dispose() { }

        public void Initialize() { }
    }
}
