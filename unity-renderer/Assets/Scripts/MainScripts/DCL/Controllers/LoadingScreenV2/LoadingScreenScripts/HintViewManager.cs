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
        private TimeSpan hintShowTime;
        private LoadingScreenV2HintsPanelView loadingScreenV2HintsPanelView;
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
            if (isIteratingHints || hintViewList.Count == 0)
                return;

            isIteratingHints = true;
            ScheduleNextUpdate(CancellationToken.None).Forget();
        }

        public async void StopCarousel()
        {
            if (!isIteratingHints)
                return;

            isIteratingHints = false;

            if (scheduledUpdateCtxSource != null)
            {
                scheduledUpdateCtxSource.Cancel();

                // Wait for the token to fully cancel before disposing
                while (!scheduledUpdateCtxSource.IsCancellationRequested)
                    await UniTask.Yield();

                scheduledUpdateCtxSource.Dispose();
                scheduledUpdateCtxSource = null;
            }
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
            scheduledUpdateCtxSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                await UniTask.Delay(hintShowTime, cancellationToken: scheduledUpdateCtxSource.Token);
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
                scheduledUpdateCtxSource.Dispose();
            }
        }

        private void UpdateHintView()
        {
            hintViewList[currentHintIndex].ToggleHint(true);
            loadingScreenV2HintsPanelView.ToggleDot(currentHintIndex);
            OnHintChanged?.Invoke();
        }

        public async void Dispose()
        {
            StopCarousel();

            // Wait for any cancellation to finish
            while (scheduledUpdateCtxSource != null)
                await UniTask.Yield();

            foreach (var hintView in hintViewList)
            {
                hintView.CancelAnyHintToggle();
            }

            hintViewList.Clear();
        }

    }
}
