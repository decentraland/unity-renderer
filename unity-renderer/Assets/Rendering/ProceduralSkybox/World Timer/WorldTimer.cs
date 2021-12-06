using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace DCL.ServerTime
{
    public class WorldTimer : Singleton<WorldTimer>
    {
        public delegate void OnTimeUpdated(DateTime serverTime);
        public event OnTimeUpdated OnTimeChanged;

        public float serverHitFrequency;
        public string serverURL = "https://worldtimeapi.org/api/timezone/Etc/UTC";

        private bool initialized = false;
        private DateTime lastTimeFromServer = DateTime.Now.ToUniversalTime();
        private DateTime lastTimeFromSystem = DateTime.Now;
        private TimeSpan timeOffset;
        private bool stopTimer = false;
        UnityWebRequest webRequest;

        public WorldTimer() { GetTimeFromServer(); }

        void GetTimeFromServer()
        {
            webRequest = UnityWebRequest.Get(serverURL);
            UnityWebRequestAsyncOperation temp = webRequest.SendWebRequest();
            temp.completed += WebRequestCompleted;
        }

        private void WebRequestCompleted(AsyncOperation obj)
        {
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.InProgress:
                    break;
                case UnityWebRequest.Result.Success:
                    TimerSchema res = JsonUtility.FromJson<TimerSchema>(webRequest.downloadHandler.text);
                    UpdateTimeWithServerTime(res.datetime);
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

    public class TimerSchema
    {
        public string datetime;
    }
}