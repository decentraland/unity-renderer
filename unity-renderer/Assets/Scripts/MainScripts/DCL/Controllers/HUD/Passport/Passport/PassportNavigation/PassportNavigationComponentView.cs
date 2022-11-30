using System;
using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private GameObject introContainer;
        [SerializeField] private Transform equippedWearablesContainer;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject wearableUIReferenceObject;

        private List<NFTIconComponentView> equippedWearables = new List<NFTIconComponentView>();

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

        public void InitializeView()
        {
            CleanEquippedWearables();
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
            introContainer.SetActive(!string.IsNullOrEmpty(description));
            descriptionText.text = description;
        }

        public void SetEquippedWearables(WearableItem[] wearables)
        {
            foreach (var wearable in wearables)
            {
                NFTIconComponentView nftIconComponentView = Instantiate(wearableUIReferenceObject, equippedWearablesContainer).GetComponent<NFTIconComponentView>();
                equippedWearables.Add(nftIconComponentView);
                NFTIconComponentModel nftModel = new NFTIconComponentModel()
                {
                    type = wearable.data.category,
                    marketplaceURI = "",
                    name = wearable.GetName(),
                    rarity = wearable.rarity,
                    imageURI = wearable.ComposeThumbnailUrl()
                };
                nftIconComponentView.Configure(nftModel);
            }
        }

        private void CleanEquippedWearables()
        {
            for (int i = equippedWearables.Count - 1; i >= 0; i--)
            {
                Destroy(equippedWearables[i].gameObject);
            }

            equippedWearables = new List<NFTIconComponentView>();
        }

        public override void RefreshControl()
        {

        }

    }
}
