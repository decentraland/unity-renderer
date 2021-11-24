using DCL;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Categories = WearableLiterals.Categories;

public class AvatarEditorHUDController : IHUD
{
    private const int LOADING_OWNED_WEARABLES_RETRIES = 3;
    private const string LOADING_OWNED_WEARABLES_ERROR_MESSAGE = "There was a problem loading your wearables";
    private const string URL_MARKET_PLACE = "https://market.decentraland.org/browse?section=wearables";
    private const string URL_GET_A_WALLET = "https://docs.decentraland.org/get-a-wallet";
    private const string URL_SELL_COLLECTIBLE_GENERIC = "https://market.decentraland.org/account";
    private const string URL_SELL_SPECIFIC_COLLECTIBLE = "https://market.decentraland.org/contracts/{collectionId}/tokens/{tokenId}";

    protected static readonly string[] categoriesThatMustHaveSelection = { Categories.BODY_SHAPE, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH };
    protected static readonly string[] categoriesToRandomize = { Categories.HAIR, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH, Categories.FACIAL, Categories.HAIR, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET };

    [NonSerialized]
    public bool bypassUpdateAvatarPreview = false;

    private UserProfile userProfile;
    private BaseDictionary<string, WearableItem> catalog;
    bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
    bool isPlayerRendererLoaded => DataStore.i.isPlayerRendererLoaded.Get();
    BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
    private readonly Dictionary<string, List<WearableItem>> wearablesByCategory = new Dictionary<string, List<WearableItem>>();
    protected readonly AvatarEditorHUDModel model = new AvatarEditorHUDModel();

    private ColorList skinColorList;
    private ColorList eyeColorList;
    private ColorList hairColorList;
    private bool prevMouseLockState = false;
    private int ownedWearablesRemainingRequests = LOADING_OWNED_WEARABLES_RETRIES;
    private bool ownedWearablesAlreadyLoaded = false;
    private List<Nft> ownedNftCollectionsL1 = new List<Nft>();
    private List<Nft> ownedNftCollectionsL2 = new List<Nft>();

    public AvatarEditorHUDView view;

    public event Action OnOpen;
    public event Action OnClose;

    public AvatarEditorHUDController() { }

    public void Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog, bool bypassUpdateAvatarPreview = false)
    {
        this.userProfile = userProfile;
        this.bypassUpdateAvatarPreview = bypassUpdateAvatarPreview;

        view = AvatarEditorHUDView.Create(this);

        avatarEditorVisible.OnChange += OnAvatarEditorVisibleChanged;
        OnAvatarEditorVisibleChanged(avatarEditorVisible.Get(), false);
        view.OnCloseActionTriggered += DiscardAndClose;

        skinColorList = Resources.Load<ColorList>("SkinTone");
        hairColorList = Resources.Load<ColorList>("HairColor");
        eyeColorList = Resources.Load<ColorList>("EyeColor");
        view.SetColors(skinColorList.colors, hairColorList.colors, eyeColorList.colors);

        SetCatalog(catalog);

        LoadUserProfile(userProfile, true);
        this.userProfile.OnUpdate += LoadUserProfile;
    }

    public void SetCatalog(BaseDictionary<string, WearableItem> catalog)
    {
        if (this.catalog != null)
        {
            this.catalog.OnAdded -= AddWearable;
            this.catalog.OnRemoved -= RemoveWearable;
        }

        this.catalog = catalog;

        ProcessCatalog(this.catalog);
        this.catalog.OnAdded += AddWearable;
        this.catalog.OnRemoved += RemoveWearable;
    }

    private void LoadUserProfile(UserProfile userProfile)
    {
        LoadUserProfile(userProfile, false);
        QueryNftCollections(userProfile.userId);
    }

    private void LoadOwnedWereables(UserProfile userProfile)
    {
        if (ownedWearablesAlreadyLoaded || ownedWearablesRemainingRequests <= 0 || string.IsNullOrEmpty(userProfile.userId))
            return;

        view.ShowCollectiblesLoadingSpinner(true);
        view.ShowCollectiblesLoadingRetry(false);
        CatalogController.RequestOwnedWearables(userProfile.userId)
            .Then((ownedWearables) =>
            {
                ownedWearablesAlreadyLoaded = true;
                this.userProfile.SetInventory(ownedWearables.Select(x => x.id).ToArray());
                LoadUserProfile(userProfile, true);
                view.ShowCollectiblesLoadingSpinner(false);
            })
            .Catch((error) =>
            {
                ownedWearablesRemainingRequests--;
                if (ownedWearablesRemainingRequests > 0)
                {
                    Debug.LogWarning("Retrying owned wereables loading...");
                    LoadOwnedWereables(userProfile);
                }
                else
                {
                    NotificationsController.i.ShowNotification(new DCL.NotificationModel.Model
                    {
                        message = LOADING_OWNED_WEARABLES_ERROR_MESSAGE,
                        type = DCL.NotificationModel.Type.GENERIC,
                        timer = 10f,
                        destroyOnFinish = true
                    });

                    view.ShowCollectiblesLoadingSpinner(false);
                    view.ShowCollectiblesLoadingRetry(true);
                    Debug.LogError(error);
                }
            });
    }

    private void QueryNftCollections(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userProfile.userId, NftCollectionsLayer.ETHEREUM)
            .Then((nfts) => ownedNftCollectionsL1 = nfts)
            .Catch((error) => Debug.LogError(error));

        DCL.Environment.i.platform.serviceProviders.theGraph.QueryNftCollections(userProfile.userId, NftCollectionsLayer.MATIC)
            .Then((nfts) => ownedNftCollectionsL2 = nfts)
            .Catch((error) => Debug.LogError(error));
    }

    public void RetryLoadOwnedWearables()
    {
        ownedWearablesRemainingRequests = LOADING_OWNED_WEARABLES_RETRIES;
        LoadOwnedWereables(userProfile);
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
                    !wearable.data.tags.Contains("base-wearable"))
                {
                    equippedOwnedWearables.Add(userProfile.avatar.wearables[i]);
                }
            }

            userProfile.SetInventory(equippedOwnedWearables.ToArray());
        }

        LoadUserProfile(userProfile, true);
        DataStore.i.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;
    }

    public void LoadUserProfile(UserProfile userProfile, bool forceLoading)
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

        CatalogController.wearableCatalog.TryGetValue(userProfile.avatar.bodyShape, out var bodyShape);

        if (bodyShape == null)
        {
            return;
        }

        view.SetIsWeb3(userProfile.hasConnectedWeb3);

        ProcessCatalog(this.catalog);
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
                CatalogController.wearableCatalog.TryGetValue(userProfile.avatar.wearables[i], out var wearable);
                if (wearable == null)
                {
                    Debug.LogError($"Couldn't find wearable with ID {userProfile.avatar.wearables[i]}");
                    continue;
                }

                EquipWearable(wearable);
            }
        }

        EnsureWearablesCategoriesNotEmpty();

        UpdateAvatarPreview();
    }

    private void EnsureWearablesCategoriesNotEmpty()
    {
        var categoriesInUse = model.wearables.Select(x => x.data.category).ToArray();
        for (var i = 0; i < categoriesThatMustHaveSelection.Length; i++)
        {
            var category = categoriesThatMustHaveSelection[i];
            if (category != Categories.BODY_SHAPE && !(categoriesInUse.Contains(category)))
            {
                WearableItem wearable;
                var defaultItemId = WearableLiterals.DefaultWearables.GetDefaultWearable(model.bodyShape.id, category);
                if (defaultItemId != null)
                {
                    CatalogController.wearableCatalog.TryGetValue(defaultItemId, out wearable);
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
        CatalogController.wearableCatalog.TryGetValue(wearableId, out var wearable);
        if (wearable == null)
            return;

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
                var sameCategoryEquipped = model.wearables.FirstOrDefault(x => x.data.category == wearable.data.category);
                if (sameCategoryEquipped != null)
                {
                    UnequipWearable(sameCategoryEquipped);
                }

                EquipWearable(wearable);
            }
        }

        UpdateAvatarPreview();
    }

    public void HairColorClicked(Color color)
    {
        EquipHairColor(color);
        view.SelectHairColor(model.hairColor);
        UpdateAvatarPreview();
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
        if (!bypassUpdateAvatarPreview)
            view.UpdateAvatarPreview(model.ToAvatarModel());
    }

    private void EquipHairColor(Color color)
    {
        var colorToSet = color;
        if (!hairColorList.colors.Any(x => x.AproxComparison(colorToSet)))
        {
            colorToSet = hairColorList.colors[hairColorList.defaultColor];
        }

        model.hairColor = colorToSet;
        view.SelectHairColor(model.hairColor);
    }

    private void EquipEyesColor(Color color)
    {
        var colorToSet = color;
        if (!eyeColorList.colors.Any(x => x.AproxComparison(color)))
        {
            colorToSet = eyeColorList.colors[eyeColorList.defaultColor];
        }

        model.eyesColor = colorToSet;
        view.SelectEyeColor(model.eyesColor);
    }

    private void EquipSkinColor(Color color)
    {
        var colorToSet = color;
        if (!skinColorList.colors.Any(x => x.AproxComparison(colorToSet)))
        {
            colorToSet = skinColorList.colors[skinColorList.defaultColor];
        }

        model.skinColor = colorToSet;
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
        if (model.wearables.Contains(wearable))
        {
            model.wearables.Remove(wearable);
            view.UnequipWearable(wearable);
        }
    }

    public void UnequipAllWearables()
    {
        foreach (var wearable in model.wearables)
        {
            view.UnequipWearable(wearable);
        }

        model.wearables.Clear();
    }

    private void ProcessCatalog(BaseDictionary<string, WearableItem> catalog)
    {
        wearablesByCategory.Clear();
        view.RemoveAllWearables();
        using (var iterator = catalog.Get().GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                AddWearable(iterator.Current.Key, iterator.Current.Value);
            }
        }
    }

    private void AddWearable(string id, WearableItem wearable)
    {
        if (!wearable.data.tags.Contains("base-wearable") && userProfile.GetItemAmount(id) == 0)
        {
            return;
        }

        if (!wearablesByCategory.ContainsKey(wearable.data.category))
        {
            wearablesByCategory.Add(wearable.data.category, new List<WearableItem>());
        }

        wearablesByCategory[wearable.data.category].Add(wearable);
        view.AddWearable(wearable, userProfile.GetItemAmount(id));
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
        EquipHairColor(hairColorList.colors[UnityEngine.Random.Range(0, hairColorList.colors.Count)]);
        EquipEyesColor(eyeColorList.colors[UnityEngine.Random.Range(0, eyeColorList.colors.Count)]);

        model.wearables.Clear();
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
                }

                var wearable = supportedWearables[UnityEngine.Random.Range(0, supportedWearables.Length - 1)];
                EquipWearable(wearable);
            }
        }

        UpdateAvatarPreview();
    }

    public List<WearableItem> GetWearablesReplacedBy(WearableItem wearableItem)
    {
        List<WearableItem> wearablesToReplace = new List<WearableItem>();

        HashSet<string> categoriesToReplace = new HashSet<string>(wearableItem.GetReplacesList(model.bodyShape.id) ?? new string[0]);

        int wearableCount = model.wearables.Count;
        for (int i = 0; i < wearableCount; i++)
        {
            var wearable = model.wearables[i];
            if (wearable == null)
                continue;

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

    private float prevRenderScale = 1.0f;
    private Camera mainCamera;

    public void SetVisibility(bool visible) { avatarEditorVisible.Set(visible); }

    private void OnAvatarEditorVisibleChanged(bool current, bool previous) { SetVisibility_Internal(current); }

    public void SetVisibility_Internal(bool visible)
    {
        var currentRenderProfile = DCL.RenderProfileManifest.i.currentProfile;

        if (!visible && view.isOpen)
        {
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);
            DCL.Environment.i.messaging.manager.paused = false;
            currentRenderProfile.avatarProfile.currentProfile = currentRenderProfile.avatarProfile.inWorld;
            currentRenderProfile.avatarProfile.Apply();
            if (prevMouseLockState)
            {
                Utils.LockCursor();
            }

            // NOTE(Brian): SSAO doesn't work correctly with the offseted avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            asset.renderScale = prevRenderScale;

            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
            DataStore.i.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;

            OnClose?.Invoke();
        }
        else if (visible && !view.isOpen)
        {
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(0f);
            LoadOwnedWereables(userProfile);
            DCL.Environment.i.messaging.manager.paused = DataStore.i.isSignUpFlow.Get();
            currentRenderProfile.avatarProfile.currentProfile = currentRenderProfile.avatarProfile.avatarEditor;
            currentRenderProfile.avatarProfile.Apply();

            prevMouseLockState = Utils.isCursorLocked;
            Utils.UnlockCursor();

            // NOTE(Brian): SSAO doesn't work correctly with the offseted avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            prevRenderScale = asset.renderScale;
            asset.renderScale = 1.0f;

            CommonScriptableObjects.isFullscreenHUDOpen.Set(true);
            DataStore.i.isPlayerRendererLoaded.OnChange += PlayerRendererLoaded;

            OnOpen?.Invoke();
        }

        currentRenderProfile.avatarProfile.Apply();
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        avatarEditorVisible.OnChange -= OnAvatarEditorVisibleChanged;
        view.OnCloseActionTriggered -= DiscardAndClose;
        DataStore.i.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;

        CleanUp();
    }

    public void CleanUp()
    {
        UnequipAllWearables();

        if (view != null)
            view.CleanUp();

        this.userProfile.OnUpdate -= LoadUserProfile;
        this.catalog.OnAdded -= AddWearable;
        this.catalog.OnRemoved -= RemoveWearable;
        DataStore.i.isPlayerRendererLoaded.OnChange -= PlayerRendererLoaded;
    }

    public void SetConfiguration(HUDConfiguration configuration) { SetVisibility(configuration.active); }

    public void SaveAvatar(Texture2D faceSnapshot, Texture2D face128Snapshot, Texture2D face256Snapshot, Texture2D bodySnapshot)
    {
        var avatarModel = model.ToAvatarModel();

        WebInterface.SendSaveAvatar(avatarModel, faceSnapshot, face128Snapshot, face256Snapshot, bodySnapshot, DataStore.i.isSignUpFlow.Get());
        userProfile.OverrideAvatar(avatarModel, face256Snapshot);
        if (DataStore.i.isSignUpFlow.Get())
            DataStore.i.HUDs.signupVisible.Set(true);

        SetVisibility(false);
    }

    public void DiscardAndClose()
    {
        if (!view.isOpen)
            return;

        if (!DataStore.i.isSignUpFlow.Get())
            LoadUserProfile(userProfile);
        else
            WebInterface.SendCloseUserAvatar(true);

        SetVisibility(false);
    }

    public void GoToMarketplace()
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
}