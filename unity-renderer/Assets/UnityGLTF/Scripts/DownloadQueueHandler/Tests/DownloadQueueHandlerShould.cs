using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityGLTF.Tests
{
    public class DownloadQueueHandlerShould
    {
        [Test]
        public void SortCorrectly()
        {
            const int maxDownloads = 1;
            bool processQueue = false;

            Vector3 characterPosition = new Vector3(2, 0, 0);

            IDownloadQueueElement element1 = Substitute.For<IDownloadQueueElement>();
            element1.ShouldPrioritizeDownload().Returns(false);
            element1.GetSqrDistance().Returns(Vector3.Distance(new Vector3(1, 0, 0), characterPosition));

            IDownloadQueueElement element2 = Substitute.For<IDownloadQueueElement>();
            element2.ShouldPrioritizeDownload().Returns(false);
            element2.GetSqrDistance().Returns(Vector3.Distance(new Vector3(2, 0, 0), characterPosition));

            IDownloadQueueElement element3 = Substitute.For<IDownloadQueueElement>();
            element3.ShouldPrioritizeDownload().Returns(false);
            element3.GetSqrDistance().Returns(Vector3.Distance(new Vector3(4, 0, 0), characterPosition));

            DownloadQueueHandler downloadQueueHandler = new DownloadQueueHandler(maxDownloads, () => processQueue ? 0 : maxDownloads);
            downloadQueueHandler.Queue(element1);
            downloadQueueHandler.Queue(element2);

            processQueue = true;

            downloadQueueHandler.Queue(element3);

            Assert.AreEqual(element2, downloadQueueHandler.nextToDownload);
            downloadQueueHandler.Dequeue(element2);

            Assert.AreEqual(element1, downloadQueueHandler.nextToDownload);
            downloadQueueHandler.Dequeue(element1);

            Assert.AreEqual(element3, downloadQueueHandler.nextToDownload);
            downloadQueueHandler.Dequeue(element3);
        }

        [Test]
        public void ApplyPrioritizeDownloadCorrectly()
        {
            const int maxDownloads = 1;
            bool processQueue = false;

            Vector3 characterPosition = new Vector3(2, 0, 0);

            IDownloadQueueElement element1 = Substitute.For<IDownloadQueueElement>();
            element1.ShouldPrioritizeDownload().Returns(false);
            element1.GetSqrDistance().Returns(Vector3.Distance(new Vector3(1, 0, 0), characterPosition));

            IDownloadQueueElement element2 = Substitute.For<IDownloadQueueElement>();
            element2.ShouldPrioritizeDownload().Returns(false);
            element2.GetSqrDistance().Returns(Vector3.Distance(new Vector3(2, 0, 0), characterPosition));

            IDownloadQueueElement prioritizedElement = Substitute.For<IDownloadQueueElement>();
            prioritizedElement.ShouldPrioritizeDownload().Returns(true);
            prioritizedElement.GetSqrDistance().Returns(Vector3.Distance(new Vector3(4, 0, 0), characterPosition));

            DownloadQueueHandler downloadQueueHandler = new DownloadQueueHandler(maxDownloads, () => processQueue ? 0 : maxDownloads);
            downloadQueueHandler.Queue(element1);
            downloadQueueHandler.Queue(element2);

            processQueue = true;

            downloadQueueHandler.Queue(prioritizedElement);

            Assert.AreEqual(prioritizedElement, downloadQueueHandler.nextToDownload);
            downloadQueueHandler.Dequeue(prioritizedElement);

            Assert.AreEqual(element2, downloadQueueHandler.nextToDownload);
            downloadQueueHandler.Dequeue(element2);

            Assert.AreEqual(element1, downloadQueueHandler.nextToDownload);
            downloadQueueHandler.Dequeue(element1);
        }

        [Test]
        public void ApplyForceDownloadCorrectly()
        {
            Vector3 characterPosition = new Vector3(2, 0, 0);

            IDownloadQueueElement element1 = Substitute.For<IDownloadQueueElement>();
            element1.ShouldForceDownload().Returns(false);
            element1.GetSqrDistance().Returns(Vector3.Distance(new Vector3(1, 0, 0), characterPosition));

            IDownloadQueueElement element2 = Substitute.For<IDownloadQueueElement>();
            element2.ShouldForceDownload().Returns(false);
            element2.GetSqrDistance().Returns(Vector3.Distance(new Vector3(2, 0, 0), characterPosition));

            IDownloadQueueElement forcedElement = Substitute.For<IDownloadQueueElement>();
            forcedElement.ShouldForceDownload().Returns(true);
            forcedElement.GetSqrDistance().Returns(Vector3.Distance(new Vector3(4, 0, 0), characterPosition));

            DownloadQueueHandler downloadQueueHandler = new DownloadQueueHandler(0, () => 0);
            downloadQueueHandler.Queue(element1);
            downloadQueueHandler.Queue(element2);
            downloadQueueHandler.Queue(forcedElement);

            Assert.IsFalse(downloadQueueHandler.CanDownload(element1));
            Assert.IsFalse(downloadQueueHandler.CanDownload(element2));
            Assert.IsTrue(downloadQueueHandler.CanDownload(forcedElement));
        }

        [Test]
        public void DownloadFlowGoesCorrectly()
        {
            List<IDownloadQueueElement> downloadedElements = new List<IDownloadQueueElement>();

            Vector3 characterPosition = new Vector3(2, 0, 0);

            IDownloadQueueElement element1 = Substitute.For<IDownloadQueueElement>();
            element1.ShouldPrioritizeDownload().Returns(false);
            element1.GetSqrDistance().Returns(Vector3.Distance(new Vector3(1, 0, 0), characterPosition));

            IDownloadQueueElement element2 = Substitute.For<IDownloadQueueElement>();
            element2.ShouldPrioritizeDownload().Returns(false);
            element2.GetSqrDistance().Returns(Vector3.Distance(new Vector3(2, 0, 0), characterPosition));

            IDownloadQueueElement element3 = Substitute.For<IDownloadQueueElement>();
            element3.ShouldPrioritizeDownload().Returns(false);
            element3.GetSqrDistance().Returns(Vector3.Distance(new Vector3(4, 0, 0), characterPosition));

            DownloadQueueHandler downloadQueueHandler = new DownloadQueueHandler(1, () => 0);
            downloadQueueHandler.Queue(element1);
            downloadQueueHandler.Queue(element2);
            downloadQueueHandler.Queue(element3);

            const int elementCounts = 3;
            int step = 0;

            while (step < elementCounts)
            {
                foreach (var element in new[] { element3, element2, element1 })
                {
                    if (downloadQueueHandler.CanDownload(element))
                    {
                        downloadedElements.Add(element);
                        downloadQueueHandler.Dequeue(element);
                        break;
                    }
                }
                step++;
            }

            Assert.AreEqual(3, downloadedElements.Count);
            Assert.AreNotEqual(downloadedElements[0], downloadedElements[1]);
            Assert.AreNotEqual(downloadedElements[1], downloadedElements[2]);
            Assert.AreNotEqual(downloadedElements[2], downloadedElements[0]);
        }
    }
}