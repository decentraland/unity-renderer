using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCL.Controllers.LoadingScreenV2
{
    public interface IHintRequestSource
    {
        List<IHint> loading_hints { get;}
        public string source { get;}
        public SourceTag sourceTag { get;}

        UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx);
    }
}
