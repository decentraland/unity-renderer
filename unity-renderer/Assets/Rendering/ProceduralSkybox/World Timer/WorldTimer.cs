using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace DCL.ServerTime
{
    public class WorldTimer
    {
        public delegate void OnTimeUpdated(DateTime serverTime);
        public event OnTimeUpdated OnTimeChanged;

        public float serverHitFrequency;
        public string serverURL = "https://peer.decentraland.org/lambdas/health";

        private bool initialized = false;
        private DateTime lastTimeFromServer = DateTime.Now.ToUniversalTime();
        private DateTime lastTimeFromSystem = DateTime.Now;
        private TimeSpan timeOffset;
        private bool stopTimer = false;
        UnityWebRequest webRequest;

        public WorldTimer() { GetTimeFromServer(); }

        void GetTimeFromServer()
        {
            try
            {
                webRequest = UnityWebRequest.Get(serverURL);
                UnityWebRequestAsyncOperation temp = webRequest.SendWebRequest();
                temp.completed += WebRequestCompleted;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void WebRequestCompleted(AsyncOperation obj)
        {
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.InProgress:
                    break;
                case UnityWebRequest.Result.Success:
                    string responseHeaderTime = webRequest.GetResponseHeader("date");
                    UpdateTimeWithServerTime(responseHeaderTime);
                    initialized = true;

                    break;
                case UnityWebRequest.Result.ConnectionError:
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    break;
                default:
                    break;
            }

            webRequest.Dispose();
        }

        void UpdateTimeWithServerTime(string datetime)
        {
            DateTime serverTime;

            if (DateTime.TryParse(datetime, out serverTime))
            {
                // Update last server time
                lastTimeFromServer = serverTime.ToUniversalTime();
                // Update current time from the system
                lastTimeFromSystem = DateTime.Now;

                // Fire Event
                OnTimeChanged?.Invoke(lastTimeFromServer);
            }
        }

        public DateTime GetCurrentTime()
        {
            TimeSpan systemTimeOffset = DateTime.Now - lastTimeFromSystem;

            return lastTimeFromServer.ToUniversalTime().Add(systemTimeOffset);
        }
    }
}