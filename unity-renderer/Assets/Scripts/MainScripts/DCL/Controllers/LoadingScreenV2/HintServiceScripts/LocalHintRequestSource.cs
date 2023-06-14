using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     The LocalHintRequestSource class manages the retrieval of loading screen hints from a local JSON source.
    /// </summary>
    public class LocalHintRequestSource : IHintRequestSource
    {
        private const string LOCAL_HINTS_JSON_SOURCE = "LoadingScreenV2LocalHintsJsonSource";

        private readonly IAddressableResourceProvider addressablesProvider;

        public LocalHintRequestSource(string sourceAddressableSceneJson, SourceTag sourceTag, IAddressableResourceProvider addressablesProvider)
        {
            Source = sourceAddressableSceneJson;
            this.SourceTag = sourceTag;
            LoadingHints = new List<Hint>();
            this.addressablesProvider = addressablesProvider;
        }

        public string Source { get; }
        public SourceTag SourceTag { get; }
        public List<Hint> LoadingHints { get; private set; }

        public UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            // If cancellation is requested, return an empty list
            if (ctx.IsCancellationRequested)
                return UniTask.FromResult(new List<Hint>());

            // Otherwise, return hints when they are loaded from the addressable
            return LoadHintsFromAddressable(Source, ctx);
        }

        public void Dispose()
        {
            LoadingHints.Clear();
        }

        internal async UniTask<List<Hint>> LoadHintsFromAddressable(string addressableSourceKey, CancellationToken ctx)
        {
            try
            {
                if (ctx.IsCancellationRequested)
                    return LoadingHints;

                TextAsset containerSceneAddressable = await addressablesProvider.GetAddressable<TextAsset>(addressableSourceKey, ctx);

                if (containerSceneAddressable == null) { throw new Exception("Failed to load the addressable asset"); }

                LoadingHints = HintSceneParserUtil.ParseJsonToHints(containerSceneAddressable.text);
                return LoadingHints;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load hints from addressable: {ex.Message}");

                return LoadingHints;
            }
        }
    }
}
