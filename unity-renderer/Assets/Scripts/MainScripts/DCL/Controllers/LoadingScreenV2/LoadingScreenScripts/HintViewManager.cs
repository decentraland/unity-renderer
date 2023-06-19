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

        internal bool isIteratingHints = false;
        internal int currentHintIndex = 0;

        public event Action OnHintChanged;

        public HintViewManager(List<HintView> hintViewList, TimeSpan hintShowTime)
        {
            this.hintViewList = hintViewList;
            this.hintShowTime = hintShowTime;
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
                await UniTask.Delay(hintShowTime, cancellationToken: cancellationTokenSource.Token);
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
