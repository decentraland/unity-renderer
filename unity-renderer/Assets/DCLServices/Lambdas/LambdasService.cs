using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.Lambdas
{
    public class LambdasService : ILambdasService
    {
        private ICatalyst catalyst;

        private Service<IWebRequestController> webRequestController;

        public UniTask<(TResponse response, bool success)> Post<TResponse, TBody>(
            string endPoint,
            TBody postData,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var postDataJson = JsonUtility.ToJson(postData);
            var url = GetUrl(endPoint, urlEncodedParams);
            var wr = webRequestController.Ref.Post(url, postDataJson, requestAttemps: attemptsNumber, timeout: timeout);

            return SendRequestAsync<TResponse>(wr, cancellationToken, endPoint, urlEncodedParams);
        }

        public UniTask<(TResponse response, bool success)> Get<TResponse>(
            string endPoint,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var url = GetUrl(endPoint, urlEncodedParams);
            var wr = webRequestController.Ref.Get(url, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false);

            return SendRequestAsync<TResponse>(wr, cancellationToken, endPoint, urlEncodedParams);
        }

        private async UniTask<(TResponse response, bool success)> SendRequestAsync<TResponse>(
            IWebRequestAsyncOperation webRequestAsyncOperation,
            CancellationToken cancellationToken,
            string endPoint,
            (string paramName, string paramValue)[] urlEncodedParams)
        {
            await webRequestAsyncOperation.WithCancellation(cancellationToken);

            if (!webRequestAsyncOperation.isSucceeded)
                return (default, false);

            string textResponse = webRequestAsyncOperation.webRequest.downloadHandler.text;
            webRequestAsyncOperation.Dispose();

            return !TryParseResponse(endPoint, urlEncodedParams, textResponse, out TResponse response) ? (default, false) : (response, true);
        }

        internal string GetUrl(string endPoint, (string paramName, string paramValue)[] urlEncodedParams)
        {
            var urlBuilder = GenericPool<StringBuilder>.Get();
            urlBuilder.Clear();

            //urlBuilder.Append(catalyst.lambdasUrl);
            // TODO (Santi): This is temporal until they fix the problems with lambdas routing
            urlBuilder.Append("http://peer-eu1.decentraland.org/lambdas/");
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

        internal static bool TryParseResponse<TResponse>(string endPoint, (string paramName, string paramValue)[] urlEncodedParams,
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
                AddSentryExtraData(urlEncodedParams);
                return false;
            }
        }

        private static void AddSentryExtraData((string paramName, string paramValue)[] urlEncodedParams)
        {
            // TODO: once Sentry is integrated add urlEncodedParams to Extra Data or Breadcrumbs
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
