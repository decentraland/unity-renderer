#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_PLATFORM
#endif

using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL
{
    public interface IMouseCatcher
    {
        event Action OnMouseUnlock;
        event Action OnMouseLock;
        bool isLocked { get; }
    }

    public class MouseCatcher : MonoBehaviour, IMouseCatcher, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private InputAction_Trigger unlockInputAction;

        public bool isLocked => Utils.IsCursorLocked;
        bool renderingEnabled => CommonScriptableObjects.rendererState.Get();

        public event Action OnMouseUnlock;
        public event Action OnMouseLock;
        public event Action OnMouseDown;

        //Default OnPointerEvent
        public LayerMask OnPointerDownTarget = 1 << 9;

        private void Start()
        {
            unlockInputAction.OnTriggered += HandleUnlockInput;
        }

        private void OnDestroy()
        {
            unlockInputAction.OnTriggered -= HandleUnlockInput;
        }

        public void LockCursor()
        {
            if (!renderingEnabled || DataStore.i.common.isSignUpFlow.Get() || DataStore.i.HUDs.loadingHUD.visible.Get())
                return;

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
            if (eventData.button == PointerEventData.InputButton.Right)
                DataStore.i.camera.panning.Set(true);
            OnMouseDown?.Invoke();
            LockCursor();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                    DataStore.i.camera.panning.Set(false);
                UnlockCursor();
            }
        }

        #region BROWSER_ONLY

        //TODO(Brian): Move all this mechanism to a new MouseLockController and branch
        //             behaviour using strategy pattern instead of this.

        /// <summary>
        /// Externally -ONLY- called by the browser
        /// </summary>
        /// <param name="val">1 is locked, 0 is unlocked</param>
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

        #endregion

        private void HandleUnlockInput(DCLAction_Trigger action)
        {
            UnlockCursor();
        }
    }
}