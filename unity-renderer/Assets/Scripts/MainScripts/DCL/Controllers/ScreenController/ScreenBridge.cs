using System;
using UnityEngine;

namespace DCL.Components
{
    public class ScreenBridge : MonoBehaviour
    {
        public static ScreenBridge i { get; private set; }
        public event Action OnScreenResize;

        private int lastWidth;
        private int lastHeight;
        private void Awake()
        {
            UpdateScreenSize();
            i = this;
        }
        private void LateUpdate()
        {
            if (HasScreenSizeChanged())
            {
                UpdateScreenSize();
                SendScreenResizeEvent();
            }
        }

        public void ForceScreenResizeEvent() { SendScreenResizeEvent(); }
        private void SendScreenResizeEvent() { OnScreenResize?.Invoke(); }
        private void UpdateScreenSize()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
        private bool HasScreenSizeChanged() { return lastWidth != Screen.width || lastHeight != Screen.height; }
    }
}