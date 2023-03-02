#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_PLATFORM
#endif

using DCL.Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DCL
{
    public interface IMouseCatcher
    {
        bool IsLocked { get; }
        event Action OnMouseUnlock;
        event Action OnMouseLock;

        void UnlockCursor();
    }

    public class MouseCatcher : MonoBehaviour, IMouseCatcher, IPointerDownHandler, IPointerUpHandler
    {
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        [SerializeField] private InputAction_Trigger unlockInputAction;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image raycastTarget;

        private HUDCanvasCameraModeController hudCanvasCameraModeController;

        public bool IsLocked => Utils.IsCursorLocked;
        private bool renderingEnabled => CommonScriptableObjects.rendererState.Get();

        public event Action OnMouseUnlock;
        public event Action OnMouseLock;
        public event Action OnMouseDown;

        private void Start()
        {
            hudCanvasCameraModeController = new HUDCanvasCameraModeController(canvas, DataStore.i.camera.hudsCamera);
            unlockInputAction.OnTriggered += HandleUnlockInput;
        }

        private void OnDestroy()
        {
            hudCanvasCameraModeController?.Dispose();
            unlockInputAction.OnTriggered -= HandleUnlockInput;
        }

        public void LockCursor()
        {
            if (!renderingEnabled || DataStore.i.common.isSignUpFlow.Get() || dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Get())
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
            {
                LockCursor();
                DataStore.i.camera.panning.Set(true);
            }
            else if(DataStore.i.camera.leftMouseButtonCursorLock.Get())
                LockCursor();

            OnMouseDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
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
                OnMouseLock?.Invoke();
            else
                OnMouseUnlock?.Invoke();
        }
#endregion

        public bool IsEqualsToRaycastTarget(GameObject gameObject) =>
            raycastTarget.gameObject == gameObject;

        private void HandleUnlockInput(DCLAction_Trigger action) =>
            UnlockCursor();
    }
}
