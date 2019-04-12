using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL
{
    public class MouseCatcher : MonoBehaviour, IPointerDownHandler
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
        }

        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

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
