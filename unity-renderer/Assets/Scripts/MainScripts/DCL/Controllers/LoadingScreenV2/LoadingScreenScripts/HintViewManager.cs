using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.LoadingScreen.V2
{
    public class HintViewManager : IHintViewManager
    {
        private readonly List<HintView> hintViewList;
        private CancellationTokenSource cancellationTokenSource;
        private TimeSpan hintShowTime;
        private LoadingScreenV2HintsPanelView loadingScreenV2HintsPanelView;

        internal bool isIteratingHints = false;
        internal int currentHintIndex = 0;

        public event Action OnHintChanged;

        public HintViewManager(List<HintView> hintViewList, TimeSpan hintShowTime, LoadingScreenV2HintsPanelView loadingScreenV2HintsPanelView)
        {
            this.hintViewList = hintViewList;
            this.hintShowTime = hintShowTime;
            this.loadingScreenV2HintsPanelView = loadingScreenV2HintsPanelView;
        }

        public void StartCarousel()
        {
            if (isIteratingHints || hintViewList.Count == 0)
                return;

            isIteratingHints = true;
            ScheduleNextUpdate(CancellationToken.None).Forget();
        }

        public void StopCarousel()
        {
            isIteratingHints = false;
            cancellationTokenSource?.Cancel();
        }

        public void CarouselNextHint()
        {
            StopCarousel();
            SetHint((currentHintIndex + 1) % hintViewList.Count);
        }

        public void CarouselPreviousHint()
        {
            StopCarousel();
            SetHint((currentHintIndex - 1 + hintViewList.Count) % hintViewList.Count);
        }

        public void SetSpecificHint(int index)
        {
            StopCarousel();
            SetHint(index);
        }

        private void SetHint(int index)
        {
            hintViewList[currentHintIndex].ToggleHint(false);
            currentHintIndex = index;
            UpdateHintView();

            // Continue the carousel if it's running.
            if (isIteratingHints)
            {
                ScheduleNextUpdate(CancellationToken.None).Forget();
            }
        }

        private async UniTask ScheduleNextUpdate(CancellationToken token)
        {
            var localCts = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                await UniTask.Delay(hintShowTime, cancellationToken: localCts.Token);
                // Continue with the next hint without stopping the carousel.
                if (hintViewList.Count > 0)
                    SetHint((currentHintIndex + 1) % hintViewList.Count);
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled
            }
            finally
            {
                localCts.Dispose();
            }
        }

        private void UpdateHintView()
        {
            // FD:: maybe here we need to check the cancelation token for the carousel
            // and return if it's canceled

            hintViewList[currentHintIndex].ToggleHint(true);
            loadingScreenV2HintsPanelView.ToggleDot(currentHintIndex);
            OnHintChanged?.Invoke();
        }

        public void Dispose()
        {
            Debug.Log("FD:: LoadingScreenHintsController - Dispose");
            StopCarousel();
            cancellationTokenSource?.Dispose();

            foreach (var hintView in hintViewList)
            {
                hintView.CancelAnyHintToggle();
                DCL.Helpers.Utils.SafeDestroy(hintView.gameObject);
            }

            hintViewList.Clear();
        }
    }
}
