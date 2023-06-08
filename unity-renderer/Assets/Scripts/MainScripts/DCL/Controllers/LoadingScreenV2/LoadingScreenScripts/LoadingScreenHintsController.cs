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
        // private bool isIteratingHints = false;

        internal HintViewManager hintViewManager;
        internal readonly List<HintView> hintViewPool;
        internal Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        internal int currentHintIndex = 0;
        // internal int hintCount = 0;
        internal CancellationTokenSource cancellationTokenSource;

        public event Action OnRequestHintsCompleted;

        public LoadingScreenHintsController(HintView hintViewPrefab, HintRequestService hintRequestService)
        {
            this.hintViewPrefab = hintViewPrefab;
            this.hintRequestService = hintRequestService;

            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            // Initializing empty hints views
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
            Debug.Log($"FD:: Hints received: {hintsResult.Count}");
            int index = 0;
            var intializedHints = new List<HintView>();
            foreach (var hintResult in hintsResult)
            {
                var hintTuple = new Tuple<Hint, Texture2D>(hintResult.Key, hintResult.Value);
                hintsDictionary.Add(index, hintTuple);

                if (index < hintViewPool.Count)
                {
                    hintViewPool[index].Initialize(hintResult.Key, hintResult.Value, index == 0);
                    intializedHints.Add(hintViewPool[index]);
                }
                index++;
            }
            Debug.Log($"FD:: Hints initialized: {hintsDictionary.Count}");
            isRequestingHints = false;
            hintViewManager = new HintViewManager(intializedHints);

            OnRequestHintsCompleted?.Invoke();
        }

        public void StartHintsCarousel()
        {
            cancellationTokenSource = new CancellationTokenSource();
            hintViewManager.StartCarousel(cancellationTokenSource.Token);

        }

        public void StopHintsCarousel()
        {
            cancellationTokenSource.Cancel();
            hintViewManager.StopCarousel();
            cancellationTokenSource = null;
        }


        public void CarouselNextHint()
        {
            hintViewManager.CarouselNextHint();
        }

        public void CarouselPreviousHint()
        {
            hintViewManager.CarouselPreviousHint();
        }

        public void SetHint(int index)
        {
            hintViewManager.SetHint(index);
        }

        public void Dispose()
        {
            hintViewManager.Dispose();
        }
    }
}
