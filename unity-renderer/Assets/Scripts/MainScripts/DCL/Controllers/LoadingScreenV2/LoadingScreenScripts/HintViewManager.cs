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
        private readonly TimeSpan hintShowTime;
        private readonly LoadingScreenV2HintsPanelView loadingScreenV2HintsPanelView;
        private CancellationTokenSource scheduledUpdateCtxSource;

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
            loadingScreenV2HintsPanelView.TogglePanelAsync(true);
            if (isIteratingHints || hintViewList.Count == 0)
                return;

            isIteratingHints = true;
            ScheduleNextUpdate(CancellationToken.None).Forget();
        }

        public void StopCarousel()
        {
            if (!isIteratingHints)
                return;

            isIteratingHints = false;

            var previousTokenSource = Interlocked.Exchange(ref scheduledUpdateCtxSource, null);

            previousTokenSource?.Cancel();
            previousTokenSource?.Dispose();
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
            CancellationTokenSource newTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            // Interlocked exchange to ensure only one CTS at a time
            var previousTokenSource = Interlocked.Exchange(ref scheduledUpdateCtxSource, newTokenSource);

            previousTokenSource?.Dispose();

            try
            {
                await UniTask.Delay(hintShowTime, cancellationToken: newTokenSource.Token);
                // Continue with the next hint without stopping the carousel.
                if (hintViewList.Count > 0)
                    SetHint((currentHintIndex + 1) % hintViewList.Count);
            }
            catch (OperationCanceledException) { }
        }

        private void UpdateHintView()
        {
            hintViewList[currentHintIndex].ToggleHint(true);
            loadingScreenV2HintsPanelView.ToggleDot(currentHintIndex);
            OnHintChanged?.Invoke();
        }

        public void Dispose()
        {
            StopCarousel();

            foreach (var hintView in hintViewList)
            {
                hintView.CancelAnyHintToggle();
            }

            hintViewList.Clear();
        }
    }
}
