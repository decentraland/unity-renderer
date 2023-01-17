using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Passports
{
    public class PassportNavigationComponentView : BaseComponentView, IPassportNavigationComponentView
    {
        private const string GUEST_TEXT = "is a guest";
        private const string BLOCKED_TEXT = "blocked you!";
        private const string LINKS_REGEX = @"\[(.*?)\)";
        private const string LINK_TITLE_REGEX = @"(?<=\[).+?(?=\])";
        private const string LINK_REGEX = @"(?<=\().+?(?=\))";
        private const string OWN_PLAYER = "\n         You don't ";
        private const string OTHER_PLAYERS = "\n         This person doesn't ";
        private const string NO_WEARABLES_TEXT = "own any Wearables yet.";
        private const string NO_EMOTES_TEXT = "own any Emotes yet.";
        private const string NO_NAMES_TEXT = "own any NAMEs yet.";
        private const string NO_LANDS_TEXT = "own any LANDs yet.";
        private const string NAME_TYPE = "name";
        private const string EMOTE_TYPE = "emote";
        private const string LEGENDARY_RARITY = "legendary";
        private const string LAND_RARITY = "land";

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
        [SerializeField] private TMP_Text emptyWearablesText;
        [SerializeField] private GameObject nftWearablesLoadingSpinner;
        [SerializeField] private CarouselComponentView nftEmotesCarousel;
        [SerializeField] private TMP_Text emptyEmotesText;
        [SerializeField] private GameObject nftEmotesLoadingSpinner;
        [SerializeField] private CarouselComponentView nftNamesCarousel;
        [SerializeField] private TMP_Text emptyNamesText;
        [SerializeField] private GameObject nftNamesLoadingSpinner;
        [SerializeField] private CarouselComponentView nftLandsCarousel;
        [SerializeField] private TMP_Text emptyLandsText;
        [SerializeField] private GameObject nftLandsLoadingSpinner;
        [SerializeField] private Transform nftWearablesCarouselContent;
        [SerializeField] private Transform nftEmotesCarouselContent;
        [SerializeField] private Transform nftNamesCarouselContent;
        [SerializeField] private Transform nftLandsCarouselContent;
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
        [SerializeField] internal NFTItemInfo nftItemInfo;
        [SerializeField] internal ScrollRect nftWearablesScrollRect;
        [SerializeField] internal ScrollRect nftEmotesScrollRect;
        [SerializeField] internal ScrollRect nftNamesScrollRect;
        [SerializeField] internal ScrollRect nftLandsScrollRect;
        [SerializeField] internal ScrollRect collectiblesMainScrollRect;

        private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.75f, 0.75f, 0.75f);
        public event Action<string, string> OnClickBuyNft;
        public event Action OnClickCollectibles;

        private const string NFT_ICON_POOL_NAME_PREFIX = "NFTIconsEntriesPool_";
        private const string NFT_PAGES_POOL_NAME_PREFIX = "NFTPagesEntriesPool_";
        private const int MAX_NFT_ICON_ENTRIES = 20;
        private const int MAX_NFT_PAGES_ENTRIES = 20;

        private List<PoolableObject> nftIconPoolableQueue = new List<PoolableObject>();
        private List<PoolableObject> nftPagesPoolableQueue = new List<PoolableObject>();
        private Pool nftIconsEntryPool;
        private Pool nftPagesEntryPool;
        private readonly List<NFTIconComponentView> equippedNftWearableViews = new List<NFTIconComponentView>();
        private readonly List<NftPageView> ownedNftWearablePageViews = new List<NftPageView>();
        private readonly List<NftPageView> ownedNftEmotePageViews = new List<NftPageView>();

        public override void Start()
        {
            collectiblesToggle.isOn = false;
            aboutToggle.isOn = true;
            aboutToggle.onValueChanged.AddListener(EnableAboutSection);
            collectiblesToggle.onValueChanged.AddListener(EnableCollectiblesSection);
            nftWearablesScrollRect.onValueChanged.AddListener((pos) => CloseAllNFTItemInfos());
            nftEmotesScrollRect.onValueChanged.AddListener((pos) => CloseAllNFTItemInfos());
            nftNamesScrollRect.onValueChanged.AddListener((pos) => CloseAllNFTItemInfos());
            nftLandsScrollRect.onValueChanged.AddListener((pos) => CloseAllNFTItemInfos());
            collectiblesMainScrollRect.onValueChanged.AddListener((pos) => CloseAllNFTItemInfos());
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
            CloseAllNFTItemInfos();
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
            nftNamesCarousel.ResetManualCarousel();
            nftLandsCarousel.ResetManualCarousel();
            CloseAllNFTItemInfos();
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

        public void SetOwnUserTexts(bool isOwnUser)
        {
            string prefix = isOwnUser ? OWN_PLAYER : OTHER_PLAYERS;
            emptyWearablesText.text = $"{prefix}{NO_WEARABLES_TEXT}";
            emptyEmotesText.text = $"{prefix}{NO_EMOTES_TEXT}";
            emptyNamesText.text = $"{prefix}{NO_NAMES_TEXT}";
            emptyLandsText.text = $"{prefix}{NO_LANDS_TEXT}";
        }

        public void SetDescription(string description)
        {
            MatchCollection matchCollection = Regex.Matches(description, LINKS_REGEX, RegexOptions.IgnoreCase);
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
            foreach (Transform child in linksContainer.transform) { Destroy(child.gameObject); }

            foreach (string link in links)
            {
                PassportLinkView newLink = Instantiate(linkPrefabReference, linksContainer.transform).GetComponent<PassportLinkView>();
                newLink.OnClickLink -= ClickedLink;
                newLink.SetLinkTitle(Regex.Matches(link, LINK_TITLE_REGEX, RegexOptions.IgnoreCase)[0].Value);
                newLink.SetLink(Regex.Matches(link, LINK_REGEX, RegexOptions.IgnoreCase)[0].Value);
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

            equippedNftWearableViews.Clear();

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
                    nftIconComponentView.onDetailInfoButtonClick.RemoveAllListeners();
                    nftIconComponentView.onFocused -= FocusEquippedNftItem;
                    nftIconComponentView.onMarketplaceButtonClick.AddListener(() => ClickOnBuyWearable(wearable.id, wearable.data.category));
                    nftIconComponentView.onDetailInfoButtonClick.AddListener(() => nftIconComponentView.SetNFTItemInfoActive(true));
                    nftIconComponentView.onFocused += FocusEquippedNftItem;

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
                    nftIconComponentView.ConfigureNFTItemInfo(nftItemInfo, wearable, true);
                    equippedNftWearableViews.Add(nftIconComponentView);
                }
            }
        }

        public void SetCollectibleWearables(WearableItem[] wearables)
        {
            nftWearablesCarousel.gameObject.SetActive(wearables.Length > 0);
            nftWearablesCarousel.CleanInstantiatedItems();
            emptyWearablesText.gameObject.SetActive(wearables.Length <= 0);
            ownedNftWearablePageViews.Clear();

            for (int i = 0; i < wearables.Length; i += 4)
            {
                NftPageView nftPageView = CreateWearablePageView(wearables, i);
                ownedNftWearablePageViews.Add(nftPageView);
                nftWearablesCarousel.AddItem(nftPageView);
            }

            nftWearablesCarousel.SetManualControlsActive();
            nftWearablesCarousel.GenerateDotsSelector();
        }

        public void SetCollectibleEmotes(WearableItem[] wearables)
        {
            nftEmotesCarousel.gameObject.SetActive(wearables.Length > 0);
            nftEmotesCarousel.CleanInstantiatedItems();
            emptyEmotesText.gameObject.SetActive(wearables.Length <= 0);

            for (int i = 0; i < wearables.Length; i += 4)
            {
                NftPageView nftPageView = CreateEmotePageView(wearables, i);
                ownedNftEmotePageViews.Add(nftPageView);
                nftEmotesCarousel.AddItem(nftPageView);
            }

            nftEmotesCarousel.SetManualControlsActive();
            nftEmotesCarousel.GenerateDotsSelector();
        }

        public void SetCollectibleNames(NamesResponse.NameEntry[] names)
        {
            nftNamesCarousel.gameObject.SetActive(names.Length > 0);
            nftNamesCarousel.CleanInstantiatedItems();
            emptyNamesText.gameObject.SetActive(names.Length <= 0);

            for (int i = 0; i < names.Length; i += 4)
            {
                PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
                nftPagesPoolableQueue.Add(nftPagePoolElement);
                nftPagePoolElement.gameObject.transform.SetParent(nftNamesCarouselContent, false);
                NftPageView nftPageView = CreateNamePageView(names, nftPagePoolElement, i);
                nftNamesCarousel.AddItem(nftPageView);
            }

            nftNamesCarousel.SetManualControlsActive();
            nftNamesCarousel.GenerateDotsSelector();
        }

        public void SetCollectibleLands(LandsResponse.LandEntry[] lands)
        {
            nftLandsCarousel.gameObject.SetActive(lands.Length > 0);
            nftLandsCarousel.CleanInstantiatedItems();
            emptyLandsText.gameObject.SetActive(lands.Length <= 0);

            for (int i = 0; i < lands.Length; i += 4)
            {
                NftPageView nftPageView = CreateLandPageView(lands, i);
                nftLandsCarousel.AddItem(nftPageView);
            }

            nftLandsCarousel.SetManualControlsActive();
            nftLandsCarousel.GenerateDotsSelector();
        }

        public void SetHasBlockedOwnUser(bool isBlocked)
        {
            hasBlockedPanel.SetActive(isBlocked);
        }

        public void SetInitialPage()
        {
            aboutToggle.isOn = true;
        }

        public void CloseAllNFTItemInfos()
        {
            for (int i = 0; i < equippedNftWearableViews.Count; i++)
                equippedNftWearableViews[i].SetNFTItemInfoActive(false);

            for (int i = 0; i < ownedNftWearablePageViews.Count; i++)
                ownedNftWearablePageViews[i].CloseAllNFTItemInfos();

            for (int i = 0; i < ownedNftEmotePageViews.Count; i++)
                ownedNftEmotePageViews[i].CloseAllNFTItemInfos();
        }

        private void ClickOnBuyWearable(string wearableId, string wearableType)
        {
            OnClickBuyNft?.Invoke(wearableId, wearableType);
        }

        private void FocusEquippedNftItem(bool isFocused)
        {
            if (!isFocused)
                return;

            FocusAnyNtfItem();
        }

        private void FocusAnyNtfItem() =>
            CloseAllNFTItemInfos();

        private void CleanEquippedWearables()
        {
            foreach (var poolObject in nftIconPoolableQueue) { nftIconsEntryPool.Release(poolObject); }

            nftIconPoolableQueue = new List<PoolableObject>();
        }

        private void CleanWearables()
        {
            foreach (var poolObject in nftPagesPoolableQueue) { nftPagesEntryPool.Release(poolObject); }

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

        public override void RefreshControl() { }

        public void SetCollectibleWearablesLoadingActive(bool isActive) =>
            SetCollectibleSectionLoadingActive(isActive, nftWearablesCarousel, emptyWearablesText.gameObject, nftWearablesLoadingSpinner);

        public void SetCollectibleEmotesLoadingActive(bool isActive) =>
            SetCollectibleSectionLoadingActive(isActive, nftEmotesCarousel, emptyEmotesText.gameObject, nftEmotesLoadingSpinner);

        public void SetCollectibleNamesLoadingActive(bool isActive) =>
            SetCollectibleSectionLoadingActive(isActive, nftNamesCarousel, emptyNamesText.gameObject, nftNamesLoadingSpinner);

        public void SetCollectibleLandsLoadingActive(bool isActive) =>
            SetCollectibleSectionLoadingActive(isActive, nftLandsCarousel, emptyLandsText.gameObject, nftLandsLoadingSpinner);

        private void SetCollectibleSectionLoadingActive(
            bool isActive,
            CarouselComponentView carousel,
            GameObject emptyText,
            GameObject loadingSpinner)
        {
            if (isActive)
            {
                carousel.gameObject.SetActive(false);
                emptyText.SetActive(false);
            }

            loadingSpinner.SetActive(isActive);
        }

        private NftPageView CreateNamePageView(NamesResponse.NameEntry[] names, PoolableObject nftPagePoolElement, int i)
        {
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = CreateNamePageElements(names, i, out (string, string)[] nftIds);
            nftPageView.SetPageElementsContent(pageElements, nftIds);
            nftPageView.IsNftInfoActived = false;
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }

        private static NFTIconComponentModel[] CreateNamePageElements(NamesResponse.NameEntry[] names, int i, out (string, string)[] nftIds)
        {
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
            nftIds = new (string, string)[4];

            for (var j = 0; j < 4; j++)
            {
                if (names.Length > i + j && names[i + j] != null)
                {
                    pageElements[j] = new NFTIconComponentModel
                    {
                        showMarketplaceButton = true,
                        showType = true,
                        type = NAME_TYPE,
                        marketplaceURI = "",
                        name = names[i + j].Name,
                        rarity = LEGENDARY_RARITY,
                        imageURI = ""
                    };

                    nftIds[j] = (names[i + j].ContractAddress, NAME_TYPE);
                }
                else
                {
                    pageElements[j] = null;
                    nftIds[j] = ("", "");
                }
            }

            return pageElements;
        }

        private NftPageView CreateWearablePageView(WearableItem[] wearables, int i)
        {
            PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
            nftPagesPoolableQueue.Add(nftPagePoolElement);
            nftPagePoolElement.gameObject.transform.SetParent(nftWearablesCarouselContent, false);
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
            (string, string)[] nftIds = new (string, string)[4];
            WearableItem[] wearableItemsForThisPage = new WearableItem[4];

            for (int j = 0; j < 4; j++)
            {
                if (wearables.Length > i + j && wearables[i + j] != null)
                {
                    pageElements[j] = new()
                    {
                        showMarketplaceButton = true,
                        showType = true,
                        type = wearables[i + j].data.category,
                        marketplaceURI = "",
                        name = wearables[i + j].GetName(),
                        rarity = wearables[i + j].rarity,
                        imageURI = wearables[i + j].ComposeThumbnailUrl()
                    };

                    nftIds[j] = (wearables[i + j].id, wearables[i + j].data.category);
                    wearableItemsForThisPage[j] = wearables[i + j];
                }
                else
                {
                    pageElements[j] = null;
                    nftIds[j] = ("", "");
                    wearableItemsForThisPage[j] = null;
                }
            }

            nftPageView.SetPageElementsContent(pageElements, nftIds);
            nftPageView.IsNftInfoActived = true;
            nftPageView.ConfigureNFTItemInfo(nftItemInfo, wearableItemsForThisPage, true);
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }

        private NftPageView CreateEmotePageView(WearableItem[] wearables, int i)
        {
            PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
            nftPagesPoolableQueue.Add(nftPagePoolElement);
            nftPagePoolElement.gameObject.transform.SetParent(nftEmotesCarouselContent, false);
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
            (string, string)[] nftIds = new (string, string)[4];
            WearableItem[] wearableItemsForThisPage = new WearableItem[4];

            for (var j = 0; j < 4; j++)
            {
                if (wearables.Length > i + j && wearables[i + j] != null)
                {
                    pageElements[j] = new NFTIconComponentModel
                    {
                        showMarketplaceButton = true,
                        showType = true,
                        type = EMOTE_TYPE,
                        marketplaceURI = "",
                        name = wearables[i + j].GetName(),
                        rarity = wearables[i + j].rarity,
                        imageURI = wearables[i + j].ComposeThumbnailUrl()
                    };

                    nftIds[j] = (wearables[i + j].id, EMOTE_TYPE);
                    wearableItemsForThisPage[j] = wearables[i + j];
                }
                else
                {
                    pageElements[j] = null;
                    nftIds[j] = ("", "");
                    wearableItemsForThisPage[j] = null;
                }
            }

            nftPageView.SetPageElementsContent(pageElements, nftIds);
            nftPageView.IsNftInfoActived = true;
            nftPageView.ConfigureNFTItemInfo(nftItemInfo, wearableItemsForThisPage, false);
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }

        private NftPageView CreateLandPageView(LandsResponse.LandEntry[] lands, int i)
        {
            PoolableObject nftPagePoolElement = nftPagesEntryPool.Get();
            nftPagesPoolableQueue.Add(nftPagePoolElement);
            nftPagePoolElement.gameObject.transform.SetParent(nftLandsCarouselContent, false);
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[4];
            (string, string)[] nftIds = new (string, string)[4];

            for (int j = 0; j < 4; j++)
            {
                if (lands.Length > i + j && lands[i + j] != null)
                {
                    pageElements[j] = new()
                    {
                        showMarketplaceButton = true,
                        showType = true,
                        type = lands[i + j].Category,
                        marketplaceURI = "",
                        name = !string.IsNullOrEmpty(lands[i + j].Name) ? lands[i + j].Name : lands[i + j].Category,
                        rarity = LAND_RARITY,
                        imageURI = lands[i + j].Image
                    };

                    nftIds[j] = (lands[i + j].ContractAddress, lands[i + j].Category);
                }
                else
                {
                    pageElements[j] = null;
                    nftIds[j] = ("", "");
                }
            }

            nftPageView.SetPageElementsContent(pageElements, nftIds);
            nftPageView.IsNftInfoActived = false;
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }
    }
}
