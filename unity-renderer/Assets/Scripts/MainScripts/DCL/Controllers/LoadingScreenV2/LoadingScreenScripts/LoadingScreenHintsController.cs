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
    /// TODO:: FD:: Delete this plugin if not needed anymore
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
            this.loadingScreenV2HintsPanelView = loadingScreenView.GetHintsPanelView();

            ConfigureShortcuts();
            hintsDictionary = new Dictionary<int, Tuple<Hint, Texture2D>>();
            hintViewPool = new List<HintView>();

            InitializeHintsAsync();
        }

        private async void InitializeHintsAsync()
        {
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
            Debug.Log("FD:: LoadingScreenHintsController - CarouselNextHint");
            hintViewManager.CarouselNextHint();
        }

        public void CarouselPreviousHint()
        {
            Debug.Log("FD:: LoadingScreenHintsController - CarouselPreviousHint");
            hintViewManager.CarouselPreviousHint();
        }

        public void SetHint(int index)
        {
            hintViewManager.SetSpecificHint(index);
        }

        public void Dispose()
        {
            shortcutLeftInputAction.OnTriggered -= OnShortcutInputActionTriggered;
            shortcutRightInputAction.OnTriggered -= OnShortcutInputActionTriggered;
            loadingScreenV2HintsPanelView.OnPreviousClicked -= CarouselPreviousHint;
            loadingScreenV2HintsPanelView.OnNextClicked -= CarouselNextHint;

            cancellationTokenSource?.Cancel();
            hintViewManager.Dispose();
        }

#region Shortcut management
        private void ConfigureShortcuts()
        {
            // closeWindow = Resources.Load<InputAction_Trigger>("CloseWindow");
            // closeWindow.OnTriggered += OnCloseWindowPressed;
            //
            // openEmotesCustomizationInputAction = Resources.Load<InputAction_Hold>("DefaultConfirmAction");
            // openEmotesCustomizationInputAction.OnFinished += OnOpenEmotesCustomizationInputActionTriggered;

            shortcutLeftInputAction = Resources.Load<InputAction_Trigger>("LoadingScreenV2HintsLeft");
            shortcutLeftInputAction.OnTriggered += OnShortcutInputActionTriggered;

            shortcutRightInputAction = Resources.Load<InputAction_Trigger>("LoadingScreenV2HintsRight");
            shortcutRightInputAction.OnTriggered += OnShortcutInputActionTriggered;
        }

        private void OnShortcutInputActionTriggered(DCLAction_Trigger action)
        {
            Debug.Log("FD:: LoadingScreenHintsController - OnShortcutInputActionTriggered");
            // if (!shortcutsCanBeUsed)
            //     return;

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
