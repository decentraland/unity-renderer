using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace DCL.ServerTime
{
    public class WorldTimer : MonoBehaviour
    {
        public delegate void OnTimeUpdated(DateTime serverTime);
        public event OnTimeUpdated OnTimeChanged;

        public static WorldTimer i;
        public float serverHitFrequency;
        public string serverURL = "http://worldtimeapi.org/api/timezone/Etc/UTC";

        private DateTime lastTimeFromServer;
        private DateTime lastTimeFromSystem = DateTime.UtcNow;
        private bool stopTimer = false;

        private void Awake()
        {
            if (i == null)
            {
                i = this;
            }
            else
            {
                DestroyImmediate(this);
                return;
            }
            StartCoroutine(GetTimeFromServer());
        }

        void OnApplicationFocus(bool hasFocus)
        {
            StopAllCoroutines();
            StartCoroutine(GetTimeFromServer());
        }

        IEnumerator GetTimeFromServer()
        {
            while (!stopTimer)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(serverURL))
                {
                    yield return webRequest.SendWebRequest();

                    switch (webRequest.result)
                    {
                        case UnityWebRequest.Result.InProgress:
                            break;
                        case UnityWebRequest.Result.Success:
                            TimerSchema res = JsonUtility.FromJson<TimerSchema>(webRequest.downloadHandler.text);
                            UpdateTimeWithServerTime(res.datetime);
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
                }
                yield return new WaitForSecondsRealtime(serverHitFrequency);
            }

        }

        void UpdateTimeWithServerTime(string datetime)
        {
            DateTime serverTime;
            if (DateTime.TryParse(datetime, out serverTime))
            {
                // Update last server time
                lastTimeFromServer = serverTime;
                // Update current time from the system
                lastTimeFromSystem = DateTime.UtcNow;

                // Fire Event
                OnTimeChanged?.Invoke(lastTimeFromServer);
            }
        }

        public DateTime GetCurrentTime()
        {
            DateTime currentTime = lastTimeFromServer.Add(DateTime.UtcNow - lastTimeFromSystem);
            return currentTime;
        }
    }

    public class TimerSchema
    {
        public string datetime;
    }
}