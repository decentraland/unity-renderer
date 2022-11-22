using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentView : BaseComponentView, IPassportNavigationComponentView
    {
        private const string GUEST_TEXT = "is a guest";

        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject wearablesPanel;
        [SerializeField] private GameObject guestPanel;
        [SerializeField] private GameObject normalPanel;
        [SerializeField] private TextMeshProUGUI usernameText;

        public void SetGuestUser(bool isGuest)
        {
            guestPanel.SetActive(isGuest);
            normalPanel.SetActive(!isGuest);
        }

        public void SetName(string username)
        {
            usernameText.text = $"{username} {GUEST_TEXT}";
        }

        private void EnableAboutPanel()
        {
            wearablesPanel.SetActive(false);
            aboutPanel.SetActive(true);
        }

        private void EnableWearablesPanel()
        {
            aboutPanel.SetActive(false);
            wearablesPanel.SetActive(true);
        }

        public override void RefreshControl()
        {
        }

    }
}