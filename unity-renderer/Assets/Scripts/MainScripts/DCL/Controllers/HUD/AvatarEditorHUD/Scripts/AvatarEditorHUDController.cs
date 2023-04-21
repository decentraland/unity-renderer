using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.EmotesCustomization;
using DCL.Helpers;
using DCL.Interface;
using DCL.NotificationModel;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Environment = DCL.Environment;
using Random = UnityEngine.Random;
using Type = DCL.NotificationModel.Type;
using Categories = WearableLiterals.Categories;

public class AvatarEditorHUDController : IHUD
{
    private const string LOADING_OWNED_WEARABLES_ERROR_MESSAGE = "There was a problem loading your wearables";
    private const string URL_MARKET_PLACE = "https://market.decentraland.org/browse?section=wearables";
    private const string URL_GET_A_WALLET = "https://docs.decentraland.org/get-a-wallet";
    private const string URL_SELL_COLLECTIBLE_GENERIC = "https://market.decentraland.org/account";
    private const string URL_SELL_SPECIFIC_COLLECTIBLE = "https://market.decentraland.org/contracts/{collectionId}/tokens/{tokenId}";
    private const string THIRD_PARTY_COLLECTIONS_FEATURE_FLAG = "third_party_collections";
    private const int REQUESTS_COOLDOWN_TIME = 60;
    internal const string EQUIP_WEARABLE_METRIC = "equip_wearable";
    protected static readonly string[] categoriesThatMustHaveSelection = { Categories.BODY_SHAPE, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH };
    protected static readonly string[] categoriesToRandomize = { Categories.HAIR, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH, Categories.FACIAL, Categories.HAIR, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET };

    [NonSerialized]
    public bool bypassUpdateAvatarPreview = false;

    private UserProfile userProfile => userProfileBridge.GetOwn();
    private readonly IAnalytics analytics;
    private readonly INewUserExperienceAnalytics newUserExperienceAnalytics;
    private BaseDictionary<string, WearableItem> catalog;
    private readonly IWearablesCatalogService wearablesCatalogService;
    private readonly IUserProfileBridge userProfileBridge;
    private bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
    private bool isPlayerRendererLoaded => DataStore.i.common.isPlayerRendererLoaded.Get();
    private BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
    private BaseVariable<Transform> configureBackpackInFullscreenMenu => DataStore.i.exploreV2.configureBackpackInFullscreenMenu;
    private BaseVariable<bool> exploreV2IsOpen => DataStore.i.exploreV2.isOpen;
    private DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

    private readonly DataStore_FeatureFlag featureFlags;

    private readonly Dictionary<string, List<WearableItem>> wearablesByCategory = new Dictionary<string, List<WearableItem>>();
    protected readonly AvatarEditorHUDModel model = new AvatarEditorHUDModel();

    private ColorList skinColorList;
    private ColorList eyeColorList;
    private ColorList hairColorList;
    private bool prevMouseLockState = false;
    private bool ownedWearablesAlreadyLoaded = false;
    private List<Nft> ownedNftCollectionsL1 = new List<Nft>();
    private List<Nft> ownedNftCollectionsL2 = new List<Nft>();
    internal bool avatarIsDirty = false;
    private float lastTimeOwnedWearablesChecked = 0;
    private float lastTimeOwnedEmotesChecked = float.MinValue;
    internal bool collectionsAlreadyLoaded = false;
    private float prevRenderScale = 1.0f;
    private bool isAvatarPreviewReady;
    private List<string> thirdPartyWearablesLoaded = new List<string>();
    private List<string> thirdPartyCollectionsActive = new List<string>();
    private CancellationTokenSource loadEmotesCTS = new CancellationTokenSource();
    private CancellationTokenSource loadOwnedWearablesCTS = new CancellationTokenSource();
    private CancellationTokenSource loadThirdPartyWearablesCTS = new CancellationTokenSource();
    private bool loadingWearables;
    private WearableItem[] emotesLoadedAsWearables;
    private AvatarEditorHUDAnimationController avatarEditorHUDAnimationController;
    private IEmotesCustomizationComponentController emotesCustomizationComponentController;
    private bool isThirdPartyCollectionsEnabled => featureFlags.flags.Get().IsFeatureEnabled(THIRD_PARTY_COLLECTIONS_FEATURE_FLAG);
    private Service<IEmotesCatalogService> emotesCatalogService;

    public AvatarEditorHUDView view;

    public event Action OnOpen;
    public event Action OnClose;

    public AvatarEditorHUDController(
        DataStore_FeatureFlag featureFlags,
        IAnalytics analytics,
        IWearablesCatalogService wearablesCatalogService,
        IUserProfileBridge userProfileBridge)
    {
        this.featureFlags = featureFlags;
        this.analytics = analytics;
        this.newUserExperienceAnalytics = new NewUserExperienceAnalytics(analytics);
        this.wearablesCatalogService = wearablesCatalogService;
        this.userProfileBridge = userProfileBridge;
    }

    public void Initialize(
        bool bypassUpdateAvatarPreview = false
        )
    {
        this.bypassUpdateAvatarPreview = bypassUpdateAvatarPreview;

        view = AvatarEditorHUDView.Create(this);

        view.skinsFeatureContainer.SetActive(true);
        avatarEditorVisible.OnChange += OnAvatarEditorVisibleChanged;
        OnAvatarEditorVisibleChanged(avatarEditorVisible.Get(), false);

        configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;
        ConfigureBackpackInFullscreenMenuChanged(configureBackpackInFullscreenMenu.Get(), null);

        exploreV2IsOpen.OnChange += ExploreV2IsOpenChanged;

        skinColorList = Resources.Load<ColorList>("SkinTone");
        hairColorList = Resources.Load<ColorList>("HairColor");
        eyeColorList = Resources.Load<ColorList>("EyeColor");
        view.SetColors(skinColorList.colors, hairColorList.colors, eyeColorList.colors);

        SetCatalog(wearablesCatalogService.WearablesCatalog);

        this.userProfile.OnUpdate += LoadUserProfile;

        view.SetSectionActive(AvatarEditorHUDView.EMOTES_SECTION_INDEX, false);

        InitializeAsyncEmoteController();
    }

    private async void InitializeAsyncEmoteController()
    {
        EmbeddedEmotesSO embeddedEmotesSo = await emotesCatalogService.Ref.GetEmbeddedEmotes();

        emotesCustomizationComponentController = new EmotesCustomizationComponentController(
            DataStore.i.emotesCustomization,
            DataStore.i.emotes,
            DataStore.i.exploreV2,
            DataStore.i.HUDs,
            view.emotesSection.transform);
        //Initialize with embedded emotes
        emotesCustomizationComponentController.SetEmotes(embeddedEmotesSo.emotes);
        view.SetSectionActive(AvatarEditorHUDView.EMOTES_SECTION_INDEX, true);

        emotesCustomizationDataStore.isEmotesCustomizationSelected.OnChange += HandleEmotesCostumizationSelection;
        emotesCustomizationDataStore.currentLoadedEmotes.OnAdded += OnNewEmoteAdded;

        emotesCustomizationComponentController.onEmotePreviewed += OnPreviewEmote;
        emotesCustomizationComponentController.onEmoteEquipped += OnEmoteEquipped;
        emotesCustomizationComponentController.onEmoteUnequipped += OnEmoteUnequipped;
        emotesCustomizationComponentController.onEmoteSell += OnRedirectToEmoteSelling;

        LoadUserProfile(userProfile, true);

        DataStore.i.HUDs.isAvatarEditorInitialized.Set(true);

        view.SetThirdPartyCollectionsVisibility(isThirdPartyCollectionsEnabled);

        this.avatarEditorHUDAnimationController = new AvatarEditorHUDAnimationController(view, wearablesCatalogService);

        Environment.i.serviceLocator.Get<IApplicationFocusService>().OnApplicationFocus += OnApplicationFocus;
    }

    private void SetCatalog(BaseDictionary<string, WearableItem> catalog)
    {
        if (this.catalog != null)
        {
            this.catalog.OnAdded -= OnAdditionalWearableAdded;
            this.catalog.OnRemoved -= OnAdditionalWearableRemoved;
        }

        this.catalog = catalog;

        ProcessCatalog(this.catalog);

        this.catalog.OnAdded += OnAdditionalWearableAdded;
        this.catalog.OnRemoved += OnAdditionalWearableRemoved;
    }

    private void OnAdditionalWearableRemoved(string s, WearableItem item)
    {
        RemoveWearable(s, item);
        view.RefreshSelectorsSize();
    }

    private void OnAdditionalWearableAdded(string id, WearableItem item)
    {
        AddWearable(id, item);
        view.RefreshSelectorsSize();
    }

    private void LoadUserProfile(UserProfile userProfile)
    {
        LoadUserProfile(userProfile, false);
        QueryNftCollections(userProfile.userId);
    }

    private void LoadOwnedWereables(UserProfile userProfile)
    {
        async UniTaskVoid RequestOwnedWearablesAsync(CancellationToken ct)
        {
            lastTimeOwnedWearablesChecked = Time.realtimeSinceStartup;
            loadingWearables = true;

            try
            {
                var ownedWearables = await wearablesCatalogService.RequestOwnedWearablesAsync(
                    userProfile.userId,
                    1,
                    int.MaxValue,
                    true,
                    ct);

                ownedWearablesAlreadyLoaded = true;
                //Prior profile V1 emotes must be retrieved along the wearables, onwards they will be requested separatedly
                this.userProfile.SetInventory(ownedWearables.wearables.Select(x => x.id).Concat(thirdPartyWearablesLoaded));
                LoadUserProfile(userProfile, true);
                if (userProfile != null && userProfile.avatar != null)
                    emotesLoadedAsWearables = ownedWearables.wearables.Where(x => x.IsEmote()).ToArray();
            }
            catch (Exception e)
            {
                LoadUserProfile(userProfile, true);

                NotificationsController.i.ShowNotification(new Model
                {
                    message = LOADING_OWNED_WEARABLES_ERROR_MESSAGE,
                    type = Type.GENERIC,
                    timer = 10f,
                    destroyOnFinish = true
                });

                Debug.LogException(e);
            }
            finally
            {
                loadingWearables = false;
            }
        }

        //If there is no userID we dont fetch owned wearabales
        if(string.IsNullOrEmpty(userProfile.userId))
            return;

        // If wearables are loaded, we are in wearable cooldown, we dont fetch again owned wearables
        if (ownedWearablesAlreadyLoaded && IsWearableUpdateInCooldown())
            return;

        loadOwnedWearablesCTS = loadOwnedWearablesCTS.SafeRestart();
        RequestOwnedWearablesAsync(loadOwnedWearablesCTS.Token).Forget();
    }

    private void LoadOwnedEmotes()
    {
        //Only check emotes once every 60 seconds
        if (IsEmotesUpdateInCooldown())
            return;

        lastTimeOwnedEmotesChecked = Time.realtimeSinceStartup;
        //TODO only request OwnedEmotes once every minute

        loadEmotesCTS.SafeCancelAndDispose();
        // we only follow this flow with new profiles
        if (userProfile?.avatar != null)
        {
            loadEmotesCTS = loadEmotesCTS.SafeRestart();
            LoadOwnedEmotesTask(loadEmotesCTS.Token);
        }
    }

    private async UniTaskVoid LoadOwnedEmotesTask(CancellationToken ct = default, int retries = 3)
    {
        var emotesCatalog = Environment.i.serviceLocator.Get<IEmotesCatalogService>();
        try
        {
            EmbeddedEmotesSO embeddedEmoteTask = await emotesCatalogService.Ref.GetEmbeddedEmotes();
            var embeddedEmotes = embeddedEmoteTask.emotes;
            var emotes = await emotesCatalog.RequestOwnedEmotesAsync(userProfile.userId, ct);
            var emotesList = emotes == null ? embeddedEmotes.Cast<WearableItem>().ToList() : emotes.Concat(embeddedEmotes).ToList();
            var emotesFilter = new HashSet<string>();
            foreach (var e in emotesList)
                emotesFilter.Add(e.id);

            if (loadingWearables)
                await UniTaskUtils.WaitForBoolean(ref loadingWearables, false, cancellationToken: ct);

            if (emotesLoadedAsWearables != null)
            {
                foreach (var emoteAsWearable in emotesLoadedAsWearables)
                {
                    if (emotesFilter.Contains(emoteAsWearable.id))
                        continue;

                    emotesList.Add(emoteAsWearable);
                }

                emotesLoadedAsWearables = null;
            }

            emotesCustomizationDataStore.UnequipMissingEmotes(emotesList);
            emotesCustomizationComponentController.SetEmotes(emotesList.ToArray());

        }
        catch (Exception e)
        {
            OperationCanceledException opCanceled = e as OperationCanceledException;
            // If the cancellation was requested upwards, dont retry
            if (opCanceled != null && ct.IsCancellationRequested)
                return;

            if (retries > 0)
            {
                LoadOwnedEmotesTask(ct, retries - 1);
            }
            else
            {
                if (opCanceled == null) // Ignore operation canceled exceptions when logging
                    Debug.LogWarning(e.ToString());
                const string ERROR = "There was a problem loading your emotes";
                NotificationsController.i.ShowNotification(new Model
                {
                    message = ERROR,
                    type = Type.GENERIC,
                    timer = 10f,
                    destroyOnFinish = true
                });
            }
        }
    }

    private void QueryNftCollections(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userProfile.userId, NftCollectionsLayer.ETHEREUM)
           .Then((nfts) => ownedNftCollectionsL1 = nfts)
           .Catch((error) => Debug.LogError(error));

        Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userProfile.userId, NftCollectionsLayer.MATIC)
           .Then((nfts) => ownedNftCollectionsL2 = nfts)
           .Catch((error) => Debug.LogError(error));
    }

    private void PlayerRendererLoaded(bool current, bool previous)
    {
        if (!current)
            return;

        if (!ownedWearablesAlreadyLoaded)
        {
            List<string> equippedOwnedWearables = new List<string>();
            for (int i = 0; i < userProfile.avatar.wearables.Count; i++)
            {
                if (catalog.TryGetValue(userProfile.avatar.wearables[i], out WearableItem wearable) &&
                    !wearable.data.tags.Contains(WearableLiterals.Tags.BASE_WEARABLE))
                {
                    equippedOwnedWearables.Add(userProfile.avatar.wearables[i]);
                }
            }

            userProfile.SetInventory(equippedOwnedWearables);
        }

        LoadUserProfile(userProfile, true);
        DataStore.i.common.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;
    }

    private void LoadUserProfile(UserProfile userProfile, bool forceLoading)
    {
        bool avatarEditorNotVisible = renderingEnabled && !view.isOpen;
        bool isPlaying = !Application.isBatchMode;

        if (!forceLoading)
        {
            if (isPlaying && avatarEditorNotVisible)
                return;
        }

        if (userProfile == null)
            return;

        if (userProfile.avatar == null || string.IsNullOrEmpty(userProfile.avatar.bodyShape))
            return;

        view.InitializeNavigationEvents(!userProfile.hasConnectedWeb3);

        wearablesCatalogService.WearablesCatalog.TryGetValue(userProfile.avatar.bodyShape, out var bodyShape);

        if (bodyShape == null)
        {
            return;
        }

        view.SetIsWeb3(userProfile.hasConnectedWeb3);

        ProcessCatalog(catalog);

        if (avatarIsDirty)
            return;

        EquipBodyShape(bodyShape);
        EquipSkinColor(userProfile.avatar.skinColor);
        EquipHairColor(userProfile.avatar.hairColor);
        EquipEyesColor(userProfile.avatar.eyeColor);

        model.wearables.Clear();
        view.UnselectAllWearables();

        int wearablesCount = userProfile.avatar.wearables.Count;

        if (isPlayerRendererLoaded)
        {
            for (var i = 0; i < wearablesCount; i++)
            {
                wearablesCatalogService.WearablesCatalog.TryGetValue(userProfile.avatar.wearables[i], out var wearable);
                if (wearable == null)
                {
                    Debug.LogError($"Couldn't find wearable with ID {userProfile.avatar.wearables[i]}");
                    continue;
                }

                if (wearable.IsEmote())
                    EquipEmote(wearable);
                else
                    EquipWearable(wearable);
            }
        }

        EnsureWearablesCategoriesNotEmpty();

        UpdateAvatarPreview();
        isAvatarPreviewReady = true;
    }

    private void EnsureWearablesCategoriesNotEmpty()
    {
        var categoriesInUse = model.wearables
            .Where(x => !x.IsEmote())
            .Select(x => x.data.category).ToArray();

        for (var i = 0; i < categoriesThatMustHaveSelection.Length; i++)
        {
            var category = categoriesThatMustHaveSelection[i];
            if (category != Categories.BODY_SHAPE && !(categoriesInUse.Contains(category)))
            {
                WearableItem wearable;
                var defaultItemId = WearableLiterals.DefaultWearables.GetDefaultWearable(model.bodyShape.id, category);
                if (defaultItemId != null)
                {
                    wearablesCatalogService.WearablesCatalog.TryGetValue(defaultItemId, out wearable);
                }
                else
                {
                    wearable = wearablesByCategory[category].FirstOrDefault(x => x.SupportsBodyShape(model.bodyShape.id));
                }

                if (wearable != null)
                {
                    EquipWearable(wearable);
                }
            }
        }
    }

    public void WearableClicked(string wearableId)
    {
        wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out var wearable);
        if (wearable == null) return;

        if (wearable.data.category == Categories.BODY_SHAPE)
        {
            if (wearable.id == model.bodyShape.id)
                return;
            EquipBodyShape(wearable);
        }
        else
        {
            if (model.wearables.Contains(wearable))
            {
                if (!categoriesThatMustHaveSelection.Contains(wearable.data.category))
                {
                    UnequipWearable(wearable);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (IsTryingToReplaceSkin(wearable))
                    UnequipWearable(model.GetWearable(Categories.SKIN));

                var sameCategoryEquipped = model.GetWearable(wearable.data.category);
                if (sameCategoryEquipped != null)
                    UnequipWearable(sameCategoryEquipped);

                EquipWearable(wearable);
            }
        }

        UpdateAvatarPreview();
        view.AddFeedbackOnAppear();
        avatarIsDirty = true;
    }

    public void HairColorClicked(Color color)
    {
        EquipHairColor(color);
        view.SelectHairColor(model.hairColor);
        UpdateAvatarPreview();
        view.AddFeedbackOnAppear();
    }

    public void SkinColorClicked(Color color)
    {
        EquipSkinColor(color);
        view.SelectSkinColor(model.skinColor);
        UpdateAvatarPreview();
    }

    public void EyesColorClicked(Color color)
    {
        EquipEyesColor(color);
        view.SelectEyeColor(model.eyesColor);
        UpdateAvatarPreview();
    }

    protected virtual void UpdateAvatarPreview()
    {
        if (bypassUpdateAvatarPreview)
            return;

        AvatarModel modelToUpdate = model.ToAvatarModel();

        // We always keep the loaded emotes into the Avatar Preview
        foreach (string emoteId in emotesCustomizationDataStore.currentLoadedEmotes.Get())
        {
            modelToUpdate.emotes.Add(new AvatarModel.AvatarEmoteEntry() { urn = emoteId });
        }

        view.UpdateAvatarPreview(modelToUpdate);
    }

    private void EquipHairColor(Color color)
    {
        model.hairColor = color;
        view.SelectHairColor(model.hairColor);
    }

    private void EquipEyesColor(Color color)
    {
        model.eyesColor = color;
        view.SelectEyeColor(model.eyesColor);
    }

    private void EquipSkinColor(Color color)
    {
        model.skinColor = color;
        view.SelectSkinColor(model.skinColor);
    }

    private void EquipBodyShape(WearableItem bodyShape)
    {
        if (bodyShape.data.category != Categories.BODY_SHAPE)
        {
            Debug.LogError($"Item ({bodyShape.id} is not a body shape");
            return;
        }

        if (model.bodyShape == bodyShape)
            return;

        model.bodyShape = bodyShape;
        emotesCustomizationComponentController.SetEquippedBodyShape(bodyShape.id);
        view.UpdateSelectedBody(bodyShape);

        int wearablesCount = model.wearables.Count;
        for (var i = wearablesCount - 1; i >= 0; i--)
        {
            UnequipWearable(model.wearables[i]);
        }

        var defaultWearables = WearableLiterals.DefaultWearables.GetDefaultWearables(bodyShape.id);
        for (var i = 0; i < defaultWearables.Length; i++)
        {
            if (catalog.TryGetValue(defaultWearables[i], out var wearable))
                EquipWearable(wearable);
        }
    }

    private void EquipWearable(WearableItem wearable)
    {
        if (wearable.IsEmote())
            return;

        if (!wearablesByCategory.ContainsKey(wearable.data.category))
            return;

        if (wearablesByCategory[wearable.data.category].Contains(wearable) && wearable.SupportsBodyShape(model.bodyShape.id) && !model.wearables.Contains(wearable))
        {
            var toReplace = GetWearablesReplacedBy(wearable);
            toReplace.ForEach(UnequipWearable);
            model.wearables.Add(wearable);
            view.EquipWearable(wearable);
        }
    }

    private void UnequipWearable(WearableItem wearable)
    {
        if (wearable.IsEmote())
            return;

        if (model.wearables.Contains(wearable))
        {
            model.wearables.Remove(wearable);
            view.UnequipWearable(wearable);
            avatarIsDirty = true;
        }
    }

    private void EquipEmote(WearableItem emote)
    {
        if (!emote.IsEmote())
            return;
        avatarIsDirty = true;
    }

    private void UnequipEmote(WearableItem emote)
    {
        if (!emote.IsEmote())
            return;

        avatarIsDirty = true;
    }

    public void UnequipAllWearables()
    {
        foreach (var wearable in model.wearables)
        {
            if (!wearable.IsEmote())
                view.UnequipWearable(wearable);
        }

        model.wearables.Clear();
    }

    private void ProcessCatalog(BaseDictionary<string, WearableItem> catalog)
    {
        wearablesByCategory.Clear();
        view.RemoveAllWearables();
        bool hasSkin = false;
        bool hasCollectible = false;
        using (var iterator = catalog.Get().GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.IsEmote())
                    continue;

                if (iterator.Current.Value.IsFromThirdPartyCollection
                    && !thirdPartyCollectionsActive.Contains(iterator.Current.Value.ThirdPartyCollectionId))
                    continue;

                AddWearable(iterator.Current.Key, iterator.Current.Value);
                hasSkin = iterator.Current.Value.IsSkin() || hasSkin;
                hasCollectible = iterator.Current.Value.IsCollectible() || hasCollectible;
            }
        }
        view.ShowSkinPopulatedList(hasSkin);
        view.ShowCollectiblesPopulatedList(hasCollectible);
        view.RefreshSelectorsSize();
    }

    private void AddWearable(string id, WearableItem wearable)
    {
        if (!wearable.data.tags.Contains(WearableLiterals.Tags.BASE_WEARABLE) && userProfile.GetItemAmount(id) == 0)
            return;

        if (!wearablesByCategory.ContainsKey(wearable.data.category))
            wearablesByCategory.Add(wearable.data.category, new List<WearableItem>());

        wearablesByCategory[wearable.data.category].Add(wearable);
        view.AddWearable(wearable, userProfile.GetItemAmount(id),
            ShouldShowHideOtherWearablesToast,
            ShouldShowReplaceOtherWearablesToast,
            ShouldShowIncompatibleWearableToast);
    }

    private void RemoveWearable(string id, WearableItem wearable)
    {
        if (wearablesByCategory.ContainsKey(wearable.data.category))
        {
            if (wearablesByCategory[wearable.data.category].Remove(wearable))
            {
                if (wearablesByCategory[wearable.data.category].Count == 0)
                {
                    wearablesByCategory.Remove(wearable.data.category);
                }
            }
        }

        view.RemoveWearable(wearable);
    }

    public void RandomizeWearables()
    {
        EquipHairColor(view.GetRandomColor());
        EquipEyesColor(view.GetRandomColor());

        List<WearableItem> wearablesToRemove = model.wearables.Where(x => !x.IsEmote()).ToList();
        foreach (var wearable in wearablesToRemove)
        {
            model.wearables.Remove(wearable);
        }

        view.UnselectAllWearables();
        using (var iterator = wearablesByCategory.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                string category = iterator.Current.Key;
                if (!categoriesToRandomize.Contains(category))
                {
                    continue;
                }

                var supportedWearables = iterator.Current.Value.Where(x => x.SupportsBodyShape(model.bodyShape.id)).ToArray();
                if (supportedWearables.Length == 0)
                {
                    Debug.LogError($"Couldn't get any wearable for category {category} and bodyshape {model.bodyShape.id}");
                    continue;
                }

                var wearable = supportedWearables[Random.Range(0, supportedWearables.Length - 1)];
                EquipWearable(wearable);
            }
        }

        UpdateAvatarPreview();
        view.AddFeedbackOnAppear();
    }

    private List<WearableItem> GetWearablesReplacedBy(WearableItem wearableItem)
    {
        var wearablesToReplace = new List<WearableItem>();
        var categoriesToReplace = new HashSet<string>(wearableItem.GetReplacesList(model.bodyShape.id) ?? new string[0]);

        int wearableCount = model.wearables.Count;
        for (int i = 0; i < wearableCount; i++)
        {
            var wearable = model.wearables[i];
            if (wearable == null) continue;

            if (categoriesToReplace.Contains(wearable.data.category))
            {
                wearablesToReplace.Add(wearable);
            }
            else
            {
                //For retrocompatibility's sake we check current wearables against new one (compatibility matrix is symmetrical)
                HashSet<string> replacesList = new HashSet<string>(wearable.GetReplacesList(model.bodyShape.id) ?? new string[0]);
                if (replacesList.Contains(wearableItem.data.category))
                {
                    wearablesToReplace.Add(wearable);
                }
            }
        }

        return wearablesToReplace;
    }

    public void SetVisibility(bool visible) { avatarEditorVisible.Set(visible); }

    private void OnAvatarEditorVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }

    private void SetVisibility_Internal(bool visible)
    {
        bool isSignUpFlow = DataStore.i.common.isSignUpFlow.Get();
        if (!visible && view.isOpen)
        {
            view.ResetPreviewEmote();

            if (isSignUpFlow)
                DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);

            Environment.i.messaging.manager.paused = false;
            DataStore.i.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InWorld);
            if (prevMouseLockState && isSignUpFlow)
            {
                Utils.LockCursor();
            }

            // NOTE(Brian): SSAO doesn't work correctly with the offseted avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            asset.renderScale = prevRenderScale;

            if (isSignUpFlow)
                CommonScriptableObjects.isFullscreenHUDOpen.Set(false);

            DataStore.i.common.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;

            OnClose?.Invoke();
        }
        else if (visible && !view.isOpen)
        {
            if (isSignUpFlow)
            {
                DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(0f);
                view.sectionSelector.Hide(true);
            }
            else
            {
                view.sectionSelector.Show(true);
            }

            if (!isSignUpFlow)
            {
                LoadOwnedWereables(userProfile);
                LoadOwnedEmotes();
            }

            LoadCollections();
            Environment.i.messaging.manager.paused = isSignUpFlow;
            DataStore.i.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InEditor);

            prevMouseLockState = Utils.IsCursorLocked;

            if (isSignUpFlow || !DataStore.i.exploreV2.isInitialized.Get())
                Utils.UnlockCursor();

            // NOTE(Brian): SSAO doesn't work correctly with the offseted avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            prevRenderScale = asset.renderScale;
            asset.renderScale = 1.0f;

            if (isSignUpFlow)
                CommonScriptableObjects.isFullscreenHUDOpen.Set(true);

            DataStore.i.common.isPlayerRendererLoaded.OnChange += PlayerRendererLoaded;

            OnOpen?.Invoke();
        }

        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        loadEmotesCTS.SafeCancelAndDispose();
        loadEmotesCTS = null;

        loadOwnedWearablesCTS.SafeCancelAndDispose();
        loadOwnedWearablesCTS = null;

        loadThirdPartyWearablesCTS.SafeCancelAndDispose();
        loadThirdPartyWearablesCTS = null;

        avatarEditorVisible.OnChange -= OnAvatarEditorVisibleChanged;
        configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
        DataStore.i.common.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;
        exploreV2IsOpen.OnChange -= ExploreV2IsOpenChanged;
        emotesCustomizationDataStore.isEmotesCustomizationSelected.OnChange -= HandleEmotesCostumizationSelection;
        emotesCustomizationDataStore.currentLoadedEmotes.OnAdded -= OnNewEmoteAdded;

        emotesCustomizationComponentController.onEmotePreviewed -= OnPreviewEmote;
        emotesCustomizationComponentController.onEmoteEquipped -= OnEmoteEquipped;
        emotesCustomizationComponentController.onEmoteUnequipped -= OnEmoteUnequipped;
        emotesCustomizationComponentController.onEmoteSell -= OnRedirectToEmoteSelling;

        avatarEditorHUDAnimationController.Dispose();
        Environment.i.serviceLocator.Get<IApplicationFocusService>().OnApplicationFocus -= OnApplicationFocus;

        CleanUp();
    }

    public void CleanUp()
    {
        UnequipAllWearables();

        if (view != null)
            view.Dispose();

        this.userProfile.OnUpdate -= LoadUserProfile;
        this.catalog.OnAdded -= AddWearable;
        this.catalog.OnRemoved -= RemoveWearable;
        DataStore.i.common.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;
    }

    public void SetConfiguration(HUDConfiguration configuration) { SetVisibility(configuration.active); }

    public void SaveAvatar(Texture2D face256Snapshot, Texture2D bodySnapshot)
    {
        var avatarModel = model.ToAvatarModel();

        // Add the equipped emotes to the avatar model
        List<AvatarModel.AvatarEmoteEntry> emoteEntries = new List<AvatarModel.AvatarEmoteEntry>();
        int equippedEmotesCount = emotesCustomizationDataStore.unsavedEquippedEmotes.Count();
        for (int i = 0; i < equippedEmotesCount; i++)
        {
            var equippedEmote = emotesCustomizationDataStore.unsavedEquippedEmotes[i];
            if (equippedEmote == null)
                continue;

            emoteEntries.Add(new AvatarModel.AvatarEmoteEntry { slot = i, urn = equippedEmote.id });
        }

        avatarModel.emotes = emoteEntries;

        SendNewEquippedWearablesAnalytics(userProfile.avatar.wearables, avatarModel.wearables);
        emotesCustomizationDataStore.equippedEmotes.Set(emotesCustomizationDataStore.unsavedEquippedEmotes.Get());

        WebInterface.SendSaveAvatar(avatarModel, face256Snapshot, bodySnapshot, DataStore.i.common.isSignUpFlow.Get());
        userProfile.OverrideAvatar(avatarModel, face256Snapshot);

        if (DataStore.i.common.isSignUpFlow.Get())
        {
            DataStore.i.HUDs.signupVisible.Set(true);
            newUserExperienceAnalytics.AvatarEditSuccessNux();
        }

        avatarIsDirty = false;
        SetVisibility(false);
    }

    public void GoToMarketplaceOrConnectWallet()
    {
        if (userProfile.hasConnectedWeb3)
            WebInterface.OpenURL(URL_MARKET_PLACE);
        else
            WebInterface.OpenURL(URL_GET_A_WALLET);
    }

    public void SellCollectible(string collectibleId)
    {
        var ownedCollectible = ownedNftCollectionsL1.FirstOrDefault(nft => nft.urn == collectibleId);
        if (ownedCollectible == null)
            ownedCollectible = ownedNftCollectionsL2.FirstOrDefault(nft => nft.urn == collectibleId);

        if (ownedCollectible != null)
            WebInterface.OpenURL(URL_SELL_SPECIFIC_COLLECTIBLE.Replace("{collectionId}", ownedCollectible.collectionId).Replace("{tokenId}", ownedCollectible.tokenId));
        else
            WebInterface.OpenURL(URL_SELL_COLLECTIBLE_GENERIC);
    }

    public void ToggleVisibility() { SetVisibility(!view.isOpen); }

    private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) { view.SetAsFullScreenMenuMode(currentParentTransform); }

    private void ExploreV2IsOpenChanged(bool current, bool previous)
    {
        if (!current && avatarIsDirty)
        {
            avatarIsDirty = false;

            LoadUserProfile(userProfile, true);

            emotesCustomizationComponentController.RestoreEmoteSlots();
        }
    }

    private void LoadCollections()
    {
        if (!isThirdPartyCollectionsEnabled || collectionsAlreadyLoaded)
            return;

        WearablesFetchingHelper.GetThirdPartyCollections()
            .Then((collections) =>
            {
                view.LoadCollectionsDropdown(collections);
                collectionsAlreadyLoaded = true;
                LoadUserThirdPartyWearables();
            })
            .Catch((error) => Debug.LogError(error));
    }

    private void LoadUserThirdPartyWearables()
    {
        List<string> collectionIdsToLoad = new List<string>();
        foreach (string wearableId in userProfile.avatar.wearables)
        {
            wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out var wearable);

            if (wearable != null && wearable.IsFromThirdPartyCollection)
            {
                if (!collectionIdsToLoad.Contains(wearable.ThirdPartyCollectionId))
                    collectionIdsToLoad.Add(wearable.ThirdPartyCollectionId);
            }
        }

        foreach (string collectionId in collectionIdsToLoad)
        {
            view.ToggleThirdPartyCollection(collectionId, true);
        }
    }

    public void ToggleThirdPartyCollection(bool isOn, string collectionId, string _)
    {
        if (isOn)
            FetchAndShowThirdPartyCollection(collectionId);
        else
            RemoveThirdPartyCollection(collectionId);
    }

    private void FetchAndShowThirdPartyCollection(string collectionId)
    {
        async UniTaskVoid RequestThirdPartyWearablesAsync(CancellationToken ct)
        {
            try
            {
                var wearables = await wearablesCatalogService.RequestThirdPartyWearablesByCollectionAsync(
                    userProfile.userId,
                    collectionId,
                    1,
                    int.MaxValue,
                    true,
                    ct);

                if (wearables.wearables.Count.Equals(0)) view.ShowNoItemOfWearableCollectionWarning();
                thirdPartyCollectionsActive.Add(collectionId);
                foreach (var wearable in wearables.wearables)
                {
                    if (!userProfile.ContainsInInventory(wearable.id))
                    {
                        userProfile.AddToInventory(wearable.id);

                        if (!thirdPartyWearablesLoaded.Contains(wearable.id))
                            thirdPartyWearablesLoaded.Add(wearable.id);
                    }
                }

                view.BlockCollectionsDropdown(false);
                LoadUserProfile(userProfile, true);
                view.RefreshSelectorsSize();
            }
            catch (Exception e)
            {
                view.BlockCollectionsDropdown(false);
                Debug.LogError(e.Message);
            }
        }

        view.BlockCollectionsDropdown(true);
        RequestThirdPartyWearablesAsync(loadThirdPartyWearablesCTS.Token).Forget();
    }

    private void RemoveThirdPartyCollection(string collectionId)
    {
        var wearablesToRemove = wearablesCatalogService.WearablesCatalog
                                                       .Where(wearable => !userProfile.HasEquipped(wearable.Key)
                                                                          && wearable.Value.ThirdPartyCollectionId == collectionId)
                                                       .Select(item => item.Key)
                                                       .ToList();

        thirdPartyCollectionsActive.Remove(collectionId);

        foreach (string wearableId in wearablesToRemove)
        {
            wearablesCatalogService.RemoveWearableFromCatalog(wearableId);
            userProfile.RemoveFromInventory(wearableId);
            thirdPartyWearablesLoaded.Remove(wearableId);
        }

        LoadUserProfile(userProfile, true);
    }

    private bool ShouldShowHideOtherWearablesToast(WearableItem wearable)
    {
        var isWearingSkinAlready = model.wearables.Any(item => item.IsSkin());
        return wearable.IsSkin() && !isWearingSkinAlready;
    }

    private bool ShouldShowIncompatibleWearableToast(WearableItem wearable)
    {
        if(wearable.data.category == Categories.BODY_SHAPE || wearable.data.category == Categories.SKIN)
            return false;
        else
            return !wearable.SupportsBodyShape(model.bodyShape.id);
    }

    private bool IsTryingToReplaceSkin(WearableItem wearable)
    {
        return model.wearables.Any(skin =>
        {
            return skin.IsSkin()
                   && skin.DoesHide(wearable.data.category, model.bodyShape.id);
        });
    }

    private bool ShouldShowReplaceOtherWearablesToast(WearableItem wearable)
    {
        if (IsTryingToReplaceSkin(wearable)) return true;
        var toReplace = GetWearablesReplacedBy(wearable);
        if (wearable == null || toReplace.Count == 0) return false;
        if (model.wearables.Contains(wearable)) return false;

        // NOTE: why just 1?
        if (toReplace.Count == 1)
        {
            var w = toReplace[0];
            if (w.data.category == wearable.data.category)
                return false;
        }
        return true;
    }


    private void HandleEmotesCostumizationSelection(bool current, bool previous)
    {
        if (!current)
            return;

        view.sectionSelector.GetSection(AvatarEditorHUDView.EMOTES_SECTION_INDEX).SelectToggle();
    }

    private void OnNewEmoteAdded(string emoteId)
    {
        if (!isAvatarPreviewReady)
            return;

        UpdateAvatarPreview();
    }

    private void OnPreviewEmote(string emoteId) { view.PlayPreviewEmote(emoteId); }

    private void OnEmoteEquipped(string emoteId)
    {
        catalog.TryGetValue(emoteId, out WearableItem equippedEmote);

        if (equippedEmote != null)
            EquipEmote(equippedEmote);
    }

    private void OnEmoteUnequipped(string emoteId)
    {
        catalog.TryGetValue(emoteId, out WearableItem unequippedEmote);

        if (unequippedEmote != null)
            UnequipEmote(unequippedEmote);
    }

    private void OnRedirectToEmoteSelling(string emoteId) { SellCollectible(emoteId); }

    internal void SendNewEquippedWearablesAnalytics(List<string> oldWearables, List<string> newWearables)
    {
        for (int i = 0; i < newWearables.Count; i++)
        {
            if (oldWearables.Contains(newWearables[i]))
                continue;

            catalog.TryGetValue(newWearables[i], out WearableItem newEquippedEmote);
            if (newEquippedEmote != null && !newEquippedEmote.IsEmote())
                SendEquipWearableAnalytic(newEquippedEmote);
        }
    }

    private void SendEquipWearableAnalytic(WearableItem equippedWearable)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("name", equippedWearable.GetName());
        data.Add("rarity", equippedWearable.rarity);
        data.Add("category", equippedWearable.data.category);
        data.Add("linked_wearable", equippedWearable.IsFromThirdPartyCollection.ToString());
        data.Add("third_party_collection_id", equippedWearable.ThirdPartyCollectionId);
        data.Add("is_in_l2", equippedWearable.IsInL2().ToString());
        data.Add("smart_item", equippedWearable.IsSmart().ToString());

        analytics.SendAnalytic(EQUIP_WEARABLE_METRIC, data);
    }

    private bool IsWearableUpdateInCooldown()
    {
        return Time.realtimeSinceStartup < lastTimeOwnedWearablesChecked + REQUESTS_COOLDOWN_TIME;
    }

    private bool IsEmotesUpdateInCooldown()
    {
        return Time.realtimeSinceStartup < lastTimeOwnedEmotesChecked + REQUESTS_COOLDOWN_TIME;
    }

    private void OnApplicationFocus()
    {
        lastTimeOwnedWearablesChecked = -REQUESTS_COOLDOWN_TIME;
        lastTimeOwnedEmotesChecked = -REQUESTS_COOLDOWN_TIME;
    }
}
