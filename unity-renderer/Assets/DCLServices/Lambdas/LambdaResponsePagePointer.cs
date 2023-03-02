using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Pool;

namespace DCLServices.Lambdas
{
    /// <summary>
    /// Caches paginated results until `Dispose` is called
    /// assuming the results are immutable while iterating
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LambdaResponsePagePointer<T> : IDisposable where T : PaginatedResponse
    {
        private readonly int pageSize;
        private readonly Dictionary<int, T> cachedPages;
        private readonly ILambdaServiceConsumer<T> serviceConsumer;
        private readonly CancellationToken cancellationToken;
        private readonly string constEndPoint;

        internal bool isDisposed { get; private set; }

        internal IReadOnlyDictionary<int, T> CachedPages => cachedPages;

        /// <param name="constEndpoint"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken">Pass Cancellation Token so the pointer will be automatically disposed on cancellation</param>
        /// <param name="consumer"></param>
        public LambdaResponsePagePointer(string constEndpoint, int pageSize, CancellationToken cancellationToken,
            ILambdaServiceConsumer<T> consumer)
        {
            this.pageSize = pageSize;
            this.cancellationToken = cancellationToken;
            this.constEndPoint = constEndpoint;

            cachedPages = DictionaryPool<int, T>.Get();
            serviceConsumer = consumer;

            cancellationToken.Register(Dispose, false);
        }

        /// <summary>
        /// Retrieves a page from the endpoint or cache
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async UniTask<(T response, bool success)> GetPageAsync(int pageNum, CancellationToken localCancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(LambdaResponsePagePointer<T>));

            if (cachedPages.TryGetValue(pageNum, out var page))
                return (page, true);

            var ct = this.cancellationToken;

            if (localCancellationToken != CancellationToken.None && !localCancellationToken.Equals(ct))
                ct = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken, localCancellationToken).Token;

            var res = await serviceConsumer.CreateRequest(constEndPoint, pageSize, pageNum, ct);

            if (res.success)
                cachedPages[pageNum] = res.response;

            return res;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            DictionaryPool<int, T>.Release(cachedPages);
            isDisposed = true;
        }
    }
}
