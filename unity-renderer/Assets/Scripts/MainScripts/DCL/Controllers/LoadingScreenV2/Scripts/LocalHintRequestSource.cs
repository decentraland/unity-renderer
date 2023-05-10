using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public class LocalHintRequestSource : IHintRequestSource
    {
        private static readonly string LOCAL_HINTS_PATH = "LoadingScreenV2/LocalHints";

        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<IHint> loading_hints { get; }

        public LocalHintRequestSource(string source, SourceTag sourceTag)
        {
            this.source = source;
            this.sourceTag = sourceTag;
        }

        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx) =>
            throw new NotImplementedException();
    }
}
