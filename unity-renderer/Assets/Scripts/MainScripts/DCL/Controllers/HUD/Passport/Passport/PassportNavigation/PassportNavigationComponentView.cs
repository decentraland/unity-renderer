using NSubstitute.ReturnsExtensions;
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
        private const int ABOUT_SUB_SECTION_INDEX = 0;
        private const int COLLECTIBLES_SUB_SECTION_INDEX = 1;

        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject wearablesPanel;
        [SerializeField] private SectionSelectorComponentView subSectionSelector;
        [SerializeField] private GameObject guestPanel;
        [SerializeField] private GameObject normalPanel;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public override void Start()
        {
            subSectionSelector.GetSection(ABOUT_SUB_SECTION_INDEX).onSelect.RemoveAllListeners();
            subSectionSelector.GetSection(COLLECTIBLES_SUB_SECTION_INDEX).onSelect.RemoveAllListeners();
            subSectionSelector.GetSection(ABOUT_SUB_SECTION_INDEX).onSelect.AddListener((isActive) =>
            {
                aboutPanel.SetActive(isActive);
            });
            subSectionSelector.GetSection(COLLECTIBLES_SUB_SECTION_INDEX).onSelect.AddListener((isActive) =>
            {
                wearablesPanel.SetActive(isActive);
            });
        }

        public void SetGuestUser(bool isGuest)
        {
            guestPanel.SetActive(isGuest);
            normalPanel.SetActive(!isGuest);
        }

        public void SetName(string username)
        {
            usernameText.text = $"{username} {GUEST_TEXT}";
        }

        public void SetDescription(string description)
        {
            descriptionText.text = description;
        }

        public override void RefreshControl()
        {
        }

    }
}
