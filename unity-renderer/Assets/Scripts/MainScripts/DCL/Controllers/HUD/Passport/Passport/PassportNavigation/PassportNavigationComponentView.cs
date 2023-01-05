using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentView : BaseComponentView, IPassportNavigationComponentView
    {
        private const string GUEST_TEXT = "is a guest";
        private const string BLOCKED_TEXT = "blocked you!";
        private const string linksRegex = @"\[(.*?)\)";
        private const string linkTitleRegex = @"(?<=\[).+?(?=\])";
        private const string linkRegex = @"(?<=\().+?(?=\))";

        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject wearablesPanel;
        [SerializeField] private Toggle aboutToggle;
        [SerializeField] private Toggle collectiblesToggle;
        [SerializeField] private GameObject guestPanel;
        [SerializeField] private GameObject normalPanel;
        [SerializeField] private GameObject hasBlockedPanel;
        [SerializeField] private Transform equippedWearablesContainer;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI blockedUsernameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject emptyDescriptionGO;
        [SerializeField] private CarouselComponentView nftWearablesCarousel;
        [SerializeField] private GameObject emptyWearablesText;
        [SerializeField] private CarouselComponentView nftEmotesCarousel;
        [SerializeField] private GameObject emptyEmotesText;
        [SerializeField] private Transform nftWearablesCarouselContent;
        [SerializeField] private Transform nftEmotesCarouselContent;
        [SerializeField] private GameObject wearableUIReferenceObject;
        [SerializeField] private GameObject nftPageUIReferenceObject;
        [SerializeField] private GameObject linksContainer;
        [SerializeField] private GameObject linksTitle;
        [SerializeField] private GameObject linkPrefabReference;
        [SerializeField] private GameObject aboutToggleOn;
        [SerializeField] private GameObject aboutToggleOff;
        [SerializeField] private GameObject collectiblesToggleOn;
        [SerializeField] private GameObject collectiblesToggleOff;
        [SerializeField] private TMP_Text aboutText;
        [SerializeField] private TMP_Text collectiblesText;
        [SerializeField] private OpenUrlView openUrlView;

        private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.75f, 0.75f, 0.75f);
        public event Action<string> OnClickBuyNft;
        public event Action OnClickCollectibles;

        private const string NFT_ICON_POOL_NAME_PREFIX = "NFTIconsEntriesPool_";
        private const string NFT_PAGES_POOL_NAME_PREFIX = "NFTPagesEntriesPool_";
        private const int MAX_NFT_ICON_ENTRIES = 20;
        private const int MAX_NFT_PAGES_ENTRIES = 20;

        private List<PoolableObject> nftIconPoolableQueue = new List<PoolableObject>();
        private List<PoolableObject> nftPagesPoolableQueue = new List<PoolableObject>();
        private Pool nftIconsEntryPool;
        private Pool nftPagesEntryPool;

        public override void Start()
        {
            collectiblesToggle.isOn = false;
            aboutToggle.isOn = true;
            aboutToggle.onValueChanged.AddListener(EnableAboutSection);
            collectiblesToggle.onValueChanged.AddListener(EnableCollectiblesSection);
        }

        private void EnableAboutSection(bool isActive)
        {
            aboutPanel.SetActive(isActive);
            aboutText.color = Color.white;
            collectiblesText.color = Color.black;
            aboutToggleOn.SetActive(true);
            aboutToggleOff.SetActive(false);
            collectiblesToggleOn.SetActive(false);
            collectiblesToggleOff.SetActive(true);
        }

        private void EnableCollectiblesSection(bool isActive)
        {
            OnClickCollectibles?.Invoke();
            wearablesPanel.SetActive(isActive);
            aboutText.color = Color.black;
            collectiblesText.color = Color.white;
            aboutToggleOn.SetActive(false);
            aboutToggleOff.SetActive(true);
            collectiblesToggleOn.SetActive(true);
            collectiblesToggleOff.SetActive(false);
            nftWearablesCarousel.ResetManualCarousel();
            nftEmotesCarousel.ResetManualCarousel();
        }

        public void InitializeView()
        {
            nftPagesEntryPool = GetNftPagesEntryPool();
            nftIconsEntryPool = GetNftIconEntryPool();
            CleanWearables();
            CleanEquippedWearables();
            SetInitialPage();
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
            MatchCollection matchCollection = Regex.Matches(description, linksRegex, RegexOptions.IgnoreCase);
            List<string> links = new List<string>();
            foreach (Match link in matchCollection)
            {
                description = description.Replace(link.Value, "");
                links.Add(link.Value);
            }
            linksTitle.SetActive(links.Count > 0);
            linksContainer.SetActive(links.Count > 0);

            if (string.IsNullOrEmpty(description))
            {
                emptyDescriptionGO.SetActive(true);
                descriptionText.gameObject.SetActive(false);
            }
            else
            {
                emptyDescriptionGO.SetActive(false);
                descriptionText.gameObject.SetActive(true);
                descriptionText.text = description;
            }

            SetLinks(links);
        }

        private void SetLinks(List<string> links)
        {
            foreach (Transform child in linksContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (string link in links)
            {
                PassportLinkView newLink = Instantiate(linkPrefabReference, linksContainer.transform).GetComponent<PassportLinkView>();
                newLink.OnClickLink -= ClickedLink;
                newLink.SetLinkTitle(Regex.Matches(link, linkTitleRegex, RegexOptions.IgnoreCase)[0].Value);
                newLink.SetLink(Regex.Matches(link, linkRegex, RegexOptions.IgnoreCase)[0].Value);
                newLink.OnClickLink += ClickedLink;
            }
        }

        private void ClickedLink(string obj)
        {
            openUrlView.SetUrlInfo(obj, obj);
            openUrlView.SetVisibility(true);
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
            nftWearablesCarousel.gameObject.SetActive(wearables.Length > 0);
            nftWearablesCarousel.CleanInstantiatedItems();
            emptyWearablesText.SetActive(wearables.Length <= 0);
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
        }

        public void SetCollectibleEmotes(WearableItem[] wearables)
        {
            nftEmotesCarousel.gameObject.SetActive(wearables.Length > 0);
            nftEmotesCarousel.CleanInstantiatedItems();
            emptyEmotesText.SetActive(wearables.Length <= 0);
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
        }

        public void SetHasBlockedOwnUser(bool isBlocked)
        {
            hasBlockedPanel.SetActive(isBlocked);
        }

        public void SetInitialPage()
        {
            aboutToggle.isOn = true;
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
