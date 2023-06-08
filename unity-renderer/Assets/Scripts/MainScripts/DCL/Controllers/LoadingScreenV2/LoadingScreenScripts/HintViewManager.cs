using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Controllers.LoadingScreenV2
{
    public class HintViewManager: IHintViewManager
    {
        private readonly TimeSpan SHOWING_TIME_HINTS = TimeSpan.FromSeconds(5);
        private const int MAX_HINTS = 15;

        internal int currentHintIndex = 0;
        private List<HintView> hintViewList;
        private bool isIteratingHints = false;
        internal CancellationToken token;

        public event Action OnHintChanged;

        public HintViewManager(List<HintView> hintViewList)
        {
            this.hintViewList = hintViewList;
        }

        public void StartCarousel(CancellationToken ct)
        {
            if (isIteratingHints || hintViewList.Count == 0)
                return;

            token = ct;
            isIteratingHints = true;
            IterateHintsAsync(ct).Forget();
        }

        public void StopCarousel()
        {
            isIteratingHints = false;
        }

        public void CarouselNextHint()
        {
            if (!isIteratingHints || hintViewList.Count == 0)
                return;

            SetHint((currentHintIndex + 1) % hintViewList.Count);
        }

        public void CarouselPreviousHint()
        {
            if (!isIteratingHints || hintViewList.Count == 0)
                return;

            SetHint((currentHintIndex - 1 + hintViewList.Count) % hintViewList.Count);
        }

        public void SetHint(int index)
        {
            hintViewList[currentHintIndex].ShowHint(false);
            currentHintIndex = index;
            UpdateHintView();
        }

        private async UniTask IterateHintsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // CarouselNextHint();
                try
                {
                    await UniTask.Delay((int)SHOWING_TIME_HINTS.TotalMilliseconds, cancellationToken: token);
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


