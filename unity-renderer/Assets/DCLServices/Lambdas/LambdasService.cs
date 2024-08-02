using Cysharp.Threading.Tasks;
using DCL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Pool;

namespace DCLServices.Lambdas
{
    public class LambdasService : ILambdasService
    {
        private ICatalyst catalyst;

        private Service<IWebRequestController> webRequestController;
        private readonly Dictionary<string, string> jsonHeaders = new() { { "Content-Type", "application/json" } };

        public async UniTask<(TResponse response, bool success)> Post<TResponse, TBody>(
            string endPointTemplate,
            string endPoint,
            TBody postData,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var postDataJson = JsonUtility.ToJson(postData);
            string lambdaUrl = await catalyst.GetLambdaUrl(cancellationToken);
            var url = GetUrl(endPoint, lambdaUrl, urlEncodedParams);
            var wr = webRequestController.Ref.Post(url, postDataJson, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false);

            return await SendRequestAsync<TResponse>(wr, cancellationToken, endPoint);
        }

        public async UniTask<(TResponse response, bool success)> Get<TResponse>(
            string endPointTemplate,
            string endPoint,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            string lambdaUrl = await catalyst.GetLambdaUrl(cancellationToken);
            var url = GetUrl(endPoint, lambdaUrl, urlEncodedParams);
            var wr = webRequestController.Ref.Get(url, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false);

            return await SendRequestAsync<TResponse>(wr, cancellationToken, endPoint);
        }

        public async UniTask<(TResponse response, bool success)> GetFromSpecificUrl<TResponse>(
            string endPointTemplate,
            string url,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            bool isSigned = false,
            string signUrl = null,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams)
        {
            string urlWithParams = AppendQueryParamsToUrl(url, urlEncodedParams);
            UnityWebRequest request = await webRequestController.Ref.GetAsync(urlWithParams, requestAttemps: attemptsNumber, timeout: timeout, isSigned: isSigned, signUrl: signUrl, cancellationToken: cancellationToken);

            if (request.result != UnityWebRequest.Result.Success)
                return (default(TResponse), false);

            string text = request.downloadHandler.text;

            try
            {
                var response = JsonConvert.DeserializeObject<TResponse>(text);
                return (response, true);
            }
            catch (Exception e)
            {
                PrintError<TResponse>(url, e.Message);
                return (default(TResponse), false);
            }
        }

        public async UniTask<(TResponse response, bool success)> PostFromSpecificUrl<TResponse, TBody>(
            string endPointTemplate,
            string url,
            TBody postData,
            int timeout = ILambdasService.DEFAULT_TIMEOUT,
            int attemptsNumber = ILambdasService.DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default)
        {
            var postDataJson = JsonUtility.ToJson(postData);
            var wr = webRequestController.Ref.Post(url, postDataJson, requestAttemps: attemptsNumber, timeout: timeout, disposeOnCompleted: false, headers: jsonHeaders);

            return await SendRequestAsync<TResponse>(wr, cancellationToken, url);
        }

        private async UniTask<(TResponse response, bool success)> SendRequestAsync<TResponse>(
            IWebRequestAsyncOperation webRequestAsyncOperation,
            CancellationToken cancellationToken,
            string endPoint)
        {
            await webRequestAsyncOperation.WithCancellation(cancellationToken);

            if (!webRequestAsyncOperation.isSucceeded)
            {
                Debug.LogError($"{webRequestAsyncOperation.asyncOp.webRequest.error} {webRequestAsyncOperation.asyncOp.webRequest.url}");
                return (default, false);
            }

            string textResponse = webRequestAsyncOperation.webRequest.downloadHandler.text;
            webRequestAsyncOperation.Dispose();

            var res = !TryParseResponse(endPoint, textResponse, out TResponse response) ? (default, false) : (response, true);
            return res;
        }

        internal string GetUrl(string endPoint, string lambdaUrl, params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var urlBuilder = GenericPool<StringBuilder>.Get();
            urlBuilder.Clear();
            urlBuilder.Append(lambdaUrl);
            if (!urlBuilder.ToString().EndsWith('/'))
                urlBuilder.Append('/');

            var endPointSpan = endPoint.AsSpan();

            if (endPoint.StartsWith('/') || endPoint.StartsWith('\\'))
                endPointSpan = endPointSpan[1..];

            if (endPoint.EndsWith('/') || endPoint.EndsWith('\\') || endPoint.EndsWith('?'))
                endPointSpan = endPointSpan[..^1];

            urlBuilder.Append(endPointSpan);

            if (urlEncodedParams.Length > 0)
            {
                urlBuilder.Append(endPoint.Contains('?') ? '&' : '?');

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

        private string AppendQueryParamsToUrl(string url, params (string paramName, string paramValue)[] urlEncodedParams)
        {
            var urlBuilder = GenericPool<StringBuilder>.Get();
            urlBuilder.Clear();
            urlBuilder.Append(url);
            if (!urlBuilder.ToString().EndsWith('/'))
                urlBuilder.Append('/');

            if (urlEncodedParams.Length > 0)
            {
                urlBuilder.Append(url.Contains('?') ? '&' : '?');

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

            var result = urlBuilder.ToString();
            GenericPool<StringBuilder>.Release(urlBuilder);
            return result;
        }

        internal static bool TryParseResponse<TResponse>(string endPoint,
            string textResponse, out TResponse response)
        {
            try
            {
                response = JsonConvert.DeserializeObject<TResponse>(textResponse);
                return true;
            }
            catch (Exception e)
            {
                response = default;
                PrintError<TResponse>(endPoint, e.Message);
                return false;
            }
        }

        internal string GetLambdasUrl() =>
            catalyst.lambdasUrl;

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
