using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DCL.Helpers;

namespace DCL
{
    public interface IMouseCatcher
    {
        event System.Action OnMouseUnlock;
        event System.Action OnMouseLock;
    }

    public class MouseCatcher : MonoBehaviour, IMouseCatcher, IPointerDownHandler
    {
        public event System.Action OnMouseUnlock;
        public event System.Action OnMouseLock;
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
            OnMouseLock?.Invoke();
        }

        //Externally called by the browser
        public void UnlockCursor()
        {
            Utils.UnlockCursor();
            OnMouseUnlock?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            LockCursor();
        }
    }
}
