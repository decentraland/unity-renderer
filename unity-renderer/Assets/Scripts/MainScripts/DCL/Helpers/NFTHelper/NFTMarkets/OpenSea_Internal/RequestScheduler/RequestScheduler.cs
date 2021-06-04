using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    internal class RequestScheduler
    {
        const float REQUEST_DELAY = 0.4f; // max ~2 requests per second

        public event Action<IRequestHandler> OnRequestReadyToSend;

        private readonly List<IRequestHandler> scheduledRequests = new List<IRequestHandler>();

        private float lastRequestSentTime = 0;
        private Coroutine scheduleTask = null;

        public void EnqueueRequest(IRequestHandler requests)
        {
            requests.schedulableRequestHandler.OnReadyToSchedule -= OnRequestReadyToSchedule;

            if (requests.schedulableRequestHandler.isReadyToSchedule)
            {
                OnRequestReadyToSchedule(requests);
            }
            else
            {
                requests.schedulableRequestHandler.OnReadyToSchedule += OnRequestReadyToSchedule;
            }
        }

        private void OnRequestReadyToSchedule(IRequestHandler requestHandler)
        {
            if (Time.unscaledTime - lastRequestSentTime >= REQUEST_DELAY)
            {
                SendRequest(requestHandler);
                return;
            }

            scheduledRequests.Add(requestHandler);

            if (scheduleTask == null)
            {
                scheduleTask = CoroutineStarter.Start(ScheduleTaskRoutine());
            }
        }

        private void SendRequest(IRequestHandler requestHandler)
        {
            lastRequestSentTime = Time.unscaledTime;
            OnRequestReadyToSend?.Invoke(requestHandler);
        }

        IEnumerator ScheduleTaskRoutine()
        {
            while (scheduledRequests.Count > 0)
            {
                float waitTime = Mathf.Clamp(REQUEST_DELAY - (Time.unscaledTime - lastRequestSentTime), 0, REQUEST_DELAY);
                yield return WaitForSecondsCache.Get(waitTime);
                SendRequest(scheduledRequests[0]);
                scheduledRequests.RemoveAt(0);
            }

            scheduleTask = null;
        }
    }
}