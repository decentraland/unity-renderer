using UnityEngine;
using System.Collections;
using DCL;
using DCL.Models;
using DCL.Controllers;
using DCL.Interface;
using DCL.Components;

namespace Builder
{
    public class DCLBuilderBridge : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;

        public delegate void SetGridResolutionDelegate(float position, float rotation, float scale);

        public static System.Action<float> OnZoomFromUI;
        public static System.Action<string> OnSelectGizmo;
        public static System.Action OnResetObject;
        public static System.Action<DCLBuilderEntity> OnEntityAdded;
        public static System.Action<DCLBuilderEntity> OnEntityRemoved;
        public static System.Action<bool> OnPreviewModeChanged;
        public static System.Action OnResetBuilderScene;
        public static System.Action<Vector3> OnSetCameraPosition;
        public static System.Action<float, float> OnSetCameraRotation;
        public static System.Action OnResetCameraZoom;
        public static System.Action<KeyCode> OnSetArrowKeyDown;
        public static event SetGridResolutionDelegate OnSetGridResolution;
        public static System.Action<ParcelScene> OnSceneChanged;

        private MouseCatcher mouseCatcher;
        private ParcelScene currentScene;
        private bool isPreviewMode = false;
        private Vector3 defaultCharacterPosition;

        private bool isGameObjectActive = false;

        [System.Serializable]
        private class MousePayload
        {
            public string id = string.Empty;
            public float x = 0;
            public float y = 0;
        }

        [System.Serializable]
        private class EntityLoadingPayload
        {
            public string type;
            public string entityId;
        }

        [System.Serializable]
        private class OnEntityLoadingEvent : DCL.Interface.WebInterface.UUIDEvent<EntityLoadingPayload>
        {
        };

        [System.Serializable]
        private class SetGridResolutionPayload
        {
            public float position = 0;
            public float rotation = 0;
            public float scale = 0;
        }

        [System.Serializable]
        public class BuilderSceneStartEvent
        {
            public string sceneId;
            public string eventType = "builderSceneStart";
        }

        private static OnEntityLoadingEvent onGetLoadingEntity = new OnEntityLoadingEvent();

        #region "Messages from Explorer"

        public void PreloadFile(string url)
        {
        }

        public void GetMousePosition(string newJson)
        {
            MousePayload m = SceneController.i.SafeFromJson<MousePayload>(newJson);

            Vector3 mousePosition = new Vector3(m.x, Screen.height - m.y, 0);
            Vector3 hitPoint;

            if (builderRaycast.RaycastToGround(mousePosition, out hitPoint))
            {
                WebInterface.ReportMousePosition(hitPoint, m.id);
            }
        }

        public void SelectGizmo(string gizmoType)
        {
            OnSelectGizmo?.Invoke(gizmoType);
        }

        public void ResetObject()
        {
            OnResetObject?.Invoke();
        }

        public void ZoomDelta(string delta)
        {
            float d = 0;
            if (float.TryParse(delta, out d))
            {
                OnZoomFromUI?.Invoke(d);
            }
        }

        public void SetPlayMode(string on)
        {
            bool isPreview = false;
            if (bool.TryParse(on, out isPreview))
            {
                SetPlayMode(isPreview);
            }
        }

        public void TakeScreenshot(string id)
        {
            StartCoroutine(TakeScreenshotRoutine(id));
        }

        public void ResetBuilderScene()
        {
            OnResetBuilderScene?.Invoke();
            DCLCharacterController.i?.gameObject.SetActive(false);

            if (currentScene)
            {
                currentScene.OnEntityAdded -= OnEntityIsAdded;
                currentScene.OnEntityRemoved -= OnEntityIsRemoved;
            }
            SetCurrentScene();
        }

        public void SetBuilderCameraPosition(string position)
        {
            if (!string.IsNullOrEmpty(position))
            {
                string[] splitPositionStr = position.Split(',');
                if (splitPositionStr.Length == 3)
                {
                    float x, y, z = 0;
                    float.TryParse(splitPositionStr[0], out x);
                    float.TryParse(splitPositionStr[1], out y);
                    float.TryParse(splitPositionStr[2], out z);
                    OnSetCameraPosition?.Invoke(new Vector3(x, y, z));
                }
            }
        }

        public void SetBuilderCameraRotation(string yawpitchRotation)
        {
            if (!string.IsNullOrEmpty(yawpitchRotation))
            {
                string[] splitRotationStr = yawpitchRotation.Split(',');
                if (splitRotationStr.Length == 2)
                {
                    float yaw, pitch = 0;
                    float.TryParse(splitRotationStr[0], out yaw);
                    float.TryParse(splitRotationStr[1], out pitch);

                    OnSetCameraRotation?.Invoke(yaw * Mathf.Rad2Deg, pitch * Mathf.Rad2Deg);
                }
            }
        }

        public void ResetBuilderCameraZoom()
        {
            OnResetCameraZoom?.Invoke();
        }

        public void SetGridResolution(string payloadJson)
        {
            try
            {
                SetGridResolutionPayload payload = JsonUtility.FromJson<SetGridResolutionPayload>(payloadJson);
                OnSetGridResolution?.Invoke(payload.position, payload.rotation, payload.scale);
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("Error parsing bBuilder's SetGridResolution Json = " + payloadJson + " " + e.ToString());
            }
        }

        public void SetArrowKeyDown(string key)
        {
            KeyCode arrowKey;
            if (System.Enum.TryParse(key, false, out arrowKey))
            {
                OnSetArrowKeyDown?.Invoke(arrowKey);
            }
        }

        public void UnloadScene(string sceneKey)
        {
            SceneController.i?.UnloadScene(sceneKey);
        }

        #endregion

        public static DecentralandEntity GetEntityFromGameObject(GameObject currentSelected)
        {
            LoadWrapper wrapper = currentSelected.GetComponent<LoadWrapper>();

            if (wrapper?.entity != null)
            {
                return wrapper.entity;
            }
            else
            {
                if (currentSelected.transform.parent != null)
                {
                    return GetEntityFromGameObject(currentSelected.transform.parent.gameObject);
                }

                return null;
            }
        }

        public static ParcelScene GetLoadedScene()
        {
            ParcelScene loadedScene = null;

            if (SceneController.i != null && SceneController.i.loadedScenes.Count > 0)
            {
                using (var iterator = SceneController.i.loadedScenes.GetEnumerator())
                {
                    iterator.MoveNext();
                    loadedScene = iterator.Current.Value;
                }
            }
            return loadedScene;
        }

        private void Awake()
        {
            mouseCatcher = InitialSceneReferences.i?.mouseCatcher;
            if (mouseCatcher != null)
            {
                mouseCatcher.enabled = false;
            }

            if (DCLCharacterController.i)
            {
                defaultCharacterPosition = DCLCharacterController.i.transform.position;
                DCLCharacterController.i.gameObject.SetActive(false);
            }

            //TODO: we need a better way for doing this
            RemoveNoneBuilderGameObjects();

            SceneController.i?.fpsPanel.SetActive(false);
            SetCaptureKeyboardInputEnabled(false);
        }

        private void Start()
        {
            SetCurrentScene();
            WebInterface.SendMessage("SceneEvent", new BuilderSceneStartEvent() { sceneId = currentScene.sceneData.id });
        }

        private void OnEntityIsAdded(DecentralandEntity entity)
        {
            var builderEntity = AddBuilderEntityComponent(entity);
            OnEntityAdded?.Invoke(builderEntity);

            entity.OnShapeUpdated += ClearEntityLoadingState;

            onGetLoadingEntity.uuid = entity.entityId;
            onGetLoadingEntity.payload.entityId = entity.entityId;
            onGetLoadingEntity.payload.type = "onEntityLoading";
            WebInterface.SendSceneEvent(currentScene.sceneData.id, "uuidEvent", onGetLoadingEntity);
        }

        private void OnEntityIsRemoved(DecentralandEntity entity)
        {
            var builderEntity = entity.gameObject.GetComponent<DCLBuilderEntity>();
            if (builderEntity != null)
            {
                OnEntityRemoved?.Invoke(builderEntity);
            }
        }

        private void ClearEntityLoadingState(DecentralandEntity entity)
        {
            entity.OnShapeUpdated -= ClearEntityLoadingState;

            onGetLoadingEntity.uuid = entity.entityId;
            onGetLoadingEntity.payload.entityId = entity.entityId;
            onGetLoadingEntity.payload.type = "onEntityFinishLoading";
            WebInterface.SendSceneEvent(currentScene.sceneData.id, "uuidEvent", onGetLoadingEntity);
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderObjectSelector.OnDraggingObjectEnd += OnObjectDragEnd;
                DCLBuilderObjectSelector.OnSelectedObject += OnObjectSelected;
                DCLBuilderObjectSelector.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnded;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectSelector.OnDraggingObjectEnd -= OnObjectDragEnd;
            DCLBuilderObjectSelector.OnSelectedObject -= OnObjectSelected;
            DCLBuilderObjectSelector.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnded;
        }

        private void OnObjectDragEnd(DCLBuilderEntity entity, Vector3 position)
        {
            NotifyGizmoEvent(entity, DCLGizmos.Gizmo.NONE);
        }

        private void OnGizmoTransformObjectEnded(DCLBuilderEntity entity, Vector3 position, string gizmoType)
        {
            NotifyGizmoEvent(entity, gizmoType);
        }

        private void OnObjectSelected(DCLBuilderEntity entity, string gizmoType)
        {
            WebInterface.ReportGizmoEvent(entity.rootEntity.scene.sceneData.id, entity.rootEntity.entityId, "gizmoSelected", gizmoType);
        }

        private void NotifyGizmoEvent(DCLBuilderEntity entity, string gizmoType)
        {
            WebInterface.ReportGizmoEvent(
                entity.rootEntity.scene.sceneData.id,
                entity.rootEntity.entityId,
                "gizmoDragEnded",
                gizmoType,
                entity.gameObject.transform
            );
        }

        private IEnumerator TakeScreenshotRoutine(string id)
        {
            yield return new WaitForEndOfFrame();

            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            WebInterface.SendScreenshot("data:image/png;base64," + System.Convert.ToBase64String(texture.EncodeToPNG()), id);
            Destroy(texture);
        }

        private void SetPlayMode(bool isPreview)
        {
            isPreviewMode = isPreview;
            OnPreviewModeChanged?.Invoke(isPreview);
            if (DCLCharacterController.i)
            {
                DCLCharacterController.i.SetPosition(defaultCharacterPosition);
                DCLCharacterController.i.gameObject.SetActive(isPreview);
            }
            if (mouseCatcher != null)
            {
                mouseCatcher.enabled = isPreview;
                if (!isPreview) mouseCatcher.UnlockCursor();
            }
            SetCaptureKeyboardInputEnabled(isPreview);
        }

        private void RemoveNoneBuilderGameObjects()
        {
            Component go = FindObjectOfType<HUDController>();
            if (go) Destroy(go.gameObject);
            go = FindObjectOfType<AvatarHUDView>();
            if (go) Destroy(go.gameObject);
            go = FindObjectOfType<MinimapHUDView>();
            if (go) Destroy(go.gameObject);
            go = FindObjectOfType<MinimapMetadataController>();
            if (go && go.transform.parent) Destroy(go.transform.parent.gameObject);
        }

        private void SetCaptureKeyboardInputEnabled(bool value)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = value;
#endif
        }

        private void SetCurrentScene()
        {
            currentScene = GetLoadedScene();
            if (currentScene)
            {
                currentScene.OnEntityAdded += OnEntityIsAdded;
                currentScene.OnEntityRemoved += OnEntityIsRemoved;
                OnSceneChanged?.Invoke(currentScene);
            }
        }

        private DCLBuilderEntity AddBuilderEntityComponent(DecentralandEntity entity)
        {
            DCLBuilderEntity builderComponent = entity.gameObject.GetComponent<DCLBuilderEntity>();
            if (builderComponent == null)
            {
                builderComponent = entity.gameObject.AddComponent<DCLBuilderEntity>();
            }
            builderComponent.SetEntity(entity);
            return builderComponent;
        }
    }
}