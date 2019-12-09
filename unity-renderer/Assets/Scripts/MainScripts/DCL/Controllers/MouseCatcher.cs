using DCL.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace DCL
{
    public class MouseCatcher : MonoBehaviour, IPointerDownHandler
    {
        //Default OnPointerEvent
        public LayerMask OnPointerDownTarget = 1 << 9;

        void Update()
        {
#if UNITY_EDITOR
            //Browser is changing this automatically
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
#endif
        }

        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            EventSystem.current.SetSelectedGameObject(null);
        }

        //Externally called by the browser
        public void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            LockCursor();
        }
    }
}