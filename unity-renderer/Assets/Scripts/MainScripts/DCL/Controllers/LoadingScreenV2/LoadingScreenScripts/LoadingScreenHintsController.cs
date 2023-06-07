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
        private bool isRequestingHints = false;

        internal readonly List<HintView> hintViewPool;
        internal Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        internal int currentHintIndex = 0;
        internal int hintCount = 0;
        internal CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public event Action OnRequestHintsCompleted;

        public LoadingScreenHintsController(HintView hintViewPrefab, HintRequestService hintRequestService)
        {
            Debug.Log("FD:: LoadingScreenHintsController constructor");
            this.hintViewPrefab = hintViewPrefab;
            this.hintRequestService = hintRequestService;

            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            // Instantiate HintView prefabs and add them to the pool
            for (int i = 0; i < MAX_HINTS; i++)
            {
                HintView newHintView = Object.Instantiate(hintViewPrefab);
                newHintView.ShowHint(false);  // Hide it initially
                hintViewPool.Add(newHintView);
            }

            InitializeHints(cancellationTokenSource.Token);
        }

        private async void InitializeHints(CancellationToken ctx)
        {
            await RequestHints(ctx);
        }

        public async UniTask RequestHints(CancellationToken ctx)
        {
            Debug.Log("FD:: RequestHints");
            if (isRequestingHints) return;
            isRequestingHints = true;

            hintViewPool.ForEach(hintView => hintView.ShowHint(false));
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
            hintCount = hintsDictionary.Count;
            isRequestingHints = false;

            OnRequestHintsCompleted?.Invoke();
        }

        public void StartHintsCarousel()
        {
            Debug.Log("FD:: StartHintsCarousel");
            if (cancellationTokenSource != null || hintsDictionary.Count == 0) return;

            cancellationTokenSource = new CancellationTokenSource();
            ResetHintTimer();
        }

        public void StopHintsCarousel()
        {
            Debug.Log("FD:: StopHintsCarousel");
            if (cancellationTokenSource ==  null) return;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        private async UniTask IterateHintsAsync(CancellationToken token)
        {
            Debug.Log("FD:: IterateHintsAsync");
            while (!token.IsCancellationRequested)
            {
                CarouselNextHint();

                await UniTask.Delay(SHOWING_TIME_HINTS, cancellationToken: token);
            }
        }

        public void ResetHintTimer()
        {
            Debug.Log("FD:: ResetHintTimer");
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                IterateHintsAsync(cancellationTokenSource.Token).Forget();
            }
        }

        public void CarouselNextHint()
        {
            Debug.Log("FD:: CarouselNextHint 1 - isRequestingHints " + isRequestingHints + " hintCount " + hintCount);
            if (isRequestingHints || hintCount == 0) return;
            Debug.Log("FD:: CarouselNextHint 2");

            hintViewPool[currentHintIndex].ShowHint(false);
            currentHintIndex = (currentHintIndex + 1) % hintsDictionary.Count;
            UpdateHintView();
            // ResetHintTimer();
        }

        public void CarouselPreviousHint()
        {
            Debug.Log("FD:: CarouselPreviousHint 1");
            if (isRequestingHints || hintCount == 0) return;
            Debug.Log("FD:: CarouselPreviousHint 2");

            hintViewPool[currentHintIndex].ShowHint(false);
            currentHintIndex = (currentHintIndex - 1 + hintsDictionary.Count) % hintsDictionary.Count;
            UpdateHintView();
            // ResetHintTimer();
        }

        private void UpdateHintView()
        {
            Debug.Log("FD:: UpdateHintView");
            var hintTuple = hintsDictionary[currentHintIndex];
            hintViewPool[currentHintIndex].Initialize(hintTuple.Item1, hintTuple.Item2);
            hintViewPool[currentHintIndex].ShowHint(true);
        }

        public void Dispose()
        {
            Debug.Log("FD:: Dispose");
            StopHintsCarousel();

            foreach (var hintKvp in hintsDictionary)
            {
                Object.Destroy(hintKvp.Value.Item2);
            }

            hintsDictionary.Clear();
            hintViewPool.ForEach(hintView => hintView.ShowHint(false));
        }
    }
}




