using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

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
            /** Camera position, world space */
            public Vector3 position;

            /** Camera rotation */
            public Quaternion rotation;

            /** Camera height, relative to the feet of the avatar or ground */
            public float playerHeight;

            public Vector3 mousePosition;

            public string id;
        }

        [System.Serializable]
        public abstract class ControlEvent
        {
            public string eventType;
        }

        public abstract class ControlEvent<T> : ControlEvent
        {
            public T payload;

            protected ControlEvent(string eventType, T payload)
            {
                this.eventType = eventType;
                this.payload = payload;
            }
        }

        [System.Serializable]
        public class SceneReady : ControlEvent<SceneReady.Payload>
        {
            [System.Serializable]
            public class Payload
            {
                public string sceneId;
            }

            public SceneReady(string sceneId) : base("SceneReady", new Payload() { sceneId = sceneId }) { }
        }

        [System.Serializable]
        public class ActivateRenderingACK : ControlEvent<object>
        {
            public ActivateRenderingACK() : base("ActivateRenderingACK", null) { }
        }

        [System.Serializable]
        public class SceneEvent<T>
        {
            public string sceneId;
            public string eventType;
            public T payload;
        }

        [System.Serializable]
        public class UUIDEvent<TPayload>
            where TPayload : class, new()
        {
            public string uuid;
            public TPayload payload = new TPayload();
        }

        public enum ACTION_BUTTON
        {
            POINTER,
            PRIMARY,
            SECONDARY,
            ANY
        }

        [System.Serializable]
        public class OnClickEvent : UUIDEvent<OnClickEventPayload>
        {
        };

        [System.Serializable]
        public class OnPointerDownEvent : UUIDEvent<OnPointerEventPayload>
        {
        };

        [System.Serializable]
        public class OnGlobalPointerEvent
        {
            public OnGlobalPointerEventPayload payload = new OnGlobalPointerEventPayload();
        };

        [System.Serializable]
        public class OnPointerUpEvent : UUIDEvent<OnPointerEventPayload>
        {
        };

        [System.Serializable]
        private class OnTextSubmitEvent : UUIDEvent<OnTextSubmitEventPayload>
        {
        };

        [System.Serializable]
        private class OnTextInputChangeEvent : UUIDEvent<OnTextInputChangeEventPayload>
        {
        };

        [System.Serializable]
        private class OnScrollChangeEvent : UUIDEvent<OnScrollChangeEventPayload>
        {
        };

        [System.Serializable]
        private class OnFocusEvent : UUIDEvent<EmptyPayload>
        {
        };

        [System.Serializable]
        private class OnBlurEvent : UUIDEvent<EmptyPayload>
        {
        };

        [System.Serializable]
        public class OnEnterEvent : UUIDEvent<OnEnterEventPayload>
        {
        };

        [System.Serializable]
        public class OnClickEventPayload
        {
        }


        [System.Serializable]
        public class OnPointerEventPayload
        {
            [System.Serializable]
            public class Hit
            {
                public Vector3 origin;
                public float length;
                public Vector3 hitPoint;
                public Vector3 normal;
                public Vector3 worldNormal;
                public string meshName;
                public string entityId;
            }

            public ACTION_BUTTON buttonId;
            public Vector3 origin;
            public Vector3 direction;
            public Hit hit;
        }

        [System.Serializable]
        public class OnGlobalPointerEventPayload : OnPointerEventPayload
        {
            public enum InputEventType
            {
                DOWN,
                UP
            }

            public InputEventType type;
        }

        [System.Serializable]
        public class OnTextSubmitEventPayload
        {
            public string id;
            public string text;
        }

        [System.Serializable]
        public class OnTextInputChangeEventPayload
        {
            public string value;
        }

        [System.Serializable]
        public class OnScrollChangeEventPayload
        {
            public Vector2 value;
            public int pointerId;
        }

        [System.Serializable]
        public class EmptyPayload
        {
        }

        [System.Serializable]
        public class MetricsModel
        {
            public int meshes;
            public int bodies;
            public int materials;
            public int textures;
            public int triangles;
            public int entities;
        }

        [System.Serializable]
        private class OnMetricsUpdate
        {
            public MetricsModel given = new MetricsModel();
            public MetricsModel limit = new MetricsModel();
        }

        [System.Serializable]
        public class OnEnterEventPayload
        {
        }

        [System.Serializable]
        public class TransformPayload
        {
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
        }

        public class OnSendScreenshot
        {
            public string id;
            public string encodedTexture;
        };

        //-----------------------------------------------------
        // Raycast
        [System.Serializable]
        public class RayInfo
        {
            public Vector3 origin;
            public Vector3 direction;
            public float distance;
        }

        [System.Serializable]
        public class RaycastHitInfo
        {
            public bool didHit;
            public RayInfo ray;

            public Vector3 hitPoint;
            public Vector3 hitNormal;
        }

        [System.Serializable]
        public class HitEntityInfo
        {
            public string entityId;
            public string meshName;
        }

        [System.Serializable]
        public class RaycastHitEntity : RaycastHitInfo
        {
            public HitEntityInfo entity;
        }

        [System.Serializable]
        public class RaycastHitEntities : RaycastHitInfo
        {
            public RaycastHitEntity[] entities;
        }

        [System.Serializable]
        public class RaycastResponse<T> where T : RaycastHitInfo
        {
            public string queryId;
            public string queryType;
            public T payload;
        }

        // Note (Zak): We need to explicitly define this classes for the JsonUtility to
        // be able to serialize them
        [System.Serializable]
        public class RaycastHitFirstResponse : RaycastResponse<RaycastHitEntity>
        {
        }

        [System.Serializable]
        public class RaycastHitAllResponse : RaycastResponse<RaycastHitEntities>
        {
        }

        [System.Serializable]
        public class SendExpressionPayload
        {
            public string id;
            public long timestamp;
        }

        [System.Serializable]
        public class UserAcceptedCollectiblesPayload
        {
            public string id;
        }

        [System.Serializable]
        public class SendBlockPlayerPayload
        {
            public string userId;
        }

        [System.Serializable]
        public class SendUnblockPlayerPayload
        {
            public string userId;
        }

        [System.Serializable]
        public class TutorialStepPayload
        {
            public int tutorialStep;
        }

        [System.Serializable]
        public class TermsOfServiceResponsePayload
        {
            public string sceneId;
            public bool dontShowAgain;
            public bool accepted;
        }

        [System.Serializable]
        public class OpenURLPayload
        {
            public string url;
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
            {
                OnMessageFromEngine.Invoke(type, message);
            }

            if (VERBOSE)
            {
                Debug.Log("MessageFromEngine called with: " + type + ", " + message);
            }
        }
#endif

        public static void SendMessage(string type)
        {
            // sending an empty JSON object to be compatible with other messages
            MessageFromEngine(type, "{}");
        }

        public static void SendMessage<T>(string type, T message)
        {
            string messageJson = JsonUtility.ToJson(message);

            if (VERBOSE)
            {
                Debug.Log($"Sending message: " + messageJson);
            }

            MessageFromEngine(type, messageJson);
        }

        private static ReportPositionPayload positionPayload = new ReportPositionPayload();
        private static OnMetricsUpdate onMetricsUpdate = new OnMetricsUpdate();
        private static OnClickEvent onClickEvent = new OnClickEvent();
        private static OnPointerDownEvent onPointerDownEvent = new OnPointerDownEvent();
        private static OnPointerUpEvent onPointerUpEvent = new OnPointerUpEvent();
        private static OnTextSubmitEvent onTextSubmitEvent = new OnTextSubmitEvent();
        private static OnTextInputChangeEvent onTextInputChangeEvent = new OnTextInputChangeEvent();
        private static OnScrollChangeEvent onScrollChangeEvent = new OnScrollChangeEvent();
        private static OnFocusEvent onFocusEvent = new OnFocusEvent();
        private static OnBlurEvent onBlurEvent = new OnBlurEvent();
        private static OnEnterEvent onEnterEvent = new OnEnterEvent();
        private static OnSendScreenshot onSendScreenshot = new OnSendScreenshot();
        private static OnPointerEventPayload onPointerEventPayload = new OnPointerEventPayload();
        private static OnGlobalPointerEventPayload onGlobalPointerEventPayload = new OnGlobalPointerEventPayload();
        private static OnGlobalPointerEvent onGlobalPointerEvent = new OnGlobalPointerEvent();

        public static void SendSceneEvent<T>(string sceneId, string eventType, T payload)
        {
            SceneEvent<T> sceneEvent = new SceneEvent<T>();
            sceneEvent.sceneId = sceneId;
            sceneEvent.eventType = eventType;
            sceneEvent.payload = payload;

            SendMessage("SceneEvent", sceneEvent);
        }

        public static void ReportPosition(Vector3 position, Quaternion rotation, float playerHeight)
        {
            positionPayload.position = position;
            positionPayload.rotation = rotation;
            positionPayload.playerHeight = playerHeight;

            SendMessage("ReportPosition", positionPayload);
        }

        public static void ReportControlEvent<T>(T controlEvent) where T : ControlEvent
        {
            SendMessage("ControlEvent", controlEvent);
        }

        public static void ReportOnClickEvent(string sceneId, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onClickEvent.uuid = uuid;

            SendSceneEvent(sceneId, "uuidEvent", onClickEvent);
        }

        private static void ReportRaycastResult<T, P>(string sceneId, string queryId, string queryType, P payload) where T : RaycastResponse<P>, new() where P : RaycastHitInfo
        {
            T response = new T();
            response.queryId = queryId;
            response.queryType = queryType;
            response.payload = payload;

            SendSceneEvent<T>(sceneId, "raycastResponse", response);
        }

        public static void ReportRaycastHitFirstResult(string sceneId, string queryId, string queryType, RaycastHitEntity payload)
        {
            ReportRaycastResult<RaycastHitFirstResponse, RaycastHitEntity>(sceneId, queryId, queryType, payload);
        }

        public static void ReportRaycastHitAllResult(string sceneId, string queryId, string queryType, RaycastHitEntities payload)
        {
            ReportRaycastResult<RaycastHitAllResponse, RaycastHitEntities>(sceneId, queryId, queryType, payload);
        }

        private static OnPointerEventPayload.Hit CreateHitObject(string entityId, string meshName, Vector3 point, Vector3 normal, float distance)
        {
            OnPointerEventPayload.Hit hit = new OnPointerEventPayload.Hit();

            hit.hitPoint = point;
            hit.length = distance;
            hit.normal = normal;
            hit.worldNormal = normal;
            hit.meshName = meshName;
            hit.entityId = entityId;

            return hit;
        }

        private static void SetPointerEventPayload(OnPointerEventPayload pointerEventPayload, ACTION_BUTTON buttonId, string entityId, string meshName, Ray ray, Vector3 point, Vector3 normal, float distance, bool isHitInfoValid)
        {
            pointerEventPayload.origin = ray.origin;
            pointerEventPayload.direction = ray.direction;
            pointerEventPayload.buttonId = buttonId;

            if (isHitInfoValid)
                pointerEventPayload.hit = CreateHitObject(entityId, meshName, point, normal, distance);
            else
                pointerEventPayload.hit = null;
        }

        public static void ReportGlobalPointerDownEvent(ACTION_BUTTON buttonId, Ray ray, Vector3 point, Vector3 normal, float distance, string sceneId, string entityId = null, string meshName = null, bool isHitInfoValid = false)
        {
            SetPointerEventPayload((OnPointerEventPayload)onGlobalPointerEventPayload, buttonId, entityId, meshName, ray, point, normal, distance, isHitInfoValid);
            onGlobalPointerEventPayload.type = OnGlobalPointerEventPayload.InputEventType.DOWN;

            onGlobalPointerEvent.payload = onGlobalPointerEventPayload;

            SendSceneEvent(sceneId, "pointerEvent", onGlobalPointerEvent);
        }

        public static void ReportGlobalPointerUpEvent(ACTION_BUTTON buttonId, Ray ray, Vector3 point, Vector3 normal, float distance, string sceneId, string entityId = null, string meshName = null, bool isHitInfoValid = false)
        {
            SetPointerEventPayload((OnPointerEventPayload)onGlobalPointerEventPayload, buttonId, entityId, meshName, ray, point, normal, distance, isHitInfoValid);
            onGlobalPointerEventPayload.type = OnGlobalPointerEventPayload.InputEventType.UP;

            onGlobalPointerEvent.payload = onGlobalPointerEventPayload;

            SendSceneEvent(sceneId, "pointerEvent", onGlobalPointerEvent);
        }

        public static void ReportOnPointerDownEvent(ACTION_BUTTON buttonId, string sceneId, string uuid, string entityId, string meshName, Ray ray, Vector3 point, Vector3 normal, float distance)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onPointerDownEvent.uuid = uuid;
            SetPointerEventPayload(onPointerEventPayload, buttonId, entityId, meshName, ray, point, normal, distance, isHitInfoValid: true);
            onPointerDownEvent.payload = onPointerEventPayload;

            SendSceneEvent(sceneId, "uuidEvent", onPointerDownEvent);
        }

        public static void ReportOnPointerUpEvent(ACTION_BUTTON buttonId, string sceneId, string uuid, string entityId, string meshName, Ray ray, Vector3 point, Vector3 normal, float distance, bool isHitInfoValid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onPointerUpEvent.uuid = uuid;
            SetPointerEventPayload(onPointerEventPayload, buttonId, entityId, meshName, ray, point, normal, distance, isHitInfoValid: true);
            onPointerUpEvent.payload = onPointerEventPayload;

            SendSceneEvent(sceneId, "uuidEvent", onPointerUpEvent);
        }

        public static void ReportOnTextSubmitEvent(string sceneId, string uuid, string text)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onTextSubmitEvent.uuid = uuid;
            onTextSubmitEvent.payload.text = text;

            SendSceneEvent(sceneId, "uuidEvent", onTextSubmitEvent);
        }

        public static void ReportOnTextInputChangedEvent(string sceneId, string uuid, string text)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onTextInputChangeEvent.uuid = uuid;
            onTextInputChangeEvent.payload.value = text;

            SendSceneEvent(sceneId, "uuidEvent", onTextInputChangeEvent);
        }

        public static void ReportOnFocusEvent(string sceneId, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onFocusEvent.uuid = uuid;
            SendSceneEvent(sceneId, "uuidEvent", onFocusEvent);
        }

        public static void ReportOnBlurEvent(string sceneId, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onBlurEvent.uuid = uuid;
            SendSceneEvent(sceneId, "uuidEvent", onBlurEvent);
        }

        public static void ReportOnScrollChange(string sceneId, string uuid, Vector2 value, int pointerId)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return;
            }

            onScrollChangeEvent.uuid = uuid;
            onScrollChangeEvent.payload.value = value;
            onScrollChangeEvent.payload.pointerId = pointerId;

            SendSceneEvent(sceneId, "uuidEvent", onScrollChangeEvent);
        }

        public static void ReportEvent<T>(string sceneId, T @event)
        {
            SendSceneEvent(sceneId, "uuidEvent", @event);
        }


        public static void ReportOnMetricsUpdate(string sceneId, MetricsModel current,
            MetricsModel limit)
        {
            onMetricsUpdate.given = current;
            onMetricsUpdate.limit = limit;

            SendSceneEvent(sceneId, "metricsUpdate", onMetricsUpdate);
        }

        public static void ReportOnEnterEvent(string sceneId, string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return;

            onEnterEvent.uuid = uuid;

            SendSceneEvent(sceneId, "uuidEvent", onEnterEvent);
        }

        public static void LogOut()
        {
            SendMessage("LogOut");
        }

        public static void PreloadFinished(string sceneId)
        {
            SendMessage("PreloadFinished", sceneId);
        }

        public static void ReportMousePosition(Vector3 mousePosition, string id)
        {
            positionPayload.mousePosition = mousePosition;
            positionPayload.id = id;
            SendMessage("ReportMousePosition", positionPayload);
        }

        public static void SendScreenshot(string encodedTexture, string id)
        {
            onSendScreenshot.encodedTexture = encodedTexture;
            onSendScreenshot.id = id;
            SendMessage("SendScreenshot", onSendScreenshot);
        }

        public static void ReportEditAvatarClicked()
        {
            SendMessage("EditAvatarClicked");
        }

        [System.Serializable]
        public class SaveAvatarPayload
        {
            public string face;
            public string body;
            public AvatarModel avatar;
        }

        public static void SendSaveAvatar(AvatarModel avatar, Sprite faceSnapshot, Sprite bodySnapshot)
        {
            var payload = new SaveAvatarPayload()
            {
                avatar = avatar,
                face = System.Convert.ToBase64String(faceSnapshot.texture.EncodeToPNG()),
                body = System.Convert.ToBase64String(bodySnapshot.texture.EncodeToPNG())
            };
            SendMessage("SaveUserAvatar", payload);
        }

        public static void SendUserAcceptedCollectibles(string airdropId)
        {
            SendMessage("UserAcceptedCollectibles", new UserAcceptedCollectiblesPayload { id = airdropId });
        }

        public static void SaveUserTutorialStep(int newTutorialStep)
        {
            SendMessage("SaveUserTutorialStep", new TutorialStepPayload() { tutorialStep = newTutorialStep });
        }

        public static void SendPerformanceReport(string encodedFrameTimesInMS)
        {
            MessageFromEngine("PerformanceReport", encodedFrameTimesInMS);
        }

        public static void SendTermsOfServiceResponse(string sceneId, bool accepted, bool dontShowAgain)
        {
            var payload = new TermsOfServiceResponsePayload()
            {
                sceneId = sceneId,
                accepted = accepted,
                dontShowAgain = dontShowAgain
            };
            SendMessage("TermsOfServiceResponse", payload);
        }

        public static void SendExpression(string expressionID, long timestamp)
        {
            SendMessage("TriggerExpression", new SendExpressionPayload()
            {
                id = expressionID,
                timestamp = timestamp
            });
        }

        public static void ReportMotdClicked()
        {
            SendMessage("MotdConfirmClicked");
        }

        public static void OpenURL(string url)
        {
            SendMessage("OpenWebURL", new OpenURLPayload { url = url });
        }

        public static void SendReportScene(string sceneID)
        {
            SendMessage("ReportScene", sceneID);
        }

        public static void SendReportPlayer(string playerName)
        {
            SendMessage("ReportPlayer", playerName);
        }

        public static void SendBlockPlayer(string userId)
        {
            SendMessage("BlockPlayer", new SendBlockPlayerPayload()
            {
                userId = userId
            });
        }

        public static void SendUnlockPlayer(string userId)
        {
            SendMessage("UnblockPlayer", new SendUnblockPlayerPayload()
            {
                userId = userId
            });
        }
    }
}
