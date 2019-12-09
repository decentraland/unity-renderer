using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Models;
using DCL.Controllers;
using DCL.Interface;
using DCL.Components;
using DCL.Helpers;
using DCL.Configuration;
using Builder.Gizmos;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering;

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
        public static System.Action<string> OnBuilderSelectEntity;
        public static System.Action OnBuilderDeselectEntity;

        private MouseCatcher mouseCatcher;
        private ParcelScene currentScene;
        private CameraController cameraController;
        private Vector3 defaultCharacterPosition;

        private bool isPreviewMode = false;
        private List<string> outOfBoundariesEntitiesId = new List<string>();

        private bool isGameObjectActive = false;

        private EntitiesOutOfBoundariesEventPayload outOfBoundariesEventPayload = new EntitiesOutOfBoundariesEventPayload();
        private static OnEntityLoadingEvent onGetLoadingEntity = new OnEntityLoadingEvent();
        private static ReportCameraTargetPosition onReportCameraTarget = new ReportCameraTargetPosition();

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
        private class EntitiesOutOfBoundariesEventPayload
        {
            public string[] entities;
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

        [System.Serializable]
        private class ReportCameraTargetPosition
        {
            public Vector3 cameraTarget;
            public string id;
        }

        #region "Messages from Explorer"

        public void PreloadFile(string url)
        {
            if (currentScene != null)
            {
                string[] split = url.Split('\t');
                string hash = split[0];
                string file = split[1];

                if (!currentScene.contentProvider.fileToHash.ContainsKey(file.ToLower()))
                {
                    currentScene.contentProvider.fileToHash.Add(file.ToLower(), hash);
                }

                if (file.EndsWith(".glb") || file.EndsWith(".gltf"))
                {
                    AssetPromise_PrefetchGLTF gltfPromise = new AssetPromise_PrefetchGLTF(currentScene.contentProvider, file);
                    AssetPromiseKeeper_GLTF.i.Keep(gltfPromise);
                }
            }
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
            outOfBoundariesEntitiesId.Clear();

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

                    if (isPreviewMode)
                    {
                        DCLCharacterController.i?.SetPosition(new Vector3(x, y, z));
                    }
                    else
                    {
                        OnSetCameraPosition?.Invoke(new Vector3(x, y, z));
                    }
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

                    if (isPreviewMode)
                    {
                        if (DCLCharacterController.i != null)
                        {
                            DCLCharacterController.i.transform.rotation = Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f);
                        }
                        if (cameraController)
                        {
                            var cameraRotation = new CameraController.SetRotationPayload()
                            {
                                x = pitch * Mathf.Rad2Deg,
                                y = 0,
                                z = 0
                            };
                            cameraController.SetRotation(JsonUtility.ToJson(cameraRotation));
                        }
                    }
                    else
                    {
                        OnSetCameraRotation?.Invoke(yaw * Mathf.Rad2Deg, pitch * Mathf.Rad2Deg);
                    }
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

        public void OnBuilderKeyDown(string key)
        {
            KeyCode arrowKey;
            if (System.Enum.TryParse(key, false, out arrowKey))
            {
                OnSetArrowKeyDown?.Invoke(arrowKey);
            }
        }

        public void UnloadBuilderScene(string sceneKey)
        {
            SceneController.i?.UnloadScene(sceneKey);
        }

        public void SelectEntity(string entityId)
        {
            OnBuilderSelectEntity?.Invoke(entityId);
        }

        public void DeselectBuilderEntity()
        {
            OnBuilderDeselectEntity?.Invoke();
        }

        public void GetCameraTargetBuilder(string id)
        {
            Vector3 targetPosition;
            Camera builderCamera = builderRaycast.builderCamera;
            if (builderRaycast.RaycastToGround(builderCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, builderCamera.nearClipPlane)),
                out targetPosition))
            {
                onReportCameraTarget.cameraTarget = targetPosition;
                onReportCameraTarget.id = id;
                WebInterface.SendMessage("ReportBuilderCameraTarget", onReportCameraTarget);
            }
        }

        #endregion

        private static ParcelScene GetLoadedScene()
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
            SetupRendererPipeline();

            cameraController = Object.FindObjectOfType<CameraController>();

            mouseCatcher = InitialSceneReferences.i?.mouseCatcher;
            if (mouseCatcher != null)
            {
                mouseCatcher.enabled = false;
            }

            if (DCLCharacterController.i)
            {
                defaultCharacterPosition = DCLCharacterController.i.transform.position;
                DCLCharacterController.i.initialPositionAlreadySet = true;
                DCLCharacterController.i.characterAlwaysEnabled = false;
                DCLCharacterController.i.gameObject.SetActive(false);
            }

            if (cameraController)
            {
                cameraController.gameObject.SetActive(false);
            }

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
            if (!isPreviewMode)
            {
                var builderEntity = AddBuilderEntityComponent(entity);
                OnEntityAdded?.Invoke(builderEntity);

                entity.OnShapeUpdated += OnEntityShapeUpdated;

                onGetLoadingEntity.uuid = entity.entityId;
                onGetLoadingEntity.payload.entityId = entity.entityId;
                onGetLoadingEntity.payload.type = "onEntityLoading";
                WebInterface.SendSceneEvent(currentScene.sceneData.id, "uuidEvent", onGetLoadingEntity);
            }
        }

        private void OnEntityIsRemoved(DecentralandEntity entity)
        {
            var builderEntity = entity.gameObject.GetComponent<DCLBuilderEntity>();
            if (builderEntity != null)
            {
                OnEntityRemoved?.Invoke(builderEntity);
            }
        }

        private void OnEntityShapeUpdated(DecentralandEntity entity)
        {
            entity.OnShapeUpdated -= OnEntityShapeUpdated;

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
                DCLBuilderObjectSelector.OnNoObjectSelected += OnNoObjectSelected;
                DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnded;
                DCLBuilderEntity.OnEntityShapeUpdated += ProcessEntityBoundaries;
                DCLBuilderEntity.OnEntityTransformUpdated += ProcessEntityBoundaries;
                RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectSelector.OnDraggingObjectEnd -= OnObjectDragEnd;
            DCLBuilderObjectSelector.OnSelectedObject -= OnObjectSelected;
            DCLBuilderObjectSelector.OnNoObjectSelected -= OnNoObjectSelected;
            DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnded;
            DCLBuilderEntity.OnEntityShapeUpdated -= ProcessEntityBoundaries;
            DCLBuilderEntity.OnEntityTransformUpdated -= ProcessEntityBoundaries;
            RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
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

        private void OnNoObjectSelected()
        {
            WebInterface.ReportGizmoEvent(currentScene.sceneData.id, null, "gizmoSelected", null);
        }

        private void NotifyGizmoEvent(DCLBuilderEntity entity, string gizmoType)
        {
            WebInterface.ReportGizmoEvent(
                entity.rootEntity.scene.sceneData.id,
                entity ? entity.rootEntity.entityId : "",
                "gizmoDragEnded",
                gizmoType != null ? gizmoType : DCL.Components.DCLGizmos.Gizmo.NONE,
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
                DCLCharacterController.i.ResetGround();
            }
            if (mouseCatcher != null)
            {
                mouseCatcher.enabled = isPreview;
                if (!isPreview) mouseCatcher.UnlockCursor();
            }
            if (cameraController)
            {
                cameraController.gameObject.SetActive(isPreviewMode);
            }
            SetCaptureKeyboardInputEnabled(isPreview);
        }

        private void OnRenderingStateChanged(bool renderingEnabled)
        {
            if (renderingEnabled)
            {
                ParcelSettings.VISUAL_LOADING_ENABLED = false;
            }
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
            DCLBuilderEntity builderComponent = Utils.GetOrCreateComponent<DCLBuilderEntity>(entity.gameObject);
            builderComponent.SetEntity(entity);
            return builderComponent;
        }

        private void ProcessEntityBoundaries(DCLBuilderEntity entity)
        {
            string entityId = entity.rootEntity.entityId;
            int entityIndexInList = outOfBoundariesEntitiesId.IndexOf(entityId);

            bool wasInsideSceneBoundaries = entityIndexInList == -1;
            bool isInsideSceneBoundaries = entity.IsInsideSceneBoundaries();

            if (wasInsideSceneBoundaries && !isInsideSceneBoundaries)
            {
                outOfBoundariesEntitiesId.Add(entityId);
            }
            else if (!wasInsideSceneBoundaries && isInsideSceneBoundaries)
            {
                outOfBoundariesEntitiesId.RemoveAt(entityIndexInList);
            }

            outOfBoundariesEventPayload.entities = outOfBoundariesEntitiesId.ToArray();
            WebInterface.SendSceneEvent<EntitiesOutOfBoundariesEventPayload>(currentScene.sceneData.id, "entitiesOutOfBoundaries", outOfBoundariesEventPayload);
            currentScene.boundariesChecker?.EvaluateEntityPosition(entity.rootEntity);
        }

        private void SetupRendererPipeline()
        {
            LightweightRenderPipelineAsset lwrpa = ScriptableObject.Instantiate(GraphicsSettings.renderPipelineAsset) as LightweightRenderPipelineAsset;

            if (lwrpa != null)
            {
                lwrpa.shadowDepthBias = 3;
                lwrpa.shadowDistance = 80f;
                GraphicsSettings.renderPipelineAsset = lwrpa;
            }
        }
    }
}