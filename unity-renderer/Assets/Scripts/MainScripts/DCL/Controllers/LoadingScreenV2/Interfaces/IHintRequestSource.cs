using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Controllers.LoadingScreenV2
{
    public interface IHintRequestSource : IDisposable
    {
        List<Hint> LoadingHints { get; }
        public string Source { get; }
        public SourceTag SourceTag { get; }

        UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx);
    }
}
