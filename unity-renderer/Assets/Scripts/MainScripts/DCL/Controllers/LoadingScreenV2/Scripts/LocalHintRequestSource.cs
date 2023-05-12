using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// We should fetch and parse the hint list from a local file
    /// If fetch fails, we return an empty list.
    /// </summary>
    public class LocalHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<IHint> loading_hints { get; private set; }

        private static readonly string LOCAL_HINTS_PATH = "LoadingScreenV2/LocalHintsFallback";

        public LocalHintRequestSource(string source, SourceTag sourceTag)
        {
            this.source = source;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();

            // Load the ScriptableObject from Resources
            LocalHintsFallback localHintsData = Resources.Load<LocalHintsFallback>(LOCAL_HINTS_PATH);

            if (localHintsData == null || localHintsData.slotsDefinition == null) return;

            foreach (SerializableKeyValuePair<SourceTag, List<BaseHint>> slotDefinition in localHintsData.slotsDefinition)
            {
                if (slotDefinition.key != this.sourceTag) continue;

                foreach (BaseHint hint in slotDefinition.value)
                {
                    this.loading_hints.Add(hint);
                }
            }
        }

        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            return UniTask.FromResult(loading_hints);
        }

        public void Dispose()
        {
            loading_hints.Clear();
        }
    }
}

