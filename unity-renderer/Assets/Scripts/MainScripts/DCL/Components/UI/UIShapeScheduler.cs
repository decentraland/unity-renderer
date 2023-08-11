using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace DCL.Components
{
    public class UIShapeScheduler
    {
        private class AlphaChangeRequest
        {
            public CanvasGroup CanvasGroup;
            public float TargetAlpha;
        }

        private Queue<AlphaChangeRequest> queue = new Queue<AlphaChangeRequest>();
        private List<AlphaChangeRequest> instantList = new List<AlphaChangeRequest>();
        private bool isProcessing = false;
        private int batchSize; // How many alpha changes to process in a batch
        private int framesBetweenBatches; // How many frames to wait between batches

        public UIShapeScheduler(int batchSize = 50, int framesBetweenBatches = 1)
        {
            this.batchSize = batchSize;
            this.framesBetweenBatches = framesBetweenBatches;
        }

        public void ScheduleAlphaChange(CanvasGroup canvasGroup, float targetAlpha, bool instant = false)
        {
            var request = new AlphaChangeRequest { CanvasGroup = canvasGroup, TargetAlpha = targetAlpha };

            if (instant)
            {
                instantList.Add(request);
            }
            else
            {
                queue.Enqueue(request);
            }

            if (!isProcessing)
            {
                ProcessQueue().Forget();
            }
        }

        private async UniTaskVoid ProcessQueue()
        {
            isProcessing = true;

            // First process instant list
            foreach (var request in instantList)
            {
                ChangeAlpha(request.CanvasGroup, request.TargetAlpha);
            }
            instantList.Clear();

            while (queue.Count > 0)
            {
                int currentBatchSize = Mathf.Min(batchSize, queue.Count);

                for (int i = 0; i < currentBatchSize; i++)
                {
                    var request = queue.Dequeue();
                    ChangeAlpha(request.CanvasGroup, request.TargetAlpha);
                }

                // Wait for the specified number of frames before processing the next batch
                await UniTask.DelayFrame(framesBetweenBatches);
            }

            isProcessing = false;
        }

        private void ChangeAlpha(CanvasGroup canvasGroup, float targetAlpha)
        {
            canvasGroup.alpha = targetAlpha;
        }
    }
}
