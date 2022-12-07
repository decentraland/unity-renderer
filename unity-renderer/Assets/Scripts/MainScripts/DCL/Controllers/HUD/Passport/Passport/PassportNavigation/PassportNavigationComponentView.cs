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
        [SerializeField] private CarouselComponentView nftWearablesCarousel;
        [SerializeField] private Transform nftWearablesCarouselContent;
        [SerializeField] private GameObject wearableUIReferenceObject;
        [SerializeField] private GameObject nftPageUIReferenceObject;

        public event Action<string> OnClickBuyNft;

        private const string NFT_ICON_POOL_NAME_PREFIX = "NFTIconsEntriesPool_";
        private const string NFT_PAGES_POOL_NAME_PREFIX = "NFTPagesEntriesPool_";
        private const int MAX_NFT_ICON_ENTRIES = 20;
        private const int MAX_NFT_PAGES_ENTRIES = 20;
        private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.7f, 0.7f, 0.7f);

        private List<PoolableObject> nftIconPoolableQueue = new List<PoolableObject>();
        private List<PoolableObject> nftPagesPoolableQueue = new List<PoolableObject>();
        private Pool nftIconsEntryPool;
        private Pool nftPagesEntryPool;

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
            CleanWearables();
            nftIconsEntryPool = GetNftIconEntryPool();
            nftPagesEntryPool = GetNftPagesEntryPool();
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
                PoolableObject poolableObject = nftIconsEntryPool.Get();
                nftIconPoolableQueue.Add(poolableObject);
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

        public void SetCollectibleWearables(WearableItem[] wearables)
        {
            List<BaseComponentView> pagesList = new List<BaseComponentView>();
            for (int i = 0; i < wearables.Length; i += 4)
            {
                PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
                nftPagesPoolableQueue.Add(nftPagePoolElement);
                nftPagePoolElement.gameObject.transform.SetParent(nftWearablesCarouselContent, false);

                NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();

                NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
                for (int j = 0; j < 4; j++)
                {
                    if (wearables[i + j] != null)
                    {
                        pageElements[j] = new ()
                        {
                            type = wearables[i+j].data.category,
                            marketplaceURI = "",
                            name = wearables[i+j].GetName(),
                            rarity = wearables[i+j].rarity,
                            imageURI = wearables[i+j].ComposeThumbnailUrl()
                        };
                    }
                    else
                    {
                        pageElements[j] = null;
                    }
                    nftPageView.SetPageElementsContent(pageElements);
                    pagesList.Add(nftPageView);
                }
            }
            nftWearablesCarousel.SetItems(pagesList);
        }

        private void CLickOnBuyWearable(string wearableId)
        {
            OnClickBuyNft?.Invoke(wearableId);
        }

        private void CleanEquippedWearables()
        {
            foreach (var poolObject in nftIconPoolableQueue)
            {
                nftIconsEntryPool.Release(poolObject);
            }

            nftIconPoolableQueue = new List<PoolableObject>();
        }

        private void CleanWearables()
        {
            foreach (var poolObject in nftPagesPoolableQueue)
            {
                nftPagesEntryPool.Release(poolObject);
            }

            nftPagesPoolableQueue = new List<PoolableObject>();
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

        private Pool GetNftPagesEntryPool()
        {
            var pool = PoolManager.i.GetPool(NFT_PAGES_POOL_NAME_PREFIX + name + GetInstanceID());
            if (pool != null) return pool;

            pool = PoolManager.i.AddPool(
                NFT_PAGES_POOL_NAME_PREFIX + name + GetInstanceID(),
                Instantiate(nftPageUIReferenceObject).gameObject,
                maxPrewarmCount: MAX_NFT_PAGES_ENTRIES,
                isPersistent: true);
            pool.ForcePrewarm();

            return pool;
        }

        public override void RefreshControl()
        {
        }

    }
}
