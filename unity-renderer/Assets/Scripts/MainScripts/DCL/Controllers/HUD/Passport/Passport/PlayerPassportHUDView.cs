using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DCL;
using DCL.Helpers;

namespace DCL.Social.Passports
{
    public class PlayerPassportHUDView : BaseComponentView
    {
        [SerializeField] private PassportPlayerInfoComponentView playerInfoView;
        [SerializeField] private PassportPlayerPreviewComponentView playerPreviewView;
        [SerializeField] private PassportNavigationComponentView passportNavigationView;
        [SerializeField] internal Button hideCardButton;
        [SerializeField] internal GameObject container;

        public PassportPlayerInfoComponentView PlayerInfoView => playerInfoView;
        public PassportPlayerPreviewComponentView PlayerPreviewView => playerPreviewView;
        public PassportNavigationComponentView PassportNavigationView => passportNavigationView;
        public event Action OnClose;

        private MouseCatcher mouseCatcher;

        public static PlayerPassportHUDView CreateView()
        {
            return Instantiate(Resources.Load<GameObject>("PlayerPassport")).GetComponent<PlayerPassportHUDView>();
        }

        public void Initialize()
        {
            hideCardButton?.onClick.RemoveAllListeners();
            hideCardButton?.onClick.AddListener(ClosePassport);
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
            mouseCatcher.LockCursor();
            OnClose?.Invoke();
        }
    }
}