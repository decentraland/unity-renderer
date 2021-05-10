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
    private const string URL_SELL_COLLECTIBLE = "https://market.decentraland.org/account";

    protected static readonly string[] categoriesThatMustHaveSelection = { Categories.BODY_SHAPE, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH };
    protected static readonly string[] categoriesToRandomize = { Categories.HAIR, Categories.EYES, Categories.EYEBROWS, Categories.MOUTH, Categories.FACIAL, Categories.HAIR, Categories.UPPER_BODY, Categories.LOWER_BODY, Categories.FEET };

    [NonSerialized]
    public bool bypassUpdateAvatarPreview = false;

    private UserProfile userProfile;
    private BaseDictionary<string, WearableItem> catalog;
    bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
    bool isPlayerRendererLoaded => DataStore.i.isPlayerRendererLoaded.Get();
    private readonly Dictionary<string, List<WearableItem>> wearablesByCategory = new Dictionary<string, List<WearableItem>>();
    protected readonly AvatarEditorHUDModel model = new AvatarEditorHUDModel();

    private ColorList skinColorList;
    private ColorList eyeColorList;
    private ColorList hairColorList;
    private bool prevMouseLockState = false;
    private int ownedWearablesRemainingRequests = LOADING_OWNED_WEARABLES_RETRIES;
    private bool ownedWearablesAlreadyLoaded = false;

    public AvatarEditorHUDView view;

    public event Action OnOpen;
    public event Action OnClose;

    public AvatarEditorHUDController() { }

    public void Initialize(UserProfile userProfile, BaseDictionary<string, WearableItem> catalog, bool bypassUpdateAvatarPreview = false)
    {
        this.userProfile = userProfile;
        this.bypassUpdateAvatarPreview = bypassUpdateAvatarPreview;

        view = AvatarEditorHUDView.Create(this);

        view.OnToggleActionTriggered += ToggleVisibility;
        view.OnCloseActionTriggered += DiscardAndClose;

        skinColorList = Resources.Load<ColorList>("SkinTone");
        hairColorList = Resources.Load<ColorList>("HairColor");
        eyeColorList = Resources.Load<ColorList>("EyeColor");
        view.SetColors(skinColorList.colors, hairColorList.colors, eyeColorList.colors);

        SetCatalog(catalog);

        LoadUserProfile(userProfile, true);
        this.userProfile.OnUpdate += LoadUserProfile;
        DataStore.i.isPlayerRendererLoaded.OnChange += PlayerRendererLoaded;
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
        LoadOwnedWereables(userProfile);
        LoadUserProfile(userProfile, false);
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
                                 NotificationsController.i.ShowNotification(new Notification.Model
                                 {
                                     message = LOADING_OWNED_WEARABLES_ERROR_MESSAGE,
                                     type = NotificationFactory.Type.GENERIC,
                                     timer = 10f,
                                     destroyOnFinish = true
                                 });

                                 view.ShowCollectiblesLoadingSpinner(false);
                                 view.ShowCollectiblesLoadingRetry(true);
                                 Debug.LogError(error);
                             }
                         });
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
                    !wearable.tags.Contains("base-wearable"))
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
        var categoriesInUse = model.wearables.Select(x => x.category).ToArray();
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

        if (wearable.category == Categories.BODY_SHAPE)
        {
            EquipBodyShape(wearable);
        }
        else
        {
            if (model.wearables.Contains(wearable))
            {
                if (!categoriesThatMustHaveSelection.Contains(wearable.category))
                {
                    UnequipWearable(wearable);
                }
            }
            else
            {
                var sameCategoryEquipped = model.wearables.FirstOrDefault(x => x.category == wearable.category);
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
        if (bodyShape.category != Categories.BODY_SHAPE)
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
        if (!wearablesByCategory.ContainsKey(wearable.category))
            return;

        if (wearablesByCategory[wearable.category].Contains(wearable) && wearable.SupportsBodyShape(model.bodyShape.id) && !model.wearables.Contains(wearable))
        {
            var toReplace = GetWearablesReplacedBy(wearable);
            toReplace.ForEach(UnequipWearable);
            model.wearables.Add(wearable);
            view.SelectWearable(wearable);
        }
    }

    private void UnequipWearable(WearableItem wearable)
    {
        if (model.wearables.Contains(wearable))
        {
            model.wearables.Remove(wearable);
            view.UnselectWearable(wearable);
        }
    }

    public void UnequipAllWearables()
    {
        foreach (var wearable in model.wearables)
        {
            view.UnselectWearable(wearable);
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
        if (!wearable.tags.Contains("base-wearable") && userProfile.GetItemAmount(id) == 0)
        {
            return;
        }

        if (!wearablesByCategory.ContainsKey(wearable.category))
        {
            wearablesByCategory.Add(wearable.category, new List<WearableItem>());
        }

        wearablesByCategory[wearable.category].Add(wearable);
        view.AddWearable(wearable, userProfile.GetItemAmount(id));
    }

    private void RemoveWearable(string id, WearableItem wearable)
    {
        if (wearablesByCategory.ContainsKey(wearable.category))
        {
            if (wearablesByCategory[wearable.category].Remove(wearable))
            {
                if (wearablesByCategory[wearable.category].Count == 0)
                {
                    wearablesByCategory.Remove(wearable.category);
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

            if (categoriesToReplace.Contains(wearable.category))
            {
                wearablesToReplace.Add(wearable);
            }
            else
            {
                //For retrocompatibility's sake we check current wearables against new one (compatibility matrix is symmetrical)
                HashSet<string> replacesList = new HashSet<string>(wearable.GetReplacesList(model.bodyShape.id) ?? new string[0]);
                if (replacesList.Contains(wearableItem.category))
                {
                    wearablesToReplace.Add(wearable);
                }
            }
        }

        return wearablesToReplace;
    }

    private float prevRenderScale = 1.0f;
    private Camera mainCamera;

    public void SetVisibility(bool visible)
    {
        var currentRenderProfile = DCL.RenderProfileManifest.i.currentProfile;

        if (!visible && view.isOpen)
        {
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
            OnClose?.Invoke();
        }
        else if (visible && !view.isOpen)
        {
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
            OnOpen?.Invoke();
        }

        currentRenderProfile.avatarProfile.Apply();
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        view.OnToggleActionTriggered -= ToggleVisibility;
        view.OnCloseActionTriggered -= DiscardAndClose;

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

        SetVisibility(false);
        DataStore.i.isSignUpFlow.Set(false);
    }

    public void DiscardAndClose()
    {
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

    public void SellCollectible(string collectibleId) { WebInterface.OpenURL(URL_SELL_COLLECTIBLE); }

    public void ToggleVisibility() { SetVisibility(!view.isOpen); }
}