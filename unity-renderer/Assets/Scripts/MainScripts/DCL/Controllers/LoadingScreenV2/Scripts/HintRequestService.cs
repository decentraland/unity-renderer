using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// HintRequestService should request hints from different sources IHintRequestSource which can be local or remote.
    /// All IHintRequestSource will yield multiple IHint, unless the request fails, they should return an empty list.
    /// Every IHint contains its texture URL, title, body, and source tag (Scene, Dcl, Event, etc)
    /// This HintRequestService is responsible for yielding the most optimal amount of hints (N) in the correct order (Scene > Event > Dcl), after that is decided, the Textures should be downloaded and created.
    /// The request for hints is asynchronous (and must be done with unitask and cancelation token and should be cancellable at all times and every IHint should be disposed and if the textures were created, they should be destroyed.
    /// If every IHintRequestSource fails, the service should return an empty list of Hints.
    /// If the texture download fails for any hint, we show the hint with no texture.
    /// If any exception occurs inside the HintRequestService, the exception should be shown in the console and the service should return an empty list of Hints to avoid locking the loading process.
    /// Every item described above should have its own Unit Test
    ///
    /// This request should be done at the LoadingScreenController after the loading is triggered, we need to figure out if loading the hints in parallel is the best approach, if not, we should load the hints first and then the scene assets.
    /// </summary>
    public class HintRequestService: IDisposable
    {
        private readonly List<IHintRequestSource> hintRequestSources;
        private readonly Dictionary<SourceTag, List<Hint>> hintsDictionary;
        private readonly SourceTag[] orderedSourceTags;
        private readonly HintTextureRequestHandler textureRequestHandler;

        private Dictionary<Hint, Texture2D> selectedHints;

        private readonly ISceneController sceneController;

        public HintRequestService(List<IHintRequestSource> hintRequestSources, ISceneController sceneController)
        {
            this.hintRequestSources = hintRequestSources;
            this.hintsDictionary = new Dictionary<SourceTag, List<Hint>>();
            this.selectedHints = new Dictionary<Hint, Texture2D>();
            this.orderedSourceTags = new[] { SourceTag.Scene, SourceTag.Event, SourceTag.Dcl };
            this.textureRequestHandler = new HintTextureRequestHandler();
            this.sceneController = sceneController;
        }

        public HintRequestService(List<IHintRequestSource> hintRequestSources, ISceneController sceneController, HintTextureRequestHandler textureRequestHandler)
        {
            this.hintRequestSources = hintRequestSources;
            this.hintsDictionary = new Dictionary<SourceTag, List<Hint>>();
            this.selectedHints = new Dictionary<Hint, Texture2D>();
            this.orderedSourceTags = new[] { SourceTag.Scene, SourceTag.Event, SourceTag.Dcl };
            this.textureRequestHandler = textureRequestHandler;
            this.sceneController = sceneController;
        }

        public async UniTask<Dictionary<Hint, Texture2D>> RequestHintsFromSources(CancellationToken ctx, int totalHints)
        {
            try
            {
                if (ctx.IsCancellationRequested)
                {
                    return selectedHints;
                }
                // Step 1: Get Hints
                var hints = await GetHintsAsync(ctx);
                // Step 2: Select optimal hints
                hints = SelectOptimalHints(hints, totalHints);
                // Step 3: Download textures
                await DownloadTextures(hints, ctx);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return selectedHints;
        }

        private async UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            var hints = new List<Hint>();
            foreach (var source in hintRequestSources)
            {
                if (!ctx.IsCancellationRequested)
                {
                    try
                    {
                        var sourceHints = await source.GetHintsAsync(ctx);
                        hints.AddRange(sourceHints);

                        foreach (var hint in sourceHints)
                        {
                            if (!hintsDictionary.TryGetValue(hint.SourceTag, out var hintList))
                            {
                                hintList = new List<Hint>();
                                hintsDictionary[hint.SourceTag] = hintList;
                            }
                            hintList.Add(hint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        throw;
                    }
                }
            }
            return hints;
        }


        private List<Hint> SelectOptimalHints(List<Hint> hints, int totalHints)
        {
            var optimalHints = new List<Hint>();
            var allAvailableHints = new List<Hint>(hints);

            foreach (var sourceTag in orderedSourceTags)
            {
                if (hintsDictionary.TryGetValue(sourceTag, out var sourceHints))
                {
                    while (sourceHints.Count > 0 && optimalHints.Count < totalHints)
                    {
                        var selectedHint = GetRandomHint(sourceHints);
                        allAvailableHints.Remove(selectedHint);
                        optimalHints.Add(selectedHint);
                    }
                }
            }

            // Random hint selection to fill the remaining slots
            while (allAvailableHints.Count > 0 && optimalHints.Count < totalHints)
            {
                var selectedHint = GetRandomHint(allAvailableHints);
                optimalHints.Add(selectedHint);
            }

            return optimalHints;
        }

        private Hint GetRandomHint(List<Hint> hints)
        {
            int randomIndex = UnityEngine.Random.Range(0, hints.Count);
            Hint randomHint = hints[randomIndex];
            hints.RemoveAt(randomIndex);
            return randomHint;
        }

        private async UniTask DownloadTextures(List<Hint> finalHints, CancellationToken ctx)
        {
            foreach (var hint in finalHints)
            {
                if (ctx.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    selectedHints.Add(hint, await textureRequestHandler.DownloadTexture(hint.TextureUrl, ctx));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void DisposeHints(List<Hint> hints)
        {
            foreach (var hintKvp in selectedHints)
            {
                if (hintKvp.Value != null)
                {
                    UnityEngine.Object.Destroy(hintKvp.Value);
                }
            }
        }

        public void Dispose()
        {
            // Dispose of the hints in the hintsDictionary
            foreach (var hintList in hintsDictionary.Values)
            {
                DisposeHints(hintList);
            }
            hintsDictionary.Clear();

            // Dispose of the textures in the selectedHints dictionary
            foreach (var texture in selectedHints.Values)
            {
                if(texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
            selectedHints.Clear();
        }

    }
}
