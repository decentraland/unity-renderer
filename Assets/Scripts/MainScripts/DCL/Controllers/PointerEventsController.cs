using DCL.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace DCL
{
    public class PointerEventsController : MonoBehaviour
    {
        //Default OnPointerEvent
        public LayerMask layerMaskTarget = 1 << 9;

        public static bool renderingIsDisabled = true;
        private Camera characterCamera;
        private OnPointerUpComponent pointerUpEvent;

        private void OnEnable()
        {
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

        void Update()
        {
            if (Cursor.lockState != CursorLockMode.None && !renderingIsDisabled)
            {
                Ray ray;
                RaycastHit hitInfo;

                if (Input.GetMouseButtonDown(0))
                {
                    // Raycast for pointer event components
                    if (Raycast(out ray, out hitInfo, layerMaskTarget))
                    {
                        GameObject go = hitInfo.rigidbody.gameObject;

                        go.GetComponentInChildren<OnClickComponent>()?.Report();
                        go.GetComponentInChildren<OnPointerDownComponent>()?.Report(ray, hitInfo);

                        pointerUpEvent = go.GetComponentInChildren<OnPointerUpComponent>();
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    // Raycast for global pointer events
                    if (CheckRaycastGlobal(out ray, out hitInfo, out var info))
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(ray, hitInfo, sceneId, info.entityId, info.meshName, isHitInfoValid: true);
                    else
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(ray, hitInfo, sceneId);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    // Raycast for pointer event components
                    if (pointerUpEvent != null)
                    {
                        bool isHitInfoValid = Raycast(out ray, out hitInfo, layerMaskTarget);
                        pointerUpEvent.Report(ray, hitInfo, isHitInfoValid);

                        pointerUpEvent = null;
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    // Raycast for global pointer events
                    if (CheckRaycastGlobal(out ray, out hitInfo, out var info))
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(ray, hitInfo, sceneId, info.entityId, info.meshName, isHitInfoValid: true);
                    else
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(ray, hitInfo, sceneId);
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

            ray = characterCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            return Physics.Raycast(ray, out hitInfo, characterCamera.farClipPlane, layerMask);
        }
    }
}