using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///  - The view should receive a list of Hint and show them.
    ///  - The view should contain the max amount of hints that can be displayed, and they should be set up after the list of Hint arrives. We could also use a pool.
    ///  - All HintViews should initialize as disabled and hidden (no text, no image)
    ///  - If the list of hints is empty or the amount is less than the max amount, we should disable the rest of the HintViews.
    ///  - When the loading is finished, this class should handle the disposal of the Hint and their textures.
    ///  - When the loading screen is triggered again, we should make sure that old Hints are not loaded or shown.
    ///  - The hints carousel goes to the next hints after a few (n) seconds.
    ///  - The hints carousel allows user input from keys (A or D) to go to the next or previous hint.
    ///  - The hints carousel allows mouse input to select a specific hint.
    ///  - When any hint changes, the next hint timer gets reset.
    /// </summary>
    public class LoadingScreenHintsController
    {
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(5);
        private const int MAX_HINTS = 15;

        private readonly HintView hintViewPrefab;
        private readonly HintRequestService hintRequestService;
        private readonly List<HintView> hintViewPool;

        private Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        private int currentHintIndex = 0;

        private CancellationTokenSource disposeCts;

        public LoadingScreenHintsController(HintView hintViewPrefab, HintRequestService hintRequestService)
        {
            this.hintViewPrefab = hintViewPrefab;
            this.hintRequestService = hintRequestService;

            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            // Instantiate HintView prefabs and add them to the pool
            for (int i = 0; i < MAX_HINTS; i++)
            {
                HintView newHintView = Object.Instantiate(hintViewPrefab);
                newHintView.HideHint();  // Hide it initially
                hintViewPool.Add(newHintView);
            }
        }

        public async UniTask RequestHints(CancellationToken ctx)
        {
            hintViewPool.ForEach(hintView => hintView.HideHint());
            hintsDictionary.Clear();

            Dictionary<Hint, Texture2D> hintsResult = await hintRequestService.RequestHintsFromSources(ctx, MAX_HINTS);
            int index = 0;
            foreach (var hintResult in hintsResult)
            {
                var hintTuple = new Tuple<Hint, Texture2D>(hintResult.Key, hintResult.Value);
                hintsDictionary.Add(index++, hintTuple);

                // Check if this is the first hint, if so start the carousel
                if (index == 1)
                {
                    StartHintsCarousel();
                }
            }
        }

        public void StartHintsCarousel()
        {
            if (disposeCts != null || hintsDictionary.Count == 0) return;

            disposeCts = new CancellationTokenSource();
            ResetHintTimer();
        }

        public void StopHintsCarousel()
        {
            if (disposeCts ==  null) return;

            disposeCts.Cancel();
            disposeCts.Dispose();
            disposeCts = null;
        }

        private async UniTask IterateHintsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                CarouselNextHint();
                await UniTask.Delay(SHOWING_TIME_HINTS, cancellationToken: token);
            }
        }

        public void ResetHintTimer()
        {
            if (disposeCts != null)
            {
                disposeCts.Cancel();
                disposeCts = new CancellationTokenSource();
                IterateHintsAsync(disposeCts.Token).Forget();
            }
        }

        public void CarouselNextHint()
        {
            hintViewPool[currentHintIndex].HideHint();
            currentHintIndex = (currentHintIndex + 1) % hintsDictionary.Count;
            UpdateHintView();
            ResetHintTimer();
        }

        public void CarouselPreviousHint()
        {
            hintViewPool[currentHintIndex].HideHint();
            currentHintIndex = (currentHintIndex - 1 + hintsDictionary.Count) % hintsDictionary.Count;
            UpdateHintView();
            ResetHintTimer();
        }

        private void UpdateHintView()
        {
            var hintTuple = hintsDictionary[currentHintIndex];
            HintView hintView = hintViewPool[currentHintIndex];
            hintView.Initialize(hintTuple.Item1, hintTuple.Item2);
            hintView.ShowHint();
        }

        public void Dispose()
        {
            StopHintsCarousel();

            foreach (var hintKvp in hintsDictionary)
            {
                Object.Destroy(hintKvp.Value.Item2);
            }

            hintsDictionary.Clear();
            hintViewPool.ForEach(hintView => hintView.HideHint());
        }
    }
}




