using Cysharp.Threading.Tasks;
using DCL.Controllers.LoadingScreenV2;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public struct HintsRules
    {
        public int HintsAmount;
        public float ScenePercentage;
        public float EventPercentage;
        public float DclPercentage;
    }

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
        private List<IHintRequestSource> hintRequestSources;
        private Dictionary<SourceTag, List<IHint>> hintsDictionary = new Dictionary<SourceTag, List<IHint>>();

        private SourceTag[] orderedSourceTags = { SourceTag.Scene, SourceTag.Event, SourceTag.Dcl };

        public HintRequestService(List<IHintRequestSource> hintRequestSources)
        {
            this.hintRequestSources = hintRequestSources;
        }

        public async UniTask<List<IHint>> RequestHintsAndDownloadTextures(CancellationToken ctx, int totalHints)
        {
            var hints = new List<IHint>();
            try
            {
                if (ctx.IsCancellationRequested)
                {
                    return new List<IHint>();
                }

                // Step 1: Get Hints
                hints = await GetHintsAsync(ctx);

                // Step 2: Select optimal hints
                hints = SelectOptimalHints(hints, totalHints);

                // Step 3: Download textures
                await DownloadTextures(hints, ctx);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return hints;
        }

        public async UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            var hints = new List<IHint>();
            foreach (var source in hintRequestSources)
            {
                if (ctx.IsCancellationRequested)
                {
                    return new List<IHint>();
                }

                try
                {
                    var sourceHints = await source.GetHintsAsync(ctx);
                    hints.AddRange(sourceHints);

                    foreach (var hint in sourceHints)
                    {
                        if (!hintsDictionary.ContainsKey(hint.SourceTag))
                        {
                            hintsDictionary[hint.SourceTag] = new List<IHint>();
                        }
                        hintsDictionary[hint.SourceTag].Add(hint);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    // In case of any exception return an empty list
                    return new List<IHint>();
                }
            }
            return hints;
        }

        public List<IHint> SelectOptimalHints(List<IHint> hints, int totalHints)
        {
            var allAvailableHints = new List<IHint>(hints);
            var selectedHints = new List<IHint>();

            foreach (var sourceTag in orderedSourceTags)
            {
                while (hintsDictionary.ContainsKey(sourceTag) && hintsDictionary[sourceTag].Count > 0 && selectedHints.Count < totalHints)
                {
                    var selectedHint = GetRandomHint(hintsDictionary[sourceTag]);
                    allAvailableHints.Remove(selectedHint);
                    selectedHints.Add(selectedHint);
                }
            }

            // Random hint selection to fill the remaining slots
            while (allAvailableHints.Count > 0 && selectedHints.Count < totalHints)
            {
                var selectedHint = GetRandomHint(allAvailableHints);
                selectedHints.Add(selectedHint);
            }

            return selectedHints;
        }

        public IHint GetRandomHint(List<IHint> hints)
        {
            int randomIndex = UnityEngine.Random.Range(0, hints.Count);
            IHint randomHint = hints[randomIndex];
            hints.RemoveAt(randomIndex);
            return randomHint;
        }

        private async UniTask DownloadTextures(List<IHint> hints, CancellationToken ctx)
        {
            foreach (var hint in hints)
            {
                if (ctx.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    // TODO:: download and create textures here

                    // TODO:: assign texture to the hint in a dictionary
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        // stub methods
        private void DisposeHints(List<IHint> hints)
        {
            foreach (var hint in hints)
            {
                hint.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var hintList in hintsDictionary.Values)
            {
                DisposeHints(hintList);
            }

            hintsDictionary.Clear();

            // TODO:: FD:: Dispose other stuff
        }
    }
}
