using System;
using System.Collections.Generic;

namespace UnityGLTF
{
    public class DownloadQueueHandler
    {
        private readonly HashSet<IDownloadQueueElement> queuedElements = new HashSet<IDownloadQueueElement>();

        private int maxDownloadCount;
        private Func<int> GetCurrentDownloadAmount;

        public IDownloadQueueElement nextToDownload { private set; get; } = null;

        public DownloadQueueHandler(int maxDownloadCount, Func<int> GetCurrentDownloadAmount)
        {
            this.maxDownloadCount = maxDownloadCount;
            this.GetCurrentDownloadAmount = GetCurrentDownloadAmount;
        }

        public void Queue(IDownloadQueueElement element)
        {
            if (element.ShouldForceDownload())
                return;

            queuedElements.Add(element);

            if (ShouldRefreshSorting())
            {
                RefreshSorting();
            }
        }

        public void Dequeue(IDownloadQueueElement element)
        {
            if (element == nextToDownload)
            {
                nextToDownload = null;
            }

            if (queuedElements.Remove(element))
            {
                if (ShouldRefreshSorting())
                {
                    RefreshSorting();
                }
            }
        }

        public bool CanDownload(IDownloadQueueElement element)
        {
            if (element.ShouldForceDownload())
                return true;

            if (ShouldRefreshSorting())
            {
                RefreshSorting();
            }

            return nextToDownload == element;
        }

        private bool ShouldRefreshSorting()
        {
            return nextToDownload == null && GetCurrentDownloadAmount() < maxDownloadCount;
        }

        private void RefreshSorting()
        {
            if (queuedElements.Count == 0)
                return;

            using (var iterator = queuedElements.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current == null)
                        continue;

                    if (nextToDownload == null)
                    {
                        nextToDownload = iterator.Current;
                        if (nextToDownload.ShouldPrioritizeDownload())
                            break;

                        continue;
                    }

                    if (iterator.Current.ShouldPrioritizeDownload())
                    {
                        nextToDownload = iterator.Current;
                        break;
                    }

                    if (iterator.Current.GetSqrDistance() < nextToDownload.GetSqrDistance())
                        nextToDownload = iterator.Current;
                }
            }
        }
    }
}