using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public class LocalHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }

        private UniTaskCompletionSource<bool> hintsLoadedCompletionSource;
        public List<IHint> loading_hints { get; private set; }

        public LocalHintRequestSource(string sourceJson, SourceTag sourceTag)
        {
            this.source = sourceJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();
            this.hintsLoadedCompletionSource = new UniTaskCompletionSource<bool>();

            // Load the hints in a separate thread to avoid blocking
            UniTask.Run(() => LoadHints(sourceJson)).Forget();
        }

        private void LoadHints(string sourceJson)
        {
            try
            {
                IParcelScene parcelScene = JsonUtility.FromJson<IParcelScene>(sourceJson);

                if (parcelScene != null && parcelScene.sceneData?.loadingScreenHints != null)
                {
                    foreach (var hint in parcelScene.sceneData.loadingScreenHints)
                    {
                        loading_hints.Add(new BaseHint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag));
                    }
                }

                hintsLoadedCompletionSource.TrySetResult(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in LocalHintRequestSource.LoadHints: {ex.Message}\n{ex.StackTrace}");
                hintsLoadedCompletionSource.TrySetResult(false);
            }
        }

        public UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            // If cancellation is requested, return an empty list
            if (ctx.IsCancellationRequested)
                return UniTask.FromResult(new List<IHint>());

            // Otherwise, return the loading_hints directly
            return UniTask.FromResult(loading_hints);
        }


        public void Dispose()
        {
            loading_hints.Clear();
        }
    }
}
