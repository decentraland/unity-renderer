using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     The HintRequestService class is a central manager for retrieving loading screen hints from different sources
    ///     and downloading corresponding textures. The class prioritizes hints based on source tag order.
    ///     If every IHintRequestSource fails, the service should return an empty list of Hints.
    ///     If the texture download fails for any hint, we show the hint with no texture.
    /// </summary>
    public class HintRequestService : IDisposable
    {
        private readonly List<IHintRequestSource> hintRequestSources;
        private readonly Dictionary<SourceTag, List<Hint>> hintsDictionary;
        private readonly SourceTag[] orderedSourceTags;

        private readonly ISceneController sceneController;
        private readonly IHintTextureRequestHandler textureRequestHandler;

        private readonly Dictionary<Hint, Texture2D> selectedHints;

        public HintRequestService(List<IHintRequestSource> hintRequestSources, ISceneController sceneController, IHintTextureRequestHandler textureRequestHandler)
        {
            this.hintRequestSources = hintRequestSources;
            hintsDictionary = new Dictionary<SourceTag, List<Hint>>();
            selectedHints = new Dictionary<Hint, Texture2D>();
            orderedSourceTags = new[] { SourceTag.Scene, SourceTag.Event, SourceTag.Dcl };
            this.textureRequestHandler = textureRequestHandler;
            this.sceneController = sceneController;
        }

        public void Dispose()
        {
            foreach (List<Hint> hintList in hintsDictionary.Values) { DisposeHints(hintList); }

            hintsDictionary.Clear();

            foreach (Texture2D texture in selectedHints.Values)
            {
                if (texture != null) { Object.Destroy(texture); }
            }

            selectedHints.Clear();
        }

        public async UniTask<Dictionary<Hint, Texture2D>> RequestHintsFromSources(CancellationToken ctx, int totalHints)
        {
            try
            {
                if (ctx.IsCancellationRequested) { return selectedHints; }

                List<Hint> hints = await GetHintsAsync(ctx);

                hints = SelectOptimalHints(hints, totalHints);

                await DownloadTextures(hints, ctx);
            }
            catch (Exception ex) { Debug.LogWarning(ex); }

            return selectedHints;
        }

        private async UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            var hints = new List<Hint>();

            foreach (IHintRequestSource source in hintRequestSources)
            {
                if (!ctx.IsCancellationRequested)
                {
                    try
                    {
                        List<Hint> sourceHints = await source.GetHintsAsync(ctx);

                        if (sourceHints != null)
                            hints.AddRange(sourceHints);

                        foreach (Hint hint in sourceHints)
                        {
                            if (!hintsDictionary.TryGetValue(hint.SourceTag, out List<Hint> hintList))
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

            foreach (SourceTag sourceTag in orderedSourceTags)
            {
                if (hintsDictionary.TryGetValue(sourceTag, out List<Hint> sourceHints))
                {
                    while (sourceHints.Count > 0 && optimalHints.Count < totalHints)
                    {
                        Hint selectedHint = GetRandomHint(sourceHints);
                        allAvailableHints.Remove(selectedHint);
                        optimalHints.Add(selectedHint);
                    }
                }
            }

            // Random hint selection to fill the remaining slots
            while (allAvailableHints.Count > 0 && optimalHints.Count < totalHints)
            {
                Hint selectedHint = GetRandomHint(allAvailableHints);
                optimalHints.Add(selectedHint);
            }

            return optimalHints;
        }

        private Hint GetRandomHint(List<Hint> hints)
        {
            int randomIndex = Random.Range(0, hints.Count);
            Hint randomHint = hints[randomIndex];
            hints.RemoveAt(randomIndex);
            return randomHint;
        }

        private async UniTask DownloadTextures(List<Hint> finalHints, CancellationToken ctx)
        {
            foreach (Hint hint in finalHints)
            {
                if (ctx.IsCancellationRequested) { break; }

                try { selectedHints.Add(hint, await textureRequestHandler.DownloadTexture(hint.TextureUrl, ctx)); }
                catch (Exception ex) { Debug.LogWarning(ex); }
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
            foreach (KeyValuePair<Hint, Texture2D> hintKvp in selectedHints)
            {
                if (hintKvp.Value != null) { Object.Destroy(hintKvp.Value); }
            }
        }
    }
}
