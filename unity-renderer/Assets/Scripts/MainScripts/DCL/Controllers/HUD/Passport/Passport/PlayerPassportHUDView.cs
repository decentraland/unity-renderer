using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PlayerPassportHUDView : BaseComponentView, IPlayerPassportHUDView
    {
        [SerializeField] private PassportPlayerInfoComponentView playerInfoView;
        [SerializeField] private PassportPlayerPreviewComponentView playerPreviewView;
        [SerializeField] private PassportNavigationComponentView passportNavigationView;
        [SerializeField] internal Button hideCardButton;
        [SerializeField] internal GameObject container;

        public IPassportPlayerInfoComponentView PlayerInfoView => playerInfoView;
        public IPassportPlayerPreviewComponentView PlayerPreviewView => playerPreviewView;
        public IPassportNavigationComponentView PassportNavigationView => passportNavigationView;
        public event Action OnClose;

        private MouseCatcher mouseCatcher;

        public static PlayerPassportHUDView CreateView() =>
            Instantiate(Resources.Load<GameObject>("PlayerPassport")).GetComponent<PlayerPassportHUDView>();

        public void Initialize()
        {
            hideCardButton.onClick.RemoveAllListeners();
            hideCardButton.onClick.AddListener(ClosePassport);
            mouseCatcher = DCL.SceneReferences.i.mouseCatcher;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseDown += ClosePassport;
        }

        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetPassportPanelVisibility(bool visible)
        {
            if (visible && mouseCatcher != null)
            {
                mouseCatcher.UnlockCursor();
            }
            container.SetActive(visible);
            CommonScriptableObjects.playerInfoCardVisibleState.Set(visible);
        }

        public override void RefreshControl()
        {
        }

        public override void Dispose()
        {
            if (mouseCatcher != null)
                mouseCatcher.OnMouseDown -= ClosePassport;
        }

        private void ClosePassport()
        {
            OnClose?.Invoke();
        }
    }
}
