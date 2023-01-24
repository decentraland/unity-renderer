using System;
using System.Threading;

namespace DCL.Tasks
{
    public static class CancellationTokenExtensions
    {
        public static CancellationTokenSource SafeRestart(this CancellationTokenSource cancellationToken)
        {
            try
            {
                cancellationToken?.Cancel();
                cancellationToken?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            return new CancellationTokenSource();
        }

        public static CancellationTokenSource SafeRestartLinked(this CancellationTokenSource cancellationToken,
            params CancellationToken[] cancellationTokens)
        {
            try
            {
                cancellationToken?.Cancel();
                cancellationToken?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            return CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens);
        }
    }
}
