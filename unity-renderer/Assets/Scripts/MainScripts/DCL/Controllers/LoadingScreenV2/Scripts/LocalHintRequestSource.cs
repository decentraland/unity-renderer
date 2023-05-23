using Cysharp.Threading.Tasks;
using DCL.Models;
using DCL.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public class LocalHintRequestSource : IHintRequestSource
    {
        private const string LOCAL_HINTS_JSON_SOURCE = "LoadingScreenV2LocalHintsJsonSource";

        private IAddressableResourceProvider addressablesProvider;
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<IHint> loading_hints { get; private set; }

        public LocalHintRequestSource(string sourceAddressableSceneJson, SourceTag sourceTag, IAddressableResourceProvider addressablesProvider)
        {
            this.source = sourceAddressableSceneJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();
            this.addressablesProvider = addressablesProvider;
        }

        internal async UniTask<List<IHint>> LoadHintsFromAddressable(string addressableSourceKey, CancellationToken ctx)
        {
            try
            {
                if (ctx.IsCancellationRequested)
                    return loading_hints;

                var containerSceneAddressable = await addressablesProvider.GetAddressable<TextAsset>(addressableSourceKey, ctx);

                if (containerSceneAddressable == null)
                {
                    throw new Exception("Failed to load the addressable asset");
                }

                loading_hints = HintSceneParser.ParseJsonToHints(containerSceneAddressable.text);
                return loading_hints;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load hints from addressable: {ex.Message}");

                return loading_hints;
            }
        }


        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            // If cancellation is requested, return an empty list
            if (ctx.IsCancellationRequested)
                return UniTask.FromResult(new List<IHint>());

            // Otherwise, return hints when they are loaded from the addressable
            return LoadHintsFromAddressable(source, ctx);
        }


        public void Dispose()
        {
            loading_hints.Clear();
        }
    }
}
