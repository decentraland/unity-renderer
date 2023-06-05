using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UIComponents.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private const int PAGE_SIZE = 4;

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

        [SerializeField] private ButtonComponentView viewAllWearables;
        [SerializeField] private ButtonComponentView viewAllEmotes;
        [SerializeField] private ButtonComponentView viewAllNAMEs;
        [SerializeField] private ButtonComponentView viewAllLANDs;

        private static readonly Vector3 NFT_ICON_SCALE = new Vector3(0.75f, 0.75f, 0.75f);
        public event Action<string, string> OnClickBuyNft;
        public event Action OnClickedLink;
        public event Action OnClickCollectibles;
        public event Action<PassportSection> OnClickedViewAll;
        public event Action<ParcelCoordinates> OnClickDescriptionCoordinates;

        private const string NFT_ICON_POOL_NAME_PREFIX = "NFTIconsEntriesPool_Passport_";
        private const string NFT_PAGES_POOL_NAME_PREFIX = "NFTPagesEntriesPool_Passport_";
        private const int MAX_NFT_ICON_ENTRIES = 20;
        private const int MAX_NFT_PAGES_ENTRIES = 20;
        private const string COORD_LINK_ID = "coord://";

        private List<PoolableObject> nftIconPoolableQueue = new ();
        private List<PoolableObject> nftWearablesPagesPoolableQueue = new ();
        private List<PoolableObject> nftEmotesPagesPoolableQueue = new ();
        private List<PoolableObject> nftNamesPagesPoolableQueue = new ();
        private List<PoolableObject> nftLandsPagesPoolableQueue = new ();

        private Pool nftIconsEntryPool;
        private Pool nftWearablesPagesEntryPool;
        private Pool nftEmotesPagesEntryPool;
        private Pool nftNamesPagesEntryPool;
        private Pool nftLandsPagesEntryPool;
        private readonly List<NFTIconComponentView> equippedNftWearableViews = new List<NFTIconComponentView>();
        private readonly List<NftPageView> ownedNftWearablePageViews = new List<NftPageView>();
        private readonly List<NftPageView> ownedNftEmotePageViews = new List<NftPageView>();

        public void Start()
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

            viewAllWearables.onClick.AddListener(()=>OpenViewAllSection(PassportSection.Wearables));
            viewAllEmotes.onClick.AddListener(()=>OpenViewAllSection(PassportSection.Emotes));
            viewAllNAMEs.onClick.AddListener(()=>OpenViewAllSection(PassportSection.Names));
            viewAllLANDs.onClick.AddListener(()=>OpenViewAllSection(PassportSection.Lands));

            descriptionText.SubscribeToClickEvents(OnDescriptionClicked);
            nftWearablesPagesEntryPool = GetNftPagesEntryPool(NFT_PAGES_POOL_NAME_PREFIX + "Wearables");
            nftEmotesPagesEntryPool = GetNftPagesEntryPool(NFT_PAGES_POOL_NAME_PREFIX + "Emotes");
            nftNamesPagesEntryPool = GetNftPagesEntryPool(NFT_PAGES_POOL_NAME_PREFIX + "Names");
            nftLandsPagesEntryPool = GetNftPagesEntryPool(NFT_PAGES_POOL_NAME_PREFIX + "Lands");
            nftIconsEntryPool = GetNftIconEntryPool();
        }

        private void OpenViewAllSection(PassportSection section)
        {
            CloseAllNFTItemInfos();
            OnClickedViewAll?.Invoke(section);
        }

        public void CloseAllSections()
        {
            aboutPanel.SetActive(false);
            wearablesPanel.SetActive(false);
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
            ReleaseAllNftPagesPoolObjects();
            ReleaseAllNftIconsPoolObjects();
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
                description = AddCoordinateLinks(description);
                descriptionText.text = description;
            }

            SetLinks(links);
        }

        private string AddCoordinateLinks(string description)
        {
            return CoordinateUtils.ReplaceTextCoordinates(description, (text, coordinates) =>
                $"<link=coord://{text}><color=#4886E3><u>{text}</u></color></link>");
        }

        private void OnDescriptionClicked(PointerEventData clickData)
        {
            if (clickData.button != PointerEventData.InputButton.Left) return;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(descriptionText, clickData.position, descriptionText.canvas.worldCamera);
            if (linkIndex == -1) return;

            string link = descriptionText.textInfo.linkInfo[linkIndex].GetLinkID();
            if (!link.StartsWith(COORD_LINK_ID)) return;

            string coordText = link[COORD_LINK_ID.Length..];
            ParcelCoordinates coordinates = CoordinateUtils.ParseCoordinatesString(coordText);
            OnClickDescriptionCoordinates?.Invoke(coordinates);
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
            OnClickedLink?.Invoke();
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
            nftWearablesCarousel.ExtractItems();
            nftWearablesPagesEntryPool.ReleaseAll();
            emptyWearablesText.gameObject.SetActive(wearables.Length <= 0);
            ownedNftWearablePageViews.Clear();

            for (int i = 0; i < wearables.Length; i += PAGE_SIZE)
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
            nftEmotesCarousel.ExtractItems();
            nftEmotesPagesEntryPool.ReleaseAll();
            emptyEmotesText.gameObject.SetActive(wearables.Length <= 0);

            for (int i = 0; i < wearables.Length; i += PAGE_SIZE)
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
            nftNamesCarousel.ExtractItems();
            nftNamesPagesEntryPool.ReleaseAll();
            emptyNamesText.gameObject.SetActive(names.Length <= 0);

            for (int i = 0; i < names.Length; i += PAGE_SIZE)
            {
                PoolableObject nftPagePoolElement = nftNamesPagesEntryPool.Get();
                nftNamesPagesPoolableQueue.Add(nftPagePoolElement);
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
            nftLandsCarousel.ExtractItems();
            nftLandsPagesEntryPool.ReleaseAll();
            emptyLandsText.gameObject.SetActive(lands.Length <= 0);

            for (int i = 0; i < lands.Length; i += PAGE_SIZE)
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

        public void OpenCollectiblesTab()
        {
            wearablesPanel.SetActive(true);
        }

        public void SetInitialPage()
        {
            aboutToggle.isOn = true;
        }

        public void SetViewAllButtonActive(PassportSection section, bool isActive)
        {
            switch (section)
            {
                case PassportSection.Wearables:
                    viewAllWearables.gameObject.SetActive(isActive);
                    break;
                case PassportSection.Emotes:
                    viewAllEmotes.gameObject.SetActive(isActive);
                    break;
                case PassportSection.Names:
                    viewAllNAMEs.gameObject.SetActive(isActive);
                    break;
                case PassportSection.Lands:
                    viewAllLANDs.gameObject.SetActive(isActive);
                    break;
            }
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

        private void ReleaseAllNftIconsPoolObjects()
        {
            ReleaseNftIconsPoolObjects(nftIconPoolableQueue);
        }

        private void ReleaseNftIconsPoolObjects(List<PoolableObject> nftIconsPoolableQueue)
        {
            foreach (var poolObject in nftIconsPoolableQueue)
                poolObject.Release();

            nftIconsPoolableQueue.Clear();
        }

        private void ReleaseAllNftPagesPoolObjects()
        {
            nftWearablesPagesEntryPool.ReleaseAll();
            nftEmotesPagesEntryPool.ReleaseAll();
            nftNamesPagesEntryPool.ReleaseAll();
            nftLandsPagesEntryPool.ReleaseAll();
        }

        private Pool GetNftIconEntryPool() =>
            GetNftEntryPool(NFT_ICON_POOL_NAME_PREFIX + name + GetInstanceID(), wearableUIReferenceObject, MAX_NFT_ICON_ENTRIES);

        private Pool GetNftPagesEntryPool(string poolId) =>
            GetNftEntryPool(poolId, nftPageUIReferenceObject, MAX_NFT_PAGES_ENTRIES);

        private static Pool GetNftEntryPool(string poolId, GameObject referenceObject, int maxPrewarmCount)
        {
            var pool = PoolManager.i.GetPool(poolId);
            if (pool != null) return pool;

            pool = PoolManager.i.AddPool(
                poolId,
                Instantiate(referenceObject).gameObject,
                maxPrewarmCount: maxPrewarmCount,
                isPersistent: true);

            pool.ForcePrewarm(forceActive: false);

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
            NFTIconComponentModel[] pageElements = CreateNamePageElements(names, i);
            nftPageView.SetPageElementsContent(pageElements);
            nftPageView.IsNftInfoActived = false;
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }

        private static NFTIconComponentModel[] CreateNamePageElements(NamesResponse.NameEntry[] names, int i)
        {
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[PAGE_SIZE];

            for (var j = 0; j < PAGE_SIZE; j++)
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
                        rarity = NAME_TYPE,
                        imageURI = "",
                        nftInfo = names[i + j].GetNftInfo(),
                    };
                }
                else
                {
                    pageElements[j] = null;
                }
            }

            return pageElements;
        }

        private NftPageView CreateWearablePageView(WearableItem[] wearables, int i)
        {
            PoolableObject nftPagePoolElement = nftWearablesPagesEntryPool.Get();
            nftWearablesPagesPoolableQueue.Add(nftPagePoolElement);
            nftPagePoolElement.gameObject.transform.SetParent(nftWearablesCarouselContent, false);
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[PAGE_SIZE];
            WearableItem[] wearableItemsForThisPage = new WearableItem[PAGE_SIZE];

            for (int j = 0; j < PAGE_SIZE; j++)
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
                        imageURI = wearables[i + j].ComposeThumbnailUrl(),
                        nftInfo = wearables[i + j].GetNftInfo(),
                    };
                    wearableItemsForThisPage[j] = wearables[i + j];
                }
                else
                {
                    pageElements[j] = null;
                    wearableItemsForThisPage[j] = null;
                }
            }

            nftPageView.SetPageElementsContent(pageElements);
            nftPageView.IsNftInfoActived = true;
            nftPageView.ConfigureNFTItemInfo(nftItemInfo, wearableItemsForThisPage, true);
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }

        private NftPageView CreateEmotePageView(WearableItem[] wearables, int i)
        {
            PoolableObject nftPagePoolElement = nftEmotesPagesEntryPool.Get();
            nftEmotesPagesPoolableQueue.Add(nftPagePoolElement);
            nftPagePoolElement.gameObject.transform.SetParent(nftEmotesCarouselContent, false);
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[PAGE_SIZE];
            WearableItem[] wearableItemsForThisPage = new WearableItem[PAGE_SIZE];

            for (var j = 0; j < PAGE_SIZE; j++)
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
                        imageURI = wearables[i + j].ComposeThumbnailUrl(),
                        nftInfo = wearables[i + j].GetNftInfo(),
                    };

                    wearableItemsForThisPage[j] = wearables[i + j];
                }
                else
                {
                    pageElements[j] = null;
                    wearableItemsForThisPage[j] = null;
                }
            }

            nftPageView.SetPageElementsContent(pageElements);
            nftPageView.IsNftInfoActived = true;
            nftPageView.ConfigureNFTItemInfo(nftItemInfo, wearableItemsForThisPage, false);
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }

        private NftPageView CreateLandPageView(LandsResponse.LandEntry[] lands, int i)
        {
            PoolableObject nftPagePoolElement = nftLandsPagesEntryPool.Get();
            nftLandsPagesPoolableQueue.Add(nftPagePoolElement);
            nftPagePoolElement.gameObject.transform.SetParent(nftLandsCarouselContent, false);
            NftPageView nftPageView = nftPagePoolElement.gameObject.GetComponent<NftPageView>();
            nftPageView.OnClickBuyNft -= ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf -= FocusAnyNtfItem;
            NFTIconComponentModel[] pageElements = new NFTIconComponentModel[PAGE_SIZE];

            for (int j = 0; j < PAGE_SIZE; j++)
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
                        imageURI = lands[i + j].Image,
                        nftInfo = lands[i + j].GetNftInfo(),
                    };
                }
                else
                {
                    pageElements[j] = null;
                }
            }

            nftPageView.SetPageElementsContent(pageElements);
            nftPageView.IsNftInfoActived = false;
            nftPageView.OnClickBuyNft += ClickOnBuyWearable;
            nftPageView.OnFocusAnyNtf += FocusAnyNtfItem;
            return nftPageView;
        }
    }
}
