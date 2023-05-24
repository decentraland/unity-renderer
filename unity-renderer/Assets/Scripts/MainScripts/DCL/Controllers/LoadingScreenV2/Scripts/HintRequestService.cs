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
    /// The HintRequestService class is a central manager for retrieving loading screen hints from different sources
    /// and downloading corresponding textures. The class prioritizes hints based on source tag order.
    /// If every IHintRequestSource fails, the service should return an empty list of Hints.
    /// If the texture download fails for any hint, we show the hint with no texture.
    /// </summary>
    public class HintRequestService: IDisposable
    {
        private readonly List<IHintRequestSource> hintRequestSources;
        private readonly Dictionary<SourceTag, List<Hint>> hintsDictionary;
        private readonly SourceTag[] orderedSourceTags;
        private readonly IHintTextureRequestHandler textureRequestHandler;

        private Dictionary<Hint, Texture2D> selectedHints;

        private readonly ISceneController sceneController;

        public HintRequestService(List<IHintRequestSource> hintRequestSources, ISceneController sceneController, IHintTextureRequestHandler textureRequestHandler)
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
                Debug.LogWarning(ex);
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

                        if (sourceHints != null)
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
                        Debug.LogWarning(ex);
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
                    Debug.LogWarning(ex);
                }
            }
        }

        internal void AddToHintsDictionary(SourceTag keyTag, List<Hint> valueHints)
        {
            hintsDictionary.Add(keyTag, valueHints);
        }

        internal void AddToSelectedHints(Hint keyHint, Texture2D valueTexture)
        {
            selectedHints.Add(keyHint, valueTexture);
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
