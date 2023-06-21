using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    /// Controller responsible of managing the hints views and requesting hints from the HintRequestService
    /// And also responsible of showing the hints in the LoadingScreen using the HintViewManager carousel
    /// </summary>
    public class LoadingScreenHintsController
    {
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(5);
        private readonly string SOURCE_HINT_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private const int MAX_HINTS = 15;

        private readonly HintRequestService hintRequestService;

        internal HintView hintViewPrefab;
        internal HintViewManager hintViewManager;
        internal readonly List<HintView> hintViewPool;
        internal Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        internal CancellationTokenSource cancellationTokenSource;

        public event Action OnRequestHintsCompleted;

        public LoadingScreenHintsController(HintRequestService hintRequestService)
        {
            this.hintRequestService = hintRequestService;

            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            InitializeHintsAsync();
        }

        private async void InitializeHintsAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();

            IAddressableResourceProvider  addressableProvider = new AddressableResourceProvider();
            hintViewPrefab = await addressableProvider.GetAddressable<HintView>(SOURCE_HINT_ADDRESSABLE, cancellationTokenSource.Token);

            // Initializing empty hints views
            for (int i = 0; i < MAX_HINTS; i++)
            {
                HintView newHintView = Object.Instantiate(hintViewPrefab);
                newHintView.ToggleHint(false);
                hintViewPool.Add(newHintView);
            }

            await RequestHints(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Requests hints from the HintRequestService and initializes the hints views with the results
        /// </summary>
        /// <param name="ctx"></param>
        public async UniTask RequestHints(CancellationToken ctx)
        {
            hintsDictionary.Clear();

            Dictionary<Hint, Texture2D> hintsResult = await hintRequestService.RequestHintsFromSources(ctx, MAX_HINTS);

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

            hintViewManager = new HintViewManager(intializedHints);

            StartHintsCarousel();
            OnRequestHintsCompleted?.Invoke();
        }

        public void StartHintsCarousel()
        {
            hintViewManager.StartCarousel();
        }

        public void StopHintsCarousel()
        {
            hintViewManager.StopCarousel();
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
            hintViewManager.SetSpecificHint(index);
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            hintViewManager.Dispose();
        }
    }
}
