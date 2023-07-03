using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DCLServices.Lambdas
{
    /// <summary>
    /// Makes requests to Lambdas2 API in a generic manner,
    /// provides retries and errors handling
    /// </summary>
    public interface ILambdasService : IService
    {
        public const int DEFAULT_TIMEOUT = 30;
        public const int DEFAULT_ATTEMPTS_NUMBER = 3;

        /// <summary>
        /// Make a Post request to Lambdas2.
        /// </summary>
        /// <param name="postData">Post Data that will be converted to JSON</param>
        /// <inheritdoc cref="Get{TResponse}"/>
        UniTask<(TResponse response, bool success)> Post<TResponse, TBody>(
            string endPointTemplate,
            string endPoint,
            TBody postData,
            int timeout = DEFAULT_TIMEOUT,
            int attemptsNumber = DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams);

        /// <summary>
        /// Make a Get request to Lambdas2.
        /// </summary>
        /// <param name="endPointTemplate">Endpoint without slashes (e.g. "nfts/emotes/")</param>
        /// <param name="endPoint">Endpoint without slashes but with path variables embedded
        ///     (e.g. "nfts/emotes/0xddf1eec586d8f8f0eb8c5a3bf51fb99379a55684")</param>
        /// <param name="timeout">Timeout for each attempt</param>
        /// <param name="attemptsNumber">Attempts number</param>
        /// <param name="cancellationToken">Cancellation token attacked to the web request</param>
        /// <param name="urlEncodedParams">Params added to the constructed URL</param>
        /// <typeparam name="TResponse">Type the response in parsed by JSON to</typeparam>
        /// <typeparam name="TBody">Type of Post Data</typeparam>
        /// <returns>
        /// Returns (Response, true) if the request is successful and the response is parsed<br/>
        /// Returns (default, false) otherwise<br/>
        /// <exception cref="OperationCanceledException"></exception>
        /// </returns>
        UniTask<(TResponse response, bool success)> Get<TResponse>(
            string endPointTemplate,
            string endPoint,
            int timeout = DEFAULT_TIMEOUT,
            int attemptsNumber = DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams);

        /// <summary>
        /// Make a Get request to Lambdas2.
        /// </summary>
        /// <param name="endPointTemplate">Endpoint without slashes (e.g. "https://peer-ue-2.decentraland.zone/nfts/emotes/:userId")</param>
        /// <param name="url">Composed url including path variables
        ///     (e.g. "https://peer-ue-2.decentraland.zone/nfts/emotes/0xddf1eec586d8f8f0eb8c5a3bf51fb99379a55684")</param>
        /// <param name="timeout">Timeout for each attempt</param>
        /// <param name="attemptsNumber">Attempts number</param>
        /// <param name="cancellationToken">Cancellation token attacked to the web request</param>
        /// <param name="urlEncodedParams">Params added to the constructed URL</param>
        /// <typeparam name="TResponse">Type the response in parsed by JSON to</typeparam>
        /// <typeparam name="TBody">Type of Post Data</typeparam>
        /// <returns>
        /// Returns (Response, true) if the request is successful and the response is parsed<br/>
        /// Returns (default, false) otherwise<br/>
        /// <exception cref="OperationCanceledException"></exception>
        /// </returns>
        UniTask<(TResponse response, bool success)> GetFromSpecificUrl<TResponse>(
            string endPointTemplate,
            string url,
            int timeout = DEFAULT_TIMEOUT,
            int attemptsNumber = DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default,
            params (string paramName, string paramValue)[] urlEncodedParams);

        UniTask<(TResponse response, bool success)> PostFromSpecificUrl<TResponse, TBody>(
            string endPointTemplate,
            string url,
            TBody postData,
            int timeout = DEFAULT_TIMEOUT,
            int attemptsNumber = DEFAULT_ATTEMPTS_NUMBER,
            CancellationToken cancellationToken = default);
    }
}
