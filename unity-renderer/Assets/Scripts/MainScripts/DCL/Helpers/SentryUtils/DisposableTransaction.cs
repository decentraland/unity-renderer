using Sentry;
using System;

namespace MainScripts.DCL.Helpers.SentryUtils
{
    /// <summary>
    /// Finishes `Span` on `Dispose`
    /// </summary>
    public struct DisposableTransaction : IDisposable
    {
        private ISpan span;

        public DisposableTransaction(ISpan span)
        {
            this.span = span;
        }

        public void Dispose()
        {
            span?.Finish();
            span = null;
        }

        public void SetStatus(SpanStatus spanStatus)
        {
            span.Status = spanStatus;
        }
    }
}
