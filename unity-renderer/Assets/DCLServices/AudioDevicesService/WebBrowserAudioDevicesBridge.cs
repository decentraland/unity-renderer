using System;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL.Services
{
    public class WebBrowserAudioDevicesBridge : MonoBehaviour, IAudioDevicesBridge
    {

        public AudioDevicesResponse AudioDevices { get; private set; }
        public event Action<AudioDevicesResponse> OnAudioDevicesRecieved;

        public static WebBrowserAudioDevicesBridge GetOrCreate()
        {
            GameObject brigeGO = SceneReferences.i?.bridgeGameObject;
            if (SceneReferences.i?.bridgeGameObject == null)
                return new GameObject("Bridge").AddComponent<WebBrowserAudioDevicesBridge>();

            return brigeGO.GetOrCreateComponent<WebBrowserAudioDevicesBridge>();
        }

        [ContextMenu(nameof(RequestAudioDevices))]
        public void RequestAudioDevices()
        {
            Debug.Log(nameof(RequestAudioDevices));
            WebInterface.RequestAudioDevices();
        }

        [PublicAPI]
        public void SetAudioDevices(string payload)
        {
            Debug.Log($"Get devices - {payload}");
            AudioDevicesResponse response = null;

            try
            {
                response = JsonUtility.FromJson<AudioDevicesResponse>(payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"Fail to parse audio devices json {e}");
            }

            if (response == null)
                return;

            AudioDevices = response;
            OnAudioDevicesRecieved?.Invoke(response);
        }
    }
}