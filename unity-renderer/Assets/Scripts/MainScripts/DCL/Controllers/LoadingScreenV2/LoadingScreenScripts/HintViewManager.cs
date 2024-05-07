using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    public class HintViewManager : IHintViewManager
    {
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(5);
        private readonly List<HintView> hintViewList;

        private CancellationTokenSource cancellationTokenSource;

        internal bool isIteratingHints = false;
        internal int currentHintIndex = 0;

        public event Action OnHintChanged;

        public HintViewManager(List<HintView> hintViewList)
        {
            this.hintViewList = hintViewList;
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
            if (hintViewList.Count == 0)
                return;

            SetHint((currentHintIndex + 1) % hintViewList.Count);
        }

        public void CarouselPreviousHint()
        {
            if (hintViewList.Count == 0)
                return;

            SetHint((currentHintIndex - 1 + hintViewList.Count) % hintViewList.Count);
        }

        public void SetSpecificHint(int index)
        {
            if (hintViewList.Count == 0)
                return;

            SetHint(index);
        }

        private void SetHint(int index)
        {
            hintViewList[currentHintIndex].ToggleHint(false);
            currentHintIndex = index;
            UpdateHintView();
        }

        private async UniTask ScheduleNextUpdate(CancellationToken token)
        {
            cancellationTokenSource?.Cancel(); // cancel previous timer
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                await UniTask.Delay(SHOWING_TIME_HINTS, cancellationToken: cancellationTokenSource.Token);
                CarouselNextHint();
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

        private void UpdateHintView()
        {
            hintViewList[currentHintIndex].ToggleHint(true);

            if (isIteratingHints)
            {
                ScheduleNextUpdate(cancellationTokenSource.Token).Forget();
            }

            OnHintChanged?.Invoke();
        }

        public void Dispose()
        {
            StopCarousel();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            foreach (var hintView in hintViewList)
            {
                DCL.Helpers.Utils.SafeDestroy(hintView.gameObject);
            }

            hintViewList.Clear();
        }
    }
}
