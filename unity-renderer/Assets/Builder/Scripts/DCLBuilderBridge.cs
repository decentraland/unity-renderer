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

        static bool LOG_MESSAGES = false;

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
        public static System.Action<KeyCode> OnSetKeyDown;
        public static event SetGridResolutionDelegate OnSetGridResolution;
        public static System.Action<ParcelScene> OnSceneChanged;
        public static System.Action<string[]> OnBuilderSelectEntity;

        private MouseCatcher mouseCatcher;
        private ParcelScene currentScene;
        private CameraController cameraController;
        private CursorController cursorController;
        private Vector3 defaultCharacterPosition;

        private bool isPreviewMode = false;
        private List<string> outOfBoundariesEntitiesId = new List<string>();
        private int lastEntitiesOutOfBoundariesCount = 0;
        private List<DCLBuilderEntity> selectedEntities;
        private bool entitiesMoved = false;

        private bool isGameObjectActive = false;

        private Coroutine screenshotCoroutine = null;

        private DCLBuilderWebInterface builderWebInterface = new DCLBuilderWebInterface();


        [System.Serializable]
        private class MousePayload
        {
            public string id = string.Empty;
            public float x = 0;
            public float y = 0;
        }

        [System.Serializable]
        private class SetGridResolutionPayload
        {
            public float position = 0;
            public float rotation = 0;
            public float scale = 0;
        }

        [System.Serializable]
        private class SelectedEntitiesPayload
        {
            public string[] entities = null;
        };

        #region "Messages from Explorer"

        public void PreloadFile(string url)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: PreloadFile {url}");
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
                    AssetPromise_PrefetchGLTF gltfPromise = new AssetPromise_PrefetchGLTF(currentScene.contentProvider, file, hash);
                    AssetPromiseKeeper_GLTF.i.Keep(gltfPromise);
                }
            }
        }

        public void GetMousePosition(string newJson)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: GetMousePosition {newJson}");
            MousePayload m = SceneController.i.SafeFromJson<MousePayload>(newJson);

            Vector3 mousePosition = new Vector3(m.x, Screen.height - m.y, 0);
            Vector3 hitPoint;

            if (builderRaycast.RaycastToGround(mousePosition, out hitPoint))
            {
                if (LOG_MESSAGES) Debug.Log($"SEND: ReportMousePosition {m.id} {hitPoint}");
                WebInterface.ReportMousePosition(hitPoint, m.id);
            }
        }

        public void SelectGizmo(string gizmoType)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: SelectGizmo {gizmoType}");
            OnSelectGizmo?.Invoke(gizmoType);
        }

        public void ResetObject()
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: ResetObject");
            OnResetObject?.Invoke();
        }

        public void ZoomDelta(string delta)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: ZoomDelta {delta}");
            float d = 0;
            if (float.TryParse(delta, out d))
            {
                OnZoomFromUI?.Invoke(d);
            }
        }

        public void SetPlayMode(string on)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: SetPlayMode {on}");
            bool isPreview = false;
            if (bool.TryParse(on, out isPreview))
            {
                SetPlayMode(isPreview);
            }
        }

        public void TakeScreenshot(string id)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: TakeScreenshot {id}");
            if (screenshotCoroutine != null)
            {
                StopCoroutine(screenshotCoroutine);
            }
            screenshotCoroutine = StartCoroutine(TakeScreenshotRoutine(id));
        }

        public void ResetBuilderScene()
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: ResetBuilderScene");
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
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: SetBuilderCameraPosition {position}");
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
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: SetBuilderCameraRotation {yawpitchRotation}");
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
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: ResetBuilderCameraZoom");
            OnResetCameraZoom?.Invoke();
        }

        public void SetGridResolution(string payloadJson)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: SetGridResolution {payloadJson}");
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
            KeyCode keyCode;
            if (System.Enum.TryParse(key, false, out keyCode))
            {
                OnSetKeyDown?.Invoke(keyCode);
            }
        }

        public void UnloadBuilderScene(string sceneKey)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: UnloadBuilderScene {sceneKey}");
            SceneController.i?.UnloadScene(sceneKey);
        }

        public void SetSelectedEntities(string msj)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: SelectEntity {msj}");
            SelectedEntitiesPayload payload = JsonUtility.FromJson<SelectedEntitiesPayload>(msj);
            OnBuilderSelectEntity?.Invoke(payload.entities);
        }

        public void GetCameraTargetBuilder(string id)
        {
            if (LOG_MESSAGES) Debug.Log($"RECEIVE: GetCameraTargetBuilder {id}");
            Vector3 targetPosition;
            Camera builderCamera = builderRaycast.builderCamera;
            if (builderRaycast.RaycastToGround(builderCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, builderCamera.nearClipPlane)),
                out targetPosition))
            {
                builderWebInterface.SendCameraTargetPosition(targetPosition, id);
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
            // NOTE: we need to set the quality settings before renderer pipeline is copy and modified
            SetupQualitySettings();
            SetupRendererPipeline();

            cameraController = Object.FindObjectOfType<CameraController>();
            cursorController = Object.FindObjectOfType<CursorController>();
            mouseCatcher = InitialSceneReferences.i?.mouseCatcher;
            var playerAvatarController = Object.FindObjectOfType<PlayerAvatarController>();

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
            if (cursorController)
            {
                cursorController.gameObject.SetActive(false);
            }

            // NOTE: no third person camera in builder yet, so avoid rendering being locked waiting for avatar.
            if (playerAvatarController)
            {
                CommonScriptableObjects.rendererState.RemoveLock(playerAvatarController);
            }

            SceneController.i?.fpsPanel.SetActive(false);
            SetCaptureKeyboardInputEnabled(false);
        }

        private void Start()
        {
            SetCurrentScene();
            builderWebInterface.SendBuilderSceneStart(currentScene.sceneData.id);
        }

        private void Update()
        {
            if (lastEntitiesOutOfBoundariesCount != outOfBoundariesEntitiesId.Count)
            {
                lastEntitiesOutOfBoundariesCount = outOfBoundariesEntitiesId.Count;
                SendOutOfBoundariesEntities();
            }
        }

        private void OnEntityIsAdded(DecentralandEntity entity)
        {
            if (!isPreviewMode)
            {
                var builderEntity = AddBuilderEntityComponent(entity);
                OnEntityAdded?.Invoke(builderEntity);

                entity.OnShapeUpdated += OnEntityShapeUpdated;

                builderWebInterface.SendEntityStartLoad(entity);
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

            builderWebInterface.SendEntityFinishLoad(entity);
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderObjectDragger.OnDraggingObjectEnd += OnObjectDragEnd;
                DCLBuilderObjectDragger.OnDraggingObject += OnObjectDrag;
                DCLBuilderObjectSelector.OnMarkObjectSelected += OnObjectSelected;
                DCLBuilderObjectSelector.OnNoObjectSelected += OnNoObjectSelected;
                DCLBuilderObjectSelector.OnSelectedObjectListChanged += OnSelectionChanged;
                DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnded;
                DCLBuilderGizmoManager.OnGizmoTransformObject += OnGizmoTransformObject;
                DCLBuilderEntity.OnEntityShapeUpdated += ProcessEntityBoundaries;
                DCLBuilderEntity.OnEntityTransformUpdated += ProcessEntityBoundaries;
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectDragger.OnDraggingObjectEnd -= OnObjectDragEnd;
            DCLBuilderObjectDragger.OnDraggingObject -= OnObjectDrag;
            DCLBuilderObjectSelector.OnMarkObjectSelected -= OnObjectSelected;
            DCLBuilderObjectSelector.OnNoObjectSelected -= OnNoObjectSelected;
            DCLBuilderObjectSelector.OnSelectedObjectListChanged -= OnSelectionChanged;
            DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnded;
            DCLBuilderGizmoManager.OnGizmoTransformObject -= OnGizmoTransformObject;
            DCLBuilderEntity.OnEntityShapeUpdated -= ProcessEntityBoundaries;
            DCLBuilderEntity.OnEntityTransformUpdated -= ProcessEntityBoundaries;
            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
        }

        private void OnObjectDragEnd()
        {
            if (selectedEntities != null && entitiesMoved)
            {
                NotifyGizmosTransformEvent(selectedEntities, DCLGizmos.Gizmo.NONE);
            }
            entitiesMoved = false;
        }

        private void OnObjectDrag()
        {
            EvaluateSelectedEntitiesPosition();
            entitiesMoved = true;
        }

        private void OnGizmoTransformObjectEnded(string gizmoType)
        {
            if (selectedEntities != null && entitiesMoved)
            {
                NotifyGizmosTransformEvent(selectedEntities, gizmoType);
            }
            entitiesMoved = false;
        }

        private void OnGizmoTransformObject(string gizmoType)
        {
            EvaluateSelectedEntitiesPosition();
            entitiesMoved = true;
        }

        private void OnObjectSelected(DCLBuilderEntity entity, string gizmoType)
        {
            NotifyGizmosSelectedEvent(entity, gizmoType);
        }

        private void OnNoObjectSelected()
        {
            NotifyGizmosSelectedEvent(null, DCLGizmos.Gizmo.NONE);
        }

        private void OnSelectionChanged(Transform selectionParent, List<DCLBuilderEntity> selectedEntitiesList)
        {
            selectedEntities = selectedEntitiesList;
        }

        private void NotifyGizmosTransformEvent(List<DCLBuilderEntity> entities, string gizmoType)
        {
            builderWebInterface.SendEntitiesTransform(entities, gizmoType, currentScene.sceneData.id);
        }

        private void NotifyGizmosSelectedEvent(DCLBuilderEntity entity, string gizmoType)
        {
            builderWebInterface.SendEntitySelected(entity, gizmoType, currentScene.sceneData.id);
        }

        private IEnumerator TakeScreenshotRoutine(string id)
        {
            yield return new WaitForEndOfFrame();

            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (LOG_MESSAGES) Debug.Log($"SEND: SendScreenshot {id}");
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

            cameraController?.gameObject.SetActive(isPreviewMode);
            cursorController?.gameObject.SetActive(isPreviewMode);

            SetCaptureKeyboardInputEnabled(isPreview);
        }

        private void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
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
                currentScene.metricsController = new DCLBuilderSceneMetricsController(currentScene);
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

            DCL.SceneController.i.boundariesChecker?.EvaluateEntityPosition(entity.rootEntity);
        }

        private void SendOutOfBoundariesEntities()
        {
            builderWebInterface.SendEntitiesOutOfBoundaries(outOfBoundariesEntitiesId.ToArray(), currentScene.sceneData.id);
        }

        private void EvaluateSelectedEntitiesPosition()
        {
            if (selectedEntities != null)
            {
                for (int i = 0; i < selectedEntities.Count; i++)
                {
                    DCL.SceneController.i.boundariesChecker?.EvaluateEntityPosition(selectedEntities[i].rootEntity);
                }
            }
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

        private void SetupQualitySettings()
        {
            DCL.SettingsData.QualitySettings settings = new DCL.SettingsData.QualitySettings()
            {
                textureQuality = DCL.SettingsData.QualitySettings.TextureQuality.FullRes,
                antiAliasing = UnityEngine.Rendering.LWRP.MsaaQuality._2x,
                renderScale = 1,
                shadows = true,
                softShadows = true,
                shadowResolution = UnityEngine.Rendering.LWRP.ShadowResolution._256,
                cameraDrawDistance = 100,
                bloom = true,
                colorGrading = true
            };
            Settings.i.ApplyQualitySettings(settings);
        }
    }
}
