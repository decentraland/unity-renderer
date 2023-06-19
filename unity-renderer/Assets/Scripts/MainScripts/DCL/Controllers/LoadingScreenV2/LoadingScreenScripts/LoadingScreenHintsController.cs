using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UIComponents.Scripts.Components;

namespace DCL.LoadingScreen.V2
{
    /// <summary>
    /// TODO:: FD:: Delete this plugin if not needed anymore
    /// Controller responsible of managing the hints views and requesting hints from the HintRequestService
    /// And also responsible of showing the hints in the LoadingScreen using the HintViewManager carousel
    /// </summary>
    public class LoadingScreenHintsController: ILoadingScreenHintsController
    {
        private const int MAX_HINTS = 15;
        private const string HINT_VIEW_PREFAB_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(5);
        private readonly HintRequestService hintRequestService;
        private readonly IAddressableResourceProvider addressableProvider;
        private readonly ILoadingScreenView loadingScreenView;

        internal HintView hintViewPrefab;
        internal HintViewManager hintViewManager;
        internal readonly List<HintView> hintViewPool;
        internal Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        internal CancellationTokenSource cancellationTokenSource;

        public event Action OnRequestHintsCompleted;

        public LoadingScreenHintsController(HintRequestService hintRequestService, ILoadingScreenView loadingScreenView, IAddressableResourceProvider addressableProvider)
        {
            // addressableProvider = Environment.i.serviceLocator.Get<IAddressableResourceProvider>();
            this.addressableProvider = addressableProvider;
            this.hintRequestService = hintRequestService;
            this.loadingScreenView = loadingScreenView;

            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            InitializeHintsAsync();
        }

        private async void InitializeHintsAsync()
        {
            Debug.Log("FD:: LoadingScreenHintsController - InitializeHintsAsync");
            cancellationTokenSource = new CancellationTokenSource();

            if (addressableProvider == null) {
                Debug.Log("FD:: addressableProvider is null");
            }
            hintViewPrefab = await addressableProvider.GetAddressable<HintView>(HINT_VIEW_PREFAB_ADDRESSABLE, cancellationTokenSource.Token);

            if (hintViewPrefab == null) {
                Debug.Log("FD:: hintViewPrefab is null");
            }

            if (loadingScreenView == null) {
                Debug.Log("FD:: loadingScreenView is null");
            }
            var hintsContainer = loadingScreenView.GetHintContainer();

            if (hintsContainer == null) {
                Debug.Log("FD:: hintsContainer is null");
            }

            // FD:: original without debugs
            // cancellationTokenSource = new CancellationTokenSource();
            // hintViewPrefab = await addressableProvider.GetAddressable<HintView>(HINT_VIEW_PREFAB_ADDRESSABLE, cancellationTokenSource.Token);
            // var hintsContainer = loadingScreenView.GetHintContainer();
            // Initializing empty hints views
            for (int i = 0; i < MAX_HINTS; i++)
            {
                HintView newHintView = UnityEngine.Object.Instantiate(hintViewPrefab, hintsContainer, true);
                newHintView.ToggleHint(false);
                hintViewPool.Add(newHintView);
            }

            await RequestHints(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Requests hints from the HintRequestService and initializes the hints views with the results
        /// </summary>
        /// <param name="ctx"></param>
        private async UniTask RequestHints(CancellationToken ctx)
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

            hintViewManager = new HintViewManager(intializedHints, SHOWING_TIME_HINTS);

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
