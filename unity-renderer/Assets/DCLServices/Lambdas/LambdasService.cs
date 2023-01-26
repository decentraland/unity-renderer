using Cysharp.Threading.Tasks;
using DCL;
using MainScripts.DCL.Helpers.SentryUtils;
using Sentry;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Pool;
using Transaction = Sentry.Transaction;

namespace DCLServices.Lambdas
{
    public class LambdasService : ILambdasService
    {
        private ICatalyst catalyst;

        private Service<IWebRequestController> webRequestController;
        private Service<IWebRequestMonitor> urlTransactionMonitor;

        public UniTask<(TResponse response, bool success)> Post<TResponse, TBody>(
            string endPointTemplate,
            string endPoint,
            TBody postData,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var postDataJson = JsonUtility.ToJson(postData);
            var url = GetUrl(endPoint, urlEncodedParams);
            var wr = webRequestController.Ref.Post(url, postDataJson, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false);
            var transaction = urlTransactionMonitor.Ref.TrackWebRequest(wr.asyncOp, endPointTemplate, data: postDataJson, finishTransactionOnWebRequestFinish: false);

            return SendRequestAsync<TResponse>(wr, cancellationToken, endPoint, transaction, urlEncodedParams);
        }

        public UniTask<(TResponse response, bool success)> Get<TResponse>(
            string endPointTemplate,
            string endPoint,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var url = GetUrl(endPoint, urlEncodedParams);
            var wr = webRequestController.Ref.Get(url, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false);
            var transaction = urlTransactionMonitor.Ref.TrackWebRequest(wr.asyncOp, endPointTemplate, finishTransactionOnWebRequestFinish: false);

            return SendRequestAsync<TResponse>(wr, cancellationToken, endPoint, transaction, urlEncodedParams);
        }

        private async UniTask<(TResponse response, bool success)> SendRequestAsync<TResponse>(
            IWebRequestAsyncOperation webRequestAsyncOperation,
            CancellationToken cancellationToken,
            string endPoint,
            DisposableTransaction transaction,
            (string paramName, string paramValue)[] urlEncodedParams)
        {
            using var disposable = transaction;
            await webRequestAsyncOperation.WithCancellation(cancellationToken);

            if (!webRequestAsyncOperation.isSucceeded)
                return (default, false);

            string textResponse = webRequestAsyncOperation.webRequest.downloadHandler.text;
            webRequestAsyncOperation.Dispose();

            var res = !TryParseResponse(endPoint, transaction, textResponse, out TResponse response) ? (default, false) : (response, true);
            return res;
        }

        internal string GetUrl(string endPoint, params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var urlBuilder = GenericPool<StringBuilder>.Get();
            urlBuilder.Clear();
            urlBuilder.Append(GetLambdasUrl());
            urlBuilder.Append('/');

            var endPointSpan = endPoint.AsSpan();

            if (endPoint.StartsWith('/') || endPoint.StartsWith('\\'))
                endPointSpan = endPointSpan[1..];

            if (endPoint.EndsWith('/') || endPoint.EndsWith('\\') || endPoint.EndsWith('?'))
                endPointSpan = endPointSpan[..^1];

            urlBuilder.Append(endPointSpan);

            if (urlEncodedParams.Length > 0)
            {
                urlBuilder.Append('?');

                for (var i = 0; i < urlEncodedParams.Length; i++)
                {
                    var param = urlEncodedParams[i];
                    urlBuilder.Append(param.paramName);
                    urlBuilder.Append('=');
                    urlBuilder.Append(param.paramValue);

                    if (i < urlEncodedParams.Length - 1)
                        urlBuilder.Append('&');
                }
            }

            var url = urlBuilder.ToString();
            GenericPool<StringBuilder>.Release(urlBuilder);
            return url;
        }

        internal static bool TryParseResponse<TResponse>(string endPoint, DisposableTransaction transaction,
            string textResponse, out TResponse response)
        {
            try
            {
                response = JsonUtility.FromJson<TResponse>(textResponse);
                return true;
            }
            catch (Exception e)
            {
                response = default;
                PrintError<TResponse>(endPoint, e.Message);
                transaction.SetStatus(SpanStatus.DataLoss);
                return false;
            }
        }

        internal string GetLambdasUrl()
        {
            // TODO (Santi): This should use catalyst.lambdasUrl instead the hardcode string
            return "https://peer.decentraland.org/lambdas";
        }

        private static void PrintError<TResponse>(string endPoint, string message)
        {
            Debug.LogError($"Lambda {endPoint}, response {typeof(TResponse)}: {message}");
        }

        public void Initialize()
        {
            catalyst = DCL.Environment.i.serviceLocator.Get<IServiceProviders>().catalyst;
        }

        public void Dispose() { }
    }
}
