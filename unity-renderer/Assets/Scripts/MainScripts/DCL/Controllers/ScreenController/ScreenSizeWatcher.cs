using System;
using UnityEngine;

namespace DCL.Components
{
    public class ScreenSizeWatcher : MonoBehaviour
    {
        private BaseVariable<Vector2Int> screenSize => DataStore.i.screen.size;
        private void Awake()
        {
            UpdateScreenSize();
        }
        private void LateUpdate()
        {
            if (HasScreenSizeChanged())
            {
                UpdateScreenSize();
            }
        }

        private void UpdateScreenSize()
        {
            screenSize.Set(new Vector2Int(Screen.width, Screen.height));
        }
        private bool HasScreenSizeChanged()
        {
            Vector2Int size = screenSize.Get();
            return size.x != Screen.width || size.y != Screen.height;
        }
    }
}