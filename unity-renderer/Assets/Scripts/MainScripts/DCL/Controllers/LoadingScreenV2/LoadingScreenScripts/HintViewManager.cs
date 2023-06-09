using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Controllers.LoadingScreenV2
{
    public class HintViewManager : IHintViewManager
    {
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(5);
        private readonly List<HintView> hintViewList;

        private bool isIteratingHints = false;

        internal int currentHintIndex = 0;

        public event Action OnHintChanged;

        public HintViewManager(List<HintView> hintViewList)
        {
            this.hintViewList = hintViewList;
        }

        public void StartCarousel(CancellationToken ct)
        {
            if (isIteratingHints || hintViewList.Count == 0)
                return;

            isIteratingHints = true;
            IterateHintsAsync(ct).Forget();
        }

        public void StopCarousel()
        {
            isIteratingHints = false;
        }

        public void CarouselNextHint(CancellationToken ct)
        {
            if (CarouselNextHint())
                return;

            if (isIteratingHints)
            {
                // Restart the timer
                IterateHintsAsync(ct).Forget();
            }
        }

        private bool CarouselNextHint()
        {
            if (hintViewList.Count == 0)
                return false;

            SetHint((currentHintIndex + 1) % hintViewList.Count);
            return true;
        }

        public void CarouselPreviousHint(CancellationToken ct)
        {
            if (hintViewList.Count == 0)
                return;

            SetHint((currentHintIndex - 1 + hintViewList.Count) % hintViewList.Count);

            if (isIteratingHints)
            {
                // Restart the timer
                IterateHintsAsync(ct).Forget();
            }
        }

        public void SetSpecificHint(int index, CancellationToken ct)
        {
            if (isIteratingHints)
            {
                IterateHintsAsync(ct).Forget();
            }
            hintViewList[currentHintIndex].ShowHint(false);
            currentHintIndex = index;
            UpdateHintView();
        }

        private void SetHint(int index)
        {
            hintViewList[currentHintIndex].ShowHint(false);
            currentHintIndex = index;
            UpdateHintView();
        }

        private async UniTask IterateHintsAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay((int)SHOWING_TIME_HINTS.TotalMilliseconds, cancellationToken: token);
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
            var hintTuple = hintViewList[currentHintIndex];
            hintViewList[currentHintIndex].ShowHint(true);

            OnHintChanged?.Invoke();
        }

        public void Dispose()
        {
            StopCarousel();

            foreach (var hintView in hintViewList)
            {
                Object.Destroy(hintView.gameObject);
            }

            hintViewList.Clear();
        }
    }
}
