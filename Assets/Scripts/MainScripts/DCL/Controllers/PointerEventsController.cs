using DCL.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace DCL
{
    public class PointerEventsController : MonoBehaviour
    {
        //Default OnPointerEvent
        public LayerMask OnPointerDownTarget = 1 << 9;
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
            if (Cursor.lockState != CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Raycast(out var ray, out var hitInfo))
                    {
                        GameObject go = hitInfo.rigidbody.gameObject;

                        go.GetComponentInChildren<OnClickComponent>()?.Report();
                        go.GetComponentInChildren<OnPointerDownComponent>()?.Report(ray, hitInfo);

                        pointerUpEvent = go.GetComponentInChildren<OnPointerUpComponent>();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (pointerUpEvent != null)
                    {
                        bool isHitInfoValid = Raycast(out var ray, out var hitInfo);
                        pointerUpEvent.Report(ray, hitInfo, isHitInfoValid);

                        pointerUpEvent = null;
                    }

                }
            }
        }

        bool Raycast(out Ray ray, out RaycastHit hitInfo)
        {
            RetrieveCharacterCameraIfNull();

            ray = characterCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            return Physics.Raycast(ray, out hitInfo, characterCamera.farClipPlane, OnPointerDownTarget);
        }
    }
}