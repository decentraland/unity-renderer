using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Controllers.LoadingScreenV2
{
    public interface IHintRequestSource: IDisposable
    {
        List<Hint> loading_hints { get;}
        public string source { get;}
        public SourceTag sourceTag { get;}

        UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx);
    }
}
