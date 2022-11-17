using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

        public static PlayerPassportHUDView CreateView()
        {
            return Instantiate(Resources.Load<GameObject>("PlayerPassport")).GetComponent<PlayerPassportHUDView>();
        }

        public void Initialize()
        {
            hideCardButton?.onClick.RemoveAllListeners();
            hideCardButton?.onClick.AddListener(() => OnClose?.Invoke());
        }

        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetPassportPanelVisibility(bool visible)
        {
            container.SetActive(visible);
        }

        public override void RefreshControl()
        {
        }
    }
}