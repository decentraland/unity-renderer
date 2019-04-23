
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL.Interface
{
    /**
     * This class contains the outgoing interface of Decentraland.
     * You must call those functions to interact with the WebInterface.
     *
     * The messages comming from the WebInterface instead, are reported directly to
     * the handler GameObject by name.
     */
    public static class WebInterface
    {
        public static bool VERBOSE = false;
        public static System.Action<string, string> OnMessageFromEngine;

        [System.Serializable]
        private class ReportPositionPayload
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        [System.Serializable]
        private class SceneEvent<T>
        {
            public string sceneId;
            public string eventType;
            public T payload;
        }

        [System.Serializable]
        private class UUIDEvent<TPayload>
            where TPayload : class, new()
        {
            public string uuid;
            public TPayload payload = new TPayload();
        }

        [System.Serializable]
        private class OnClickEvent : UUIDEvent<OnClickEventPayload> { };

        [System.Serializable]
        private class OnTextSubmitEvent : UUIDEvent<OnTextSubmitEventPayload> { };

        [System.Serializable]
        private class OnChangeEvent : UUIDEvent<OnChangeEventPayload> { };

        [System.Serializable]
        private class OnFocusEvent : UUIDEvent<EmptyPayload> { };

        [System.Serializable]
        private class OnBlurEvent : UUIDEvent<EmptyPayload> { };


        [System.Serializable]
        public class OnClickEventPayload
        {
            public int pointerId;
        }

        [System.Serializable]
        public class OnTextSubmitEventPayload
        {
            public string id;
            public string text;
        }

        [System.Serializable]
        public class OnChangeEventPayload
        {
            public object value;
            public int pointerId;
        }

        [System.Serializable]
        public class EmptyPayload
        {
        }

        [System.Serializable]
        private class OnMetricsUpdate
        {
            public SceneMetricsController.Model current = new SceneMetricsController.Model();
            public SceneMetricsController.Model limit = new SceneMetricsController.Model();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    /**
     * This method is called after the first render. It marks the loading of the
     * rest of the JS client.
     */
    [DllImport("__Internal")] public static extern void StartDecentraland();
    [DllImport("__Internal")] public static extern void MessageFromEngine(string type, string message);
#else
        public static void StartDecentraland() =>
          Debug.Log("StartDecentraland called");

        public static void MessageFromEngine(string type, string message)
        {
            if (OnMessageFromEngine != null)
                OnMessageFromEngine.Invoke(type, message);

            if (VERBOSE)
                Debug.Log("MessageFromEngine called with: " + type + ", " + message);
        }
#endif

        public static void SendMessage<T>(string type, T message)
        {
            string messageJson = JsonUtility.ToJson(message);

            if (VERBOSE) Debug.Log($"Sending message: " + messageJson);

            MessageFromEngine(type, messageJson);
        }

        private static ReportPositionPayload positionPayload = new ReportPositionPayload();
        private static OnMetricsUpdate onMetricsUpdate = new OnMetricsUpdate();

        private static OnClickEvent onClickEvent = new OnClickEvent();
        private static OnTextSubmitEvent onTextSubmitEvent = new OnTextSubmitEvent();
        private static OnChangeEvent onChangeEvent = new OnChangeEvent();
        private static OnFocusEvent onFocusEvent = new OnFocusEvent();
        private static OnBlurEvent onBlurEvent = new OnBlurEvent();

        public static void SendSceneEvent<T>(string sceneId, string eventType, T payload)
        {
            SceneEvent<T> sceneEvent = new SceneEvent<T>();
            sceneEvent.sceneId = sceneId;
            sceneEvent.eventType = eventType;
            sceneEvent.payload = payload;

            SendMessage("SceneEvent", sceneEvent);
        }

        public static void ReportPosition(Vector3 position, Quaternion rotation)
        {
            positionPayload.position = position;
            positionPayload.rotation = rotation;

            SendMessage("ReportPosition", positionPayload);
        }

        public static void ReportOnClickEvent(string sceneId, string uuid, int pointerId)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onClickEvent.uuid = uuid;
            onClickEvent.payload.pointerId = pointerId;

            SendSceneEvent(sceneId, "uuidEvent", onClickEvent);
        }

        public static void ReportOnTextSubmitEvent(string sceneId, string uuid, string text)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onTextSubmitEvent.uuid = uuid;
            onTextSubmitEvent.payload.text = text;

            SendSceneEvent(sceneId, "uuidEvent", onTextSubmitEvent);
        }

        public static void ReportOnFocusEvent(string sceneId, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onFocusEvent.uuid = uuid;
            SendSceneEvent(sceneId, "uuidEvent", onFocusEvent);
        }

        public static void ReportOnBlurEvent(string sceneId, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onBlurEvent.uuid = uuid;
            SendSceneEvent(sceneId, "uuidEvent", onBlurEvent);
        }

        public static void ReportOnChangedEvent(string sceneId, string uuid, string text, int pointerId)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onChangeEvent.uuid = uuid;
            onChangeEvent.payload.value = text;
            onChangeEvent.payload.pointerId = pointerId;

            SendSceneEvent(sceneId, "uuidEvent", onChangeEvent);
        }

        public static void ReportOnScrollChange(string sceneId, string uuid, Vector2 value, int pointerId)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onChangeEvent.uuid = uuid;
            onChangeEvent.payload.value = value;
            onChangeEvent.payload.pointerId = pointerId;

            SendSceneEvent(sceneId, "uuidEvent", onChangeEvent);
        }

        public static void ReportEvent<T>(string sceneId, T @event)
        {
            SendSceneEvent(sceneId, "uuidEvent", @event);
        }


        public static void ReportOnMetricsUpdate(string sceneId, SceneMetricsController.Model current, SceneMetricsController.Model limit)
        {
            onMetricsUpdate.current = current;
            onMetricsUpdate.limit = limit;

            SendSceneEvent(sceneId, "metricsUpdate", onMetricsUpdate);
        }
    }
}
