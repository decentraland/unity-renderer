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

        public event Action<string> OnClickBuyNft;

        private const string NFT_ICON_POOL_NAME_PREFIX = "NFTIconsEntriesPool_";
        private const int MAX_NFT_ICON_ENTRIES = 20;
        private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.7f, 0.7f, 0.7f);

        private List<PoolableObject> poolableQueue = new List<PoolableObject>();
        private Pool entryPool;

        public override void Start()
        {
            subSectionSelector.GetSection(ABOUT_SUB_SECTION_INDEX).onSelect.RemoveAllListeners();
            subSectionSelector.GetSection(COLLECTIBLES_SUB_SECTION_INDEX).onSelect.RemoveAllListeners();
            subSectionSelector.GetSection(ABOUT_SUB_SECTION_INDEX).onSelect.AddListener((isActive) => aboutPanel.SetActive(isActive));
            subSectionSelector.GetSection(COLLECTIBLES_SUB_SECTION_INDEX).onSelect.AddListener((isActive) => wearablesPanel.SetActive(isActive));
        }

        public void InitializeView()
        {
            CleanEquippedWearables();
            entryPool = GetNftIconEntryPool();
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
                PoolableObject poolableObject = entryPool.Get();
                poolableQueue.Add(poolableObject);
                poolableObject.gameObject.transform.SetParent(equippedWearablesContainer, false);
                poolableObject.gameObject.transform.localScale = NFT_ICON_SCALE;
                NFTIconComponentView nftIconComponentView = poolableObject.gameObject.GetComponent<NFTIconComponentView>();
                nftIconComponentView.onMarketplaceButtonClick.RemoveAllListeners();
                nftIconComponentView.onMarketplaceButtonClick.AddListener(() => CLickOnBuyWearable(wearable.id));
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

        private void CLickOnBuyWearable(string wearableId)
        {
            OnClickBuyNft?.Invoke(wearableId);
        }

        private void CleanEquippedWearables()
        {
            foreach (var poolObject in poolableQueue)
            {
                entryPool.Release(poolObject);
            }

            poolableQueue = new List<PoolableObject>();
        }

        private Pool GetNftIconEntryPool()
        {
            var pool = PoolManager.i.GetPool(NFT_ICON_POOL_NAME_PREFIX + name + GetInstanceID());
            if (pool != null) return pool;

            pool = PoolManager.i.AddPool(
                NFT_ICON_POOL_NAME_PREFIX + name + GetInstanceID(),
                Instantiate(wearableUIReferenceObject).gameObject,
                maxPrewarmCount: MAX_NFT_ICON_ENTRIES,
                isPersistent: true);
            pool.ForcePrewarm();

            return pool;
        }

        public override void RefreshControl()
        {
        }

    }
}
