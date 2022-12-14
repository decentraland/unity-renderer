using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentView : BaseComponentView, IPassportNavigationComponentView
    {
        private const string GUEST_TEXT = "is a guest";
        private const string BLOCKED_TEXT = "blocked you!";
        private const string TEMPLATE_DESCRIPTION_TEXT = "This person doesn't have an about description yet.";
        private const int ABOUT_SUB_SECTION_INDEX = 0;
        private const int COLLECTIBLES_SUB_SECTION_INDEX = 1;

        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject wearablesPanel;
        [SerializeField] private SectionSelectorComponentView subSectionSelector;
        [SerializeField] private GameObject guestPanel;
        [SerializeField] private GameObject normalPanel;
        [SerializeField] private GameObject hasBlockedPanel;
        [SerializeField] private Transform equippedWearablesContainer;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI blockedUsernameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private CarouselComponentView nftWearablesCarousel;
        [SerializeField] private CarouselComponentView nftEmotesCarousel;
        [SerializeField] private Transform nftWearablesCarouselContent;
        [SerializeField] private Transform nftEmotesCarouselContent;
        [SerializeField] private GameObject wearableUIReferenceObject;
        [SerializeField] private GameObject nftPageUIReferenceObject;

        public event Action<string> OnClickBuyNft;
        public event Action OnClickCollectibles;

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
            subSectionSelector.GetSection(COLLECTIBLES_SUB_SECTION_INDEX).onSelect.AddListener((isActive) =>
            {
                OnClickCollectibles?.Invoke();
                wearablesPanel.SetActive(isActive);
            });
        }

        public void InitializeView()
        {
            nftPagesEntryPool = GetNftPagesEntryPool();
            nftIconsEntryPool = GetNftIconEntryPool();
            CleanWearables();
            CleanEquippedWearables();
            SetInitialTab();
        }

        public void SetGuestUser(bool isGuest)
        {
            guestPanel.SetActive(isGuest);
            normalPanel.SetActive(!isGuest);
            hasBlockedPanel.SetActive(false);
        }

        public void SetName(string username)
        {
            usernameText.text = $"{username} {GUEST_TEXT}";
            blockedUsernameText.text = $"{username} {BLOCKED_TEXT}";
        }

        public void SetDescription(string description)
        {
            descriptionText.text = string.IsNullOrEmpty(description) ? TEMPLATE_DESCRIPTION_TEXT : description;
        }

        public void SetEquippedWearables(WearableItem[] wearables, string bodyShapeId)
        {
            HashSet<string> hidesList = WearableItem.ComposeHiddenCategories(bodyShapeId, wearables.ToList());

            foreach (var wearable in wearables)
            {
                if (!hidesList.Contains(wearable.data.category))
                {
                    PoolableObject poolableObject = nftIconsEntryPool.Get();
                    nftIconPoolableQueue.Add(poolableObject);
                    poolableObject.gameObject.transform.SetParent(equippedWearablesContainer, false);
                    poolableObject.gameObject.transform.localScale = NFT_ICON_SCALE;
                    NFTIconComponentView nftIconComponentView = poolableObject.gameObject.GetComponent<NFTIconComponentView>();
                    nftIconComponentView.onMarketplaceButtonClick.RemoveAllListeners();
                    nftIconComponentView.onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(wearable.id));

                    NFTIconComponentModel nftModel = new NFTIconComponentModel()
                    {
                        showMarketplaceButton = wearable.IsCollectible(),
                        showType = wearable.IsCollectible(),
                        type = wearable.data.category,
                        marketplaceURI = "",
                        name = wearable.GetName(),
                        rarity = wearable.rarity,
                        imageURI = wearable.ComposeThumbnailUrl()
                    };

                    nftIconComponentView.Configure(nftModel);
                }
            }
        }

        public void SetCollectibleWearables(WearableItem[] wearables)
        {
            nftWearablesCarousel.CleanInstantiatedItems();
            for (int i = 0; i < wearables.Length; i += 4)
            {
                PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
                nftPagesPoolableQueue.Add(nftPagePoolElement);
                nftPagePoolElement.gameObject.transform.SetParent(nftWearablesCarouselContent, false);
                NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
                nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
                NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
                string[] nftIds = new string[4];
                for (int j = 0; j < 4; j++)
                {
                    if (wearables.Length > i + j && wearables[i + j] != null)
                    {
                        pageElements[j] = new ()
                        {
                            showMarketplaceButton = true,
                            showType = true,
                            type = wearables[i+j].data.category,
                            marketplaceURI = "",
                            name = wearables[i+j].GetName(),
                            rarity = wearables[i+j].rarity,
                            imageURI = wearables[i+j].ComposeThumbnailUrl()
                        };

                        nftIds[j] = wearables[i + j].id;
                    }
                    else
                    {
                        pageElements[j] = null;
                        nftIds[j] = "";
                    }
                }
                nftPageView.SetPageElementsContent(pageElements, nftIds);
                nftPageView.OnClickBuyNft += ClickOnBuyWearable;
                nftWearablesCarousel.AddItem(nftPageView);
            }
            nftWearablesCarousel.GenerateDotsSelector();
            nftWearablesCarousel.ResetManualCarousel();
        }

        public void SetCollectibleEmotes(WearableItem[] wearables)
        {
            nftEmotesCarousel.CleanInstantiatedItems();
            for (int i = 0; i < wearables.Length; i += 4)
            {
                PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
                nftPagesPoolableQueue.Add(nftPagePoolElement);
                nftPagePoolElement.gameObject.transform.SetParent(nftEmotesCarouselContent, false);
                NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
                nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
                NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
                string[] nftIds = new string[4];
                for (int j = 0; j < 4; j++)
                {
                    if (wearables.Length > i + j && wearables[i + j] != null)
                    {
                        pageElements[j] = new ()
                        {
                            showMarketplaceButton = true,
                            showType = true,
                            type = "emote",
                            marketplaceURI = "",
                            name = wearables[i+j].GetName(),
                            rarity = wearables[i+j].rarity,
                            imageURI = wearables[i+j].ComposeThumbnailUrl()
                        };

                        nftIds[j] = wearables[i + j].id;
                    }
                    else
                    {
                        pageElements[j] = null;
                        nftIds[j] = "";
                    }
                }
                nftPageView.SetPageElementsContent(pageElements, nftIds);
                nftPageView.OnClickBuyNft += ClickOnBuyWearable;
                nftEmotesCarousel.AddItem(nftPageView);
            }
            nftEmotesCarousel.GenerateDotsSelector();
            nftEmotesCarousel.ResetManualCarousel();
        }

        public void SetHasBlockedOwnUser(bool isBlocked)
        {
            hasBlockedPanel.SetActive(isBlocked);
        }

        public void SetInitialTab()
        {
            subSectionSelector.GetSection(ABOUT_SUB_SECTION_INDEX).SelectToggle();
            subSectionSelector.GetSection(COLLECTIBLES_SUB_SECTION_INDEX).SetUnselectedVisuals();
        }

        private void ClickOnBuyWearable(string wearableId)
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
