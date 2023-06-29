using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UIComponents.Scripts.Components;
using Object = UnityEngine.Object;

namespace DCL.LoadingScreen.V2
{
    /// <summary>
    /// Controller responsible of managing the hints views and requesting hints from the HintRequestService
    /// And also responsible of showing the hints in the LoadingScreen using the HintViewManager carousel
    /// </summary>
    public class LoadingScreenHintsController: ILoadingScreenHintsController
    {
        private const int MAX_HINTS = 15;
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(10f);
        private readonly float FADE_DURATION = 0.5f;
        private const string HINT_VIEW_PREFAB_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private readonly HintRequestService hintRequestService;
        private readonly IAddressableResourceProvider addressableProvider;
        private readonly ILoadingScreenView loadingScreenView;
        private readonly LoadingScreenV2HintsPanelView loadingScreenV2HintsPanelView;

        private InputAction_Trigger shortcutLeftInputAction;
        private InputAction_Trigger shortcutRightInputAction;

        private bool hintsControllerInitialized = false;

        internal HintView hintViewPrefab;
        internal HintViewManager hintViewManager;
        internal List<HintView> hintViewPool;
        internal Dictionary<int, Tuple<Hint, Texture2D>> hintsDictionary;
        internal CancellationTokenSource cancellationTokenSource;

        public event Action OnRequestHintsCompleted;

        public LoadingScreenHintsController(HintRequestService hintRequestService, ILoadingScreenView loadingScreenView, IAddressableResourceProvider addressableProvider)
        {
            this.addressableProvider = addressableProvider;
            this.hintRequestService = hintRequestService;
            this.loadingScreenView = loadingScreenView;
            this.loadingScreenV2HintsPanelView = loadingScreenView.GetHintsPanelView();

            ConfigureShortcuts();

            Initialize();
        }

        // This is initialized when teleporting to a new scene (also when loading the game for the first time)
        public void Initialize()
        {
            if (hintsControllerInitialized)
                return;

            hintsControllerInitialized = true;
            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            InitializeHintsAsync();
        }

        private async void InitializeHintsAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            hintViewPrefab = await addressableProvider.GetAddressable<HintView>(HINT_VIEW_PREFAB_ADDRESSABLE, cancellationTokenSource.Token);
            var hintsContainer = loadingScreenView.GetHintContainer();

            // Initializing empty hints views
            for (var i = 0; i < MAX_HINTS; i++)
            {
                HintView newHintView = Object.Instantiate(hintViewPrefab, hintsContainer, false);
                newHintView.transform.localPosition = Vector3.zero;
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
            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();

            Dictionary<Hint, Texture2D> hintsResult = await hintRequestService.RequestHintsFromSources(ctx, MAX_HINTS);

            int index = 0;
            var intializedHints = new List<HintView>();
            foreach (var hintResult in hintsResult)
            {
                var hintTuple = new Tuple<Hint, Texture2D>(hintResult.Key, hintResult.Value);
                hintsDictionary.Add(index, hintTuple);

                if (index < hintViewPool.Count)
                {
                    hintViewPool[index].Initialize(hintResult.Key, hintResult.Value, FADE_DURATION, index == 0);
                    intializedHints.Add(hintViewPool[index]);
                }
                index++;
            }

            if (loadingScreenV2HintsPanelView != null)
            {
                loadingScreenV2HintsPanelView.Initialize(intializedHints.Count);
                loadingScreenV2HintsPanelView.OnPreviousClicked += CarouselPreviousHint;
                loadingScreenV2HintsPanelView.OnNextClicked += CarouselNextHint;
            }

            hintViewManager = new HintViewManager(intializedHints, SHOWING_TIME_HINTS, loadingScreenV2HintsPanelView);

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
            hintsControllerInitialized = false;

            loadingScreenV2HintsPanelView?.CleanUp();

            if (shortcutLeftInputAction != null)
            {
                shortcutLeftInputAction.OnTriggered -= OnShortcutInputActionTriggered;
            }

            if (shortcutRightInputAction != null)
            {
                shortcutRightInputAction.OnTriggered -= OnShortcutInputActionTriggered;
            }

            if (loadingScreenV2HintsPanelView != null)
            {
                loadingScreenV2HintsPanelView.OnPreviousClicked -= CarouselPreviousHint;
                loadingScreenV2HintsPanelView.OnNextClicked -= CarouselNextHint;
            }

            cancellationTokenSource?.Cancel();
            hintViewManager?.Dispose();

            if (hintViewPool != null)
            {
                foreach (var hint in hintViewPool)
                {
                    if (hint != null)
                        DCL.Helpers.Utils.SafeDestroy(hint.gameObject);
                }
            }

            hintsDictionary = null;
            hintViewPool = null;
        }


#region Shortcut management
        private void ConfigureShortcuts()
        {
            shortcutLeftInputAction = Resources.Load<InputAction_Trigger>("LoadingScreenV2HintsLeft");
            shortcutLeftInputAction.OnTriggered += OnShortcutInputActionTriggered;

            shortcutRightInputAction = Resources.Load<InputAction_Trigger>("LoadingScreenV2HintsRight");
            shortcutRightInputAction.OnTriggered += OnShortcutInputActionTriggered;
        }

        private void OnShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            switch (action)
            {
                case DCLAction_Trigger.LoadingScreenV2HintsLeft:
                    CarouselPreviousHint();
                    break;
                case DCLAction_Trigger.LoadingScreenV2HintsRight:
                    CarouselNextHint();
                    break;
            }
        }
#endregion
    }
}
