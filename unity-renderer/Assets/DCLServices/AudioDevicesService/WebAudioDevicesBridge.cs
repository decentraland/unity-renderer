using System;
using DCL.Helpers;
using JetBrains.Annotations;
using UnityEngine;

namespace DCL.Services
{
    public class WebAudioDevicesBridge : MonoBehaviour, IAudioDevicesBridge
    {

        public AudioDevicesResponse AudioDevices { get; private set; }
        public event Action<AudioDevicesResponse> OnAudioDevicesRecieved;

        public static WebAudioDevicesBridge GetOrCreate()
        {
            GameObject brigeGO = SceneReferences.i?.bridgeGameObject;
            if (SceneReferences.i?.bridgeGameObject == null)
                return new GameObject("Bridge").AddComponent<WebAudioDevicesBridge>();

            return brigeGO.GetOrCreateComponent<WebAudioDevicesBridge>();
        }

        [PublicAPI]
        public void AddAudioDevices(string payload)
        {
            Debug.Log("[1] GETTING AUDIO DEVICES: " + payload.Length);

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