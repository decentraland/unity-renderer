using DCL.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DCL.Helpers;

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
            Utils.LockCursor();
        }

        //Externally called by the browser
        public void UnlockCursor()
        {
            Utils.UnlockCursor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            LockCursor();
        }
    }
}