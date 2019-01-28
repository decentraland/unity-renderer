
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
        private class ReportPositionPayload
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        private class SceneEvent
        {
            public string sceneId;
            public string eventType; // uuidEvent
            public object payload;
        }

        public class OnClickEventPayload
        {
            public int pointerId;
        }

        private class OnClickEvent
        {
            public string uuid;
            public OnClickEventPayload payload = new OnClickEventPayload();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    /**
     * This method is called after the first render. It marks the loading of the
     * rest of the JS client.
     */
    [DllImport("__Internal")] public static extern void StartDecentraland();
    [DllImport("__Internal")] public static extern void MessageFromEngine(string type, string message);
#else
        public static System.Action<string, string> OnMessageFromEngine;

        public static void StartDecentraland() =>
          Debug.Log("StartDecentraland called");

        public static void MessageFromEngine(string type, string message)
        {
            if (OnMessageFromEngine != null)
                OnMessageFromEngine.Invoke(type, message);
            else
                Debug.Log("MessageFromEngine called with: " + type + ", " + message);
        }
#endif

        public static void SendMessage(string type, object message)
        {
            MessageFromEngine(type, UnityEngine.JsonUtility.ToJson(message));
        }

        private static ReportPositionPayload positionPayload = new ReportPositionPayload();
        private static SceneEvent sceneEvent = new SceneEvent();
        private static OnClickEvent onClickEvent = new OnClickEvent();

        public static void SendSceneEvent(string sceneId, string eventType, object payload)
        {
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
            onClickEvent.uuid = uuid;
            onClickEvent.payload.pointerId = pointerId;

            SendSceneEvent(sceneId, "uuidEvent", onClickEvent);
        }
    }
}
