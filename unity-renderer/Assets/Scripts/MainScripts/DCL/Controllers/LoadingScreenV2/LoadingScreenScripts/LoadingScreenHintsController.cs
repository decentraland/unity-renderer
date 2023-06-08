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
        private bool isIteratingHints = false;

        internal readonly List<HintView> hintViewPool;
        internal Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        internal int currentHintIndex = 0;
        internal int hintCount = 0;
        internal CancellationTokenSource cancellationTokenSource;

        public event Action OnRequestHintsCompleted;
        public event Action OnHintChanged;

        public LoadingScreenHintsController(HintView hintViewPrefab, HintRequestService hintRequestService)
        {
            this.hintViewPrefab = hintViewPrefab;
            this.hintRequestService = hintRequestService;

            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            for (int i = 0; i < MAX_HINTS; i++)
            {
                HintView newHintView = Object.Instantiate(hintViewPrefab);
                newHintView.ShowHint(false);
                hintViewPool.Add(newHintView);
            }

            InitializeHints();
        }

        private async void InitializeHints()
        {
            cancellationTokenSource = new CancellationTokenSource();
            await RequestHints(cancellationTokenSource.Token);
        }

        public async UniTask RequestHints(CancellationToken ctx)
        {
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
            if (isIteratingHints || hintsDictionary.Count == 0)
                return;

            isIteratingHints = true;
            IterateHintsAsync(cancellationTokenSource.Token).Forget();
        }

        public void StopHintsCarousel()
        {
            if (cancellationTokenSource == null)
                return;

            isIteratingHints = false; // stop the iteration
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        private async UniTask IterateHintsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                CarouselNextHint();
                try
                {
                    await UniTask.Delay((int)SHOWING_TIME_HINTS.TotalMilliseconds, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    // Operation was cancelled
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void CarouselNextHint()
        {
            if (isRequestingHints || hintCount == 0)
                return;

            SetHint((currentHintIndex + 1) % hintsDictionary.Count);
        }

        public void CarouselPreviousHint()
        {
            if (isRequestingHints || hintCount == 0)
                return;

            SetHint((currentHintIndex - 1 + hintsDictionary.Count) % hintsDictionary.Count);
        }

        public void SetHint(int index)
        {
            hintViewPool[currentHintIndex].ShowHint(false);
            currentHintIndex = index;
            UpdateHintView();
        }

        private void UpdateHintView()
        {
            var hintTuple = hintsDictionary[currentHintIndex];
            hintViewPool[currentHintIndex].Initialize(hintTuple.Item1, hintTuple.Item2);
            hintViewPool[currentHintIndex].ShowHint(true);

            OnHintChanged?.Invoke();
        }

        public void Dispose()
        {
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
