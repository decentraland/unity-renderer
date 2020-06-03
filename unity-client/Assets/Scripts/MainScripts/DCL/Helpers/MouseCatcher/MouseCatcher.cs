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
            OnMouseDown?.Invoke();
            LockCursor();
        }
    }
}