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

        public static System.Action OnResetCamera;
        public static System.Action<float> OnZoomFromUI;
        public static System.Action<string> OnSelectGizmo;
        public static System.Action OnResetObject;
        public static System.Action<string> OnUpdateSceneParcels;
        public static System.Action<DecentralandEntity> OnEntityAdded;
        public static System.Action<DecentralandEntity> OnEntityRemoved;
        public static System.Action<bool> OnPreviewModeChanged;

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

        public void ResetCamera()
        {
            OnResetCamera?.Invoke();
        }

        public void SetPlayMode(string on)
        {
            bool isPreview = false;
            if (bool.TryParse(on, out isPreview))
            {
                SetPlayMode(isPreview);
            }
        }

        public void TakeScreenshot(string mime)
        {
            StartCoroutine(TakeScreenshotRoutine(mime));
        }

        public void UpdateParcelScenes(string sceneJSON)
        {
            OnUpdateSceneParcels?.Invoke(sceneJSON);
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

        private void Awake()
        {
            mouseCatcher = FindObjectOfType<MouseCatcher>();
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

            currentScene = GameObject.FindObjectOfType<ParcelScene>();
            if (currentScene)
            {
                currentScene.OnEntityAdded += (entity) => OnEntityAdded?.Invoke(entity);
                currentScene.OnEntityRemoved += (entity) => OnEntityRemoved?.Invoke(entity);
            }
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderObjectSelector.OnDraggingObjectEnd += OnObjectDragEnd;
                DCLBuilderObjectSelector.OnSelectedObject += OnObjectSelected;
                DCLBuilderObjectSelector.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnded;
                DCLBuilderInput.OnKeyboardButtonDown += OnKeyDown;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectSelector.OnDraggingObjectEnd -= OnObjectDragEnd;
            DCLBuilderObjectSelector.OnSelectedObject -= OnObjectSelected;
            DCLBuilderObjectSelector.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnded;
            DCLBuilderInput.OnKeyboardButtonDown -= OnKeyDown;
        }

        private void OnObjectDragEnd(DecentralandEntity entity, Vector3 position)
        {
            NotifyGizmoEvent(entity, DCLGizmos.Gizmo.NONE);
        }

        private void OnGizmoTransformObjectEnded(DecentralandEntity entity, Vector3 position, string gizmoType)
        {
            NotifyGizmoEvent(entity, gizmoType);
        }

        private void OnObjectSelected(DecentralandEntity entity, string gizmoType)
        {
            WebInterface.ReportGizmoEvent(entity.scene.sceneData.id, entity.entityId, "gizmoSelected", gizmoType);
        }

        private void OnKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Minus:
                case KeyCode.Underscore:
                    OnZoomFromUI?.Invoke(0.5f);
                    break;
                case KeyCode.Plus:
                case KeyCode.Equals:
                    OnZoomFromUI?.Invoke(-0.5f);
                    break;
                case KeyCode.W:
                    OnSelectGizmo?.Invoke(DCLGizmos.Gizmo.MOVE);
                    break;
                case KeyCode.E:
                    OnSelectGizmo?.Invoke(DCLGizmos.Gizmo.ROTATE);
                    break;
                case KeyCode.I:
                    SetPlayMode(!isPreviewMode);
                    break;
            }
        }

        private void NotifyGizmoEvent(DecentralandEntity entity, string gizmoType)
        {
            WebInterface.ReportGizmoEvent(
                entity.scene.sceneData.id,
                entity.entityId,
                "gizmoDragEnded",
                gizmoType,
                entity.gameObject.transform
            );
        }

        private IEnumerator TakeScreenshotRoutine(string id)
        {
            yield return null;

            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            WebInterface.SendScreenshot(System.Convert.ToBase64String(texture.EncodeToPNG()), id);
            Destroy(texture);
        }

        private void SetPlayMode(bool isPreview)
        {
            isPreviewMode = isPreview;
            OnPreviewModeChanged?.Invoke(isPreview);
            DCLCharacterController.i?.SetPosition(defaultCharacterPosition);
            DCLCharacterController.i?.gameObject.SetActive(isPreview);
            if (mouseCatcher != null)
            {
                mouseCatcher.enabled = isPreview;
                if (!isPreview) mouseCatcher.UnlockCursor();
            }
        }

        private void RemoveNoneBuilderGameObjects()
        {
            Component go = FindObjectOfType<HUDController>();
            if (go) Destroy(go.gameObject);
            go = FindObjectOfType<AvatarHUDView>();
            if (go) Destroy(go.gameObject);
            go = FindObjectOfType<MinimapHUDView>();
            if (go) Destroy(go.gameObject);
            go = FindObjectOfType<MinimapMetadataRetriever>();
            if (go && go.transform.parent) Destroy(go.transform.parent.gameObject);
        }
    }
}