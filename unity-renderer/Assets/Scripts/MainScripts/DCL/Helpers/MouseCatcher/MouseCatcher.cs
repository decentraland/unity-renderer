#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_PLATFORM
#endif

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
        bool isLocked { get; }
    }

    public class MouseCatcher : MonoBehaviour, IMouseCatcher, IPointerDownHandler
    {
        public bool isLocked => Utils.isCursorLocked;
        bool renderingEnabled => CommonScriptableObjects.rendererState.Get();

        public event System.Action OnMouseUnlock;
        public event System.Action OnMouseLock;
        public event System.Action OnMouseDown;

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
            if (!renderingEnabled) return;

            Utils.LockCursor();
#if !WEB_PLATFORM            
            OnMouseLock?.Invoke();
#endif
        }

        public void UnlockCursor()
        {
            Utils.UnlockCursor();
#if !WEB_PLATFORM            
            OnMouseUnlock?.Invoke();
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnMouseDown?.Invoke();
            LockCursor();
        }

#if WEB_PLATFORM
        //Externally -ONLY- called by the browser
        public void UnlockCursorBrowser(int val)
        {
            bool lockPointer = val != 0;
            Utils.BrowserSetCursorState(lockPointer);
            if (lockPointer)
            {
                OnMouseLock?.Invoke();
            }
            else
            {
                OnMouseUnlock?.Invoke();
            }
        }
#endif

    }
}