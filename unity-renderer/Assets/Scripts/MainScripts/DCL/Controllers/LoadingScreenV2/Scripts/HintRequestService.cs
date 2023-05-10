using Cysharp.Threading.Tasks;
using DCL.Controllers.LoadingScreenV2;
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
    public class HintRequestService
    {
        private List<IHintRequestSource> hintRequestSources;
        private Dictionary<SourceTag, List<IHint>> hintsDictionary = new Dictionary<SourceTag, List<IHint>>();

        public HintRequestService(List<IHintRequestSource> hintRequestSources)
        {
            this.hintRequestSources = hintRequestSources;
        }

        public async UniTask<List<IHint>> RequestHintsAndDownloadTextures(CancellationToken ctx, HintsRules hintRules)
        {
            var hints = new List<IHint>();
            try
            {
                // Step 1: Get Hints
                hints = await GetHintsAsync(ctx);

                // Step 2: Select optimal hints
                // hints = SelectOptimalHints(hints, hintRules);
                hints = SelectOptimalHints(hints);

                // Step 3: Download textures
                await DownloadTextures(hints, ctx);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                // // In case of any exception return an empty list
                // return new List<IHint>();
            }
            return hints;
        }

        public async UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
        {
            var hints = new List<IHint>();
            foreach (var source in hintRequestSources)
            {
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
                    // continue with other sources if one fails
                }
                if (ctx.IsCancellationRequested)
                    break;
            }
            return hints;
        }

        public List<IHint> SelectOptimalHints(List<IHint> hints)
        {
            var allAvailableHints = new List<IHint>(hints);
            var selectedHints = new List<IHint>();

            SourceTag[] orderedSourceTags = { SourceTag.Scene, SourceTag.Event, SourceTag.Dcl };

            foreach (var sourceTag in orderedSourceTags)
            {
                while (hintsDictionary.ContainsKey(sourceTag) && hintsDictionary[sourceTag].Count > 0)
                {
                    var selectedHint = GetRandomHint(hintsDictionary[sourceTag]);
                    allAvailableHints.Remove(selectedHint);
                    selectedHints.Add(selectedHint);
                }
            }

            // If there are still available hints, select them randomly.
            while (allAvailableHints.Count > 0)
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

        // public Dictionary<SourceTag, List<IHint>> GroupHintsBySourceTag(List<IHint> hints)
        // {
        //     var groupedHints = new Dictionary<SourceTag, List<IHint>>();
        //     foreach (var hint in hints)
        //     {
        //         if (!groupedHints.ContainsKey(hint.SourceTag))
        //         {
        //             groupedHints[hint.SourceTag] = new List<IHint>();
        //         }
        //         groupedHints[hint.SourceTag].Add(hint);
        //     }
        //     return groupedHints;
        // }
        //

        // public List<IHint> SelectOptimalHints(List<IHint> hints, HintsRules hintsRules)
        // {
        //     var allAvailableHints = new List<IHint>(hints);
        //     var selectedHints = new List<IHint>();
        //     int hintsToSelect = hintsRules.HintsAmount;
        //
        //     while (hintsToSelect > 0 && allAvailableHints.Count > 0)
        //     {
        //         SourceTag[] orderedSourceTags = { SourceTag.Scene, SourceTag.Event, SourceTag.Dcl };
        //
        //         foreach (var sourceTag in orderedSourceTags)
        //         {
        //             int requiredAmount = 0;
        //             switch (sourceTag)
        //             {
        //                 case SourceTag.Scene:
        //                     requiredAmount = (int)(hintsRules.HintsAmount * hintsRules.ScenePercentage);
        //                     break;
        //                 case SourceTag.Event:
        //                     requiredAmount = (int)(hintsRules.HintsAmount * hintsRules.EventPercentage);
        //                     break;
        //                 case SourceTag.Dcl:
        //                     requiredAmount = (int)(hintsRules.HintsAmount * hintsRules.DclPercentage);
        //                     break;
        //             }
        //
        //             while (requiredAmount > 0 && hintsDictionary.ContainsKey(sourceTag) && hintsDictionary[sourceTag].Count > 0)
        //             {
        //                 var randomHint = GetRandomHint(hintsDictionary[sourceTag]);
        //                 allAvailableHints.Remove(randomHint);
        //                 selectedHints.Add(randomHint);
        //                 hintsToSelect--;
        //                 requiredAmount--;
        //             }
        //
        //             if (hintsToSelect <= 0)
        //             {
        //                 break;
        //             }
        //         }
        //
        //         // If we couldn't select enough hints according to the rules, select randomly from the remaining available hints
        //         while (hintsToSelect > 0 && allAvailableHints.Count > 0)
        //         {
        //             var randomHint = GetRandomHint(allAvailableHints);
        //             selectedHints.Add(randomHint);
        //             hintsToSelect--;
        //         }
        //     }
        //
        //     if (hintsToSelect > 0)
        //     {
        //         Debug.LogWarning("Couldn't select enough hints according to the rules, selected hints count is less than required amount");
        //     }
        //
        //     return selectedHints;
        // }


        private async UniTask DownloadTextures(List<IHint> hints, CancellationToken ctx)
        {
            foreach (var hint in hints)
            {
                try
                {
                    // download and create textures here

                    // assign texture to the hint
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    // continue with other textures if one fails
                }
                if (ctx.IsCancellationRequested)
                    break;
            }
        }

        // stub
        private void DisposeHints(List<IHint> hints)
        {
            foreach (var hint in hints)
            {
            }
        }

        public void Destroy()
        {

        }

    }
}
