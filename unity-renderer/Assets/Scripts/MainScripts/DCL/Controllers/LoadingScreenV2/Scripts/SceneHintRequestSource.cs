using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public class SceneHintRequestSource: IHintRequestSource
    {
        public string source { get; }
        public List<IHint> loading_hints { get; }
        public SourceTag sourceTag { get; }

        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx) =>
            throw new NotImplementedException();

        public SceneHintRequestSource(string source, SourceTag sourceTag)
        {
            this.source = source;
            this.sourceTag = sourceTag;
        }

    }
}
