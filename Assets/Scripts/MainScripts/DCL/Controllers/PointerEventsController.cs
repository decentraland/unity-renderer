using DCL.Components;
using UnityEngine;
using DCL.Interface;

namespace DCL
{
    public class PointerEventsController
    {
        private static PointerEventsController instance = null;

        public static PointerEventsController i
        {
            get
            {
                if (instance == null)
                    instance = new PointerEventsController();

                return instance;
            }
        }

        //Default OnPointerEvent
        private LayerMask layerMaskTarget = 1 << 9;

        public static bool renderingIsDisabled = true;
        private Camera characterCamera;
        private OnPointerUpComponent pointerUpEvent;

        private PointerEventsController()
        {
        }

        public void Initialize()
        {
            InputController.i.AddListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController.i.AddListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController.i.AddListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);

            RetrieveCharacterCameraIfNull();
        }

        private void RetrieveCharacterCameraIfNull()
        {
            if (characterCamera == null)
            {
                if (DCLCharacterController.i == null)
                {
                    return;
                }

                characterCamera = DCLCharacterController.i.GetComponentInChildren<Camera>();
            }
        }

        void OnButtonEvent(WebInterface.ACTION_BUTTON buttonId, InputController.EVENT evt, bool useRaycast)
        {
            if (Cursor.lockState != CursorLockMode.None && !renderingIsDisabled)
            {
                Ray ray;
                RaycastHit hitInfo;

                if (evt == InputController.EVENT.BUTTON_DOWN)
                {
                    // Raycast for pointer event components
                    if (useRaycast && Raycast(out ray, out hitInfo, layerMaskTarget))
                    {
                        GameObject go = hitInfo.rigidbody.gameObject;

                        go.GetComponentInChildren<OnClickComponent>()?.Report(buttonId);
                        go.GetComponentInChildren<OnPointerDownComponent>()?.Report(buttonId, ray, hitInfo);

                        pointerUpEvent = go.GetComponentInChildren<OnPointerUpComponent>();
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    if (!string.IsNullOrEmpty(sceneId))
                    {
                        // Raycast for global pointer events
                        if (useRaycast && CheckRaycastGlobal(out ray, out hitInfo, out var info))
                            DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(buttonId, ray, hitInfo, sceneId, info.entityId, info.meshName, isHitInfoValid: true);
                        else
                            DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(buttonId, GetRayFromScreenCenter(), new RaycastHit(), sceneId);
                    }
                }
                else if (evt == InputController.EVENT.BUTTON_UP)
                {
                    // Raycast for pointer event components
                    if (pointerUpEvent != null)
                    {
                        bool isHitInfoValid = Raycast(out ray, out hitInfo, layerMaskTarget);
                        pointerUpEvent.Report(buttonId, ray, hitInfo, isHitInfoValid);

                        pointerUpEvent = null;
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    if (!string.IsNullOrEmpty(sceneId))
                    {
                        // Raycast for global pointer events
                        if (useRaycast && CheckRaycastGlobal(out ray, out hitInfo, out var info))
                            DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(buttonId, ray, hitInfo, sceneId, info.entityId, info.meshName, isHitInfoValid: true);
                        else
                            DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(buttonId, GetRayFromScreenCenter(), new RaycastHit(), sceneId);
                    }
                }
            }
        }

        bool CheckRaycastGlobal(out Ray ray, out RaycastHit hitInfo, out ColliderInfo info)
        {
            bool isRaycastValidWithinScene = false;

            // Raycast everything but the onPointerEventTarget for the global pointer event
            if (Raycast(out ray, out hitInfo, ~layerMaskTarget))
            {
                if (CollidersManager.i.GetInfo(hitInfo.collider, out info))
                {
                    // We only report events for entities inside current scene
                    if (SceneController.i.IsCharacterInsideScene(info.sceneId))
                        isRaycastValidWithinScene = true;
                }
            }
            else
                info = new ColliderInfo();

            return isRaycastValidWithinScene;
        }

        bool Raycast(out Ray ray, out RaycastHit hitInfo, LayerMask layerMask)
        {
            RetrieveCharacterCameraIfNull();

            ray = GetRayFromScreenCenter();
            return Physics.Raycast(ray, out hitInfo, characterCamera.farClipPlane, layerMask);
        }

        Ray GetRayFromScreenCenter()
        {
            return characterCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
    }
}