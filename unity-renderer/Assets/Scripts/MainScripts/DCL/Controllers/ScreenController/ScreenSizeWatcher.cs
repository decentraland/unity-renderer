using System;
using UnityEngine;

namespace DCL.Components
{
    public class ScreenSizeWatcher : MonoBehaviour
    {
        private BaseVariable<Vector2Int> ScreenSize => DataStore.i.screen.size;
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
            ScreenSize.Set(new Vector2Int(Screen.width, Screen.height));
        }
        private bool HasScreenSizeChanged()
        {
            Vector2Int size = ScreenSize.Get();
            return size.x != Screen.width || size.y != Screen.height;
        }
    }
}