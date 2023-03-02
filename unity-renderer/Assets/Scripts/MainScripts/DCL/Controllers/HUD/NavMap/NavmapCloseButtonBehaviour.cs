using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapCloseButtonBehaviour: IDisposable
    {
        private readonly Button closeButton;
        
        private readonly BaseVariable<bool> navmapVisible;
        private readonly BaseVariable<Transform> configureMapInFullscreenMenu;
        
        public NavmapCloseButtonBehaviour (Button closeButton, BaseVariable<bool> navmapVisible, BaseVariable<Transform> configureMapInFullscreenMenu)
        {
            this.navmapVisible = navmapVisible;
            
            this.closeButton = closeButton;
            this.configureMapInFullscreenMenu = configureMapInFullscreenMenu;

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            configureMapInFullscreenMenu.OnChange += HideCloseButton;
        }

        public void Dispose()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            configureMapInFullscreenMenu.OnChange -= HideCloseButton;
        }
        
        private void OnCloseButtonClicked() => navmapVisible.Set(false);

        private void HideCloseButton(Transform currentParentTransform, Transform _)
        {
            if (currentParentTransform != null)
                closeButton.gameObject.SetActive(false);
        }
    }
}