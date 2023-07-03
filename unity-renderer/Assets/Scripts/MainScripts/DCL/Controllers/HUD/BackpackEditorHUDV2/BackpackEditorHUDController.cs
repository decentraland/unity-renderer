using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController
    {
        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly IBackpackEmotesSectionController backpackEmotesSectionController;
        private readonly IBackpackAnalyticsService backpackAnalyticsService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly RendererState rendererState;
        private readonly WearableGridController wearableGridController;
        private readonly AvatarSlotsHUDController avatarSlotsHUDController;
        private readonly OutfitsController outfitsController;
        private string currentSlotSelected;
        private bool avatarIsDirty;
        private CancellationTokenSource loadProfileCancellationToken = new ();
        private CancellationTokenSource setVisibilityCancellationToken = new ();
        private CancellationTokenSource outfitLoadCancellationToken = new ();
        private string categoryPendingToPlayEmote;

        private BaseCollection<string> previewEquippedWearables => dataStore.backpackV2.previewEquippedWearables;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private readonly BackpackEditorHUDModel model = new ();

        private int currentAnimationIndexShown;
        private bool shouldRequestOutfits = true;

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            RendererState rendererState,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            IBackpackEmotesSectionController backpackEmotesSectionController,
            IBackpackAnalyticsService backpackAnalyticsService,
            WearableGridController wearableGridController,
            AvatarSlotsHUDController avatarSlotsHUDController,
            OutfitsController outfitsController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.rendererState = rendererState;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.backpackEmotesSectionController = backpackEmotesSectionController;
            this.backpackAnalyticsService = backpackAnalyticsService;
            this.wearableGridController = wearableGridController;
            this.avatarSlotsHUDController = avatarSlotsHUDController;
            this.outfitsController = outfitsController;

            avatarSlotsHUDController.GenerateSlots();
            ownUserProfile.OnUpdate += LoadUserProfileFromProfileUpdate;
            dataStore.HUDs.avatarEditorVisible.OnChange += OnBackpackVisibleChanged;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;

            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);

            backpackEmotesSectionController.OnNewEmoteAdded += OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed += OnEmotePreviewed;
            backpackEmotesSectionController.OnEmoteEquipped += OnEmoteEquipped;
            backpackEmotesSectionController.OnEmoteUnEquipped += OnEmoteUnEquipped;

            wearableGridController.OnWearableSelected += SelectWearable;
            wearableGridController.OnWearableEquipped += EquipWearableFromGrid;
            wearableGridController.OnWearableUnequipped += UnEquipWearableFromGrid;
            wearableGridController.OnCategoryFilterRemoved += OnCategoryFilterRemoved;

            avatarSlotsHUDController.OnToggleSlot += ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot += UnEquipWearableFromSlot;
            avatarSlotsHUDController.OnHideUnhidePressed += UpdateOverrideHides;

            view.SetColorPickerVisibility(false);
            view.OnContinueSignup += SaveAvatarAndContinueSignupProcess;
            view.OnColorChanged += OnWearableColorChanged;
            view.OnColorPickerToggle += OnColorPickerToggled;
            view.OnAvatarUpdated += OnAvatarUpdated;
            view.OnOutfitsOpened += OnOutfitsOpened;
            outfitsController.OnOutfitEquipped += OnOutfitEquipped;

            view.SetOutfitsEnabled(dataStore.featureFlags.flags.Get().IsFeatureEnabled("outfits"));
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get(), saveAvatar: false);
        }

        private void OnOutfitEquipped(OutfitItem outfit)
        {
            Dictionary<string, WearableItem> keyValuePairs = new Dictionary<string, WearableItem>(model.wearables);
            foreach (KeyValuePair<string, WearableItem> keyValuePair in keyValuePairs)
                UnEquipWearable(keyValuePair.Key, UnequipWearableSource.None, false, false);

            foreach (string forcedCategory in model.forceRender)
                UpdateOverrideHides(forcedCategory, false, false);

            outfitLoadCancellationToken = new CancellationTokenSource();
            LoadAndEquipOutfitWearables(outfit, outfitLoadCancellationToken.Token).Forget();
        }

        private async UniTaskVoid LoadAndEquipOutfitWearables(OutfitItem outfit, CancellationToken cancellationToken)
        {
            if (!wearablesCatalogService.WearablesCatalog.ContainsKey(outfit.outfit.bodyShape))
                await wearablesCatalogService.RequestWearableAsync(outfit.outfit.bodyShape, cancellationToken);
            foreach (string outfitWearable in outfit.outfit.wearables)
            {
                if (wearablesCatalogService.WearablesCatalog.ContainsKey(outfitWearable)) continue;

                try { await wearablesCatalogService.RequestWearableAsync(outfitWearable, cancellationToken); }
                catch (Exception e) { Debug.LogWarning($"Cannot resolve the wearable {outfitWearable} for the outfit {outfit.slot}"); }
            }

            EquipWearable(outfit.outfit.bodyShape, setAsDirty: false, updateAvatarPreview: false);

            foreach (string outfitWearable in outfit.outfit.wearables)
                EquipWearable(outfitWearable, setAsDirty: true, updateAvatarPreview: true);

            SetAllColors(outfit.outfit.eyes.color, outfit.outfit.hair.color, outfit.outfit.skin.color);

            foreach (string forcedCategory in outfit.outfit.forceRender)
                UpdateOverrideHides(forcedCategory, true, true);
        }

        private void OnOutfitsOpened()
        {
            if (!shouldRequestOutfits) return;

            outfitsController.RequestOwnedOutfits();
            shouldRequestOutfits = false;
        }

        public void Dispose()
        {
            ownUserProfile.OnUpdate -= LoadUserProfileFromProfileUpdate;
            dataStore.HUDs.avatarEditorVisible.OnChange -= OnBackpackVisibleChanged;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;

            backpackEmotesSectionController.OnNewEmoteAdded -= OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed -= OnEmotePreviewed;
            backpackEmotesSectionController.OnEmoteEquipped -= OnEmoteEquipped;
            backpackEmotesSectionController.OnEmoteUnEquipped -= OnEmoteUnEquipped;
            backpackEmotesSectionController.Dispose();

            wearableGridController.OnWearableSelected -= SelectWearable;
            wearableGridController.OnWearableEquipped -= EquipWearableFromGrid;
            wearableGridController.OnWearableUnequipped -= UnEquipWearableFromGrid;
            wearableGridController.OnCategoryFilterRemoved -= OnCategoryFilterRemoved;
            wearableGridController.Dispose();

            avatarSlotsHUDController.OnToggleSlot -= ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot -= UnEquipWearableFromSlot;
            avatarSlotsHUDController.OnHideUnhidePressed -= UpdateOverrideHides;
            avatarSlotsHUDController.Dispose();

            view.OnColorChanged -= OnWearableColorChanged;
            view.OnContinueSignup -= SaveAvatarAndContinueSignupProcess;
            view.Dispose();
        }

        private void UpdateOverrideHides(string category, bool toggleOn) =>
            UpdateOverrideHides(category, toggleOn, true);

        private void UpdateOverrideHides(string category, bool toggleOn, bool setAsDirty)
        {
            if (toggleOn)
                model.forceRender.Add(category);
            else
                model.forceRender.Remove(category);

            avatarIsDirty = setAsDirty;
            avatarSlotsHUDController.Recalculate(model.forceRender);
            UpdateAvatarPreview();
        }

        private void OnBackpackVisibleChanged(bool current, bool _) =>
            SetVisibility(current, saveAvatar: avatarIsDirty);

        private void SetVisibility(bool visible, bool saveAvatar = true)
        {
            async UniTaskVoid SetVisibilityAsync(bool visible, bool saveAvatar, CancellationToken cancellationToken)
            {
                if (visible)
                {
                    avatarIsDirty = false;
                    dataStore.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InEditor);
                    backpackEmotesSectionController.RestoreEmoteSlots();
                    backpackEmotesSectionController.LoadEmotes();
                    wearableGridController.LoadWearables();
                    wearableGridController.LoadCollections();
                    LoadUserProfile(ownUserProfile);
                    view.Show();

                    if (dataStore.common.isSignUpFlow.Get())
                    {
                        view.ShowContinueSignup();
                        avatarSlotsHUDController.SelectSlot(WearableLiterals.Categories.BODY_SHAPE);
                    }
                    else
                        view.HideContinueSignup();
                }
                else
                {
                    if (saveAvatar)
                    {
                        try
                        {
                            await TakeSnapshotsAndSaveAvatarAsync(cancellationToken);
                            CloseView();
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            CloseView();
                        }
                    }
                    else
                        CloseView();

                    wearableGridController.CancelWearableLoading();
                }
            }

            setVisibilityCancellationToken = setVisibilityCancellationToken.SafeRestart();
            SetVisibilityAsync(visible, saveAvatar, setVisibilityCancellationToken.Token).Forget();
        }

        private void CloseView()
        {
            view.Hide();
            view.ResetPreviewPanel();
            wearableGridController.ResetFilters();
            dataStore.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InWorld);
        }

        private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void LoadUserProfileFromProfileUpdate(UserProfile userProfile)
        {
            bool isEditorVisible = rendererState.Get() && view.isVisible;
            if (!isEditorVisible) return;
            LoadUserProfile(userProfile);
        }

        private void LoadUserProfile(UserProfile userProfile)
        {
            if (avatarIsDirty) return;
            if (userProfile == null) return;

            if (userProfile.avatar == null || string.IsNullOrEmpty(userProfile.avatar.bodyShape))
            {
                Debug.LogWarning("Cannot update the avatar body shape is invalid");
                return;
            }

            async UniTaskVoid LoadUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken)
            {
                try
                {
                    wearablesCatalogService.WearablesCatalog.TryGetValue(userProfile.avatar.bodyShape, out var bodyShape);
                    bodyShape ??= await wearablesCatalogService.RequestWearableAsync(userProfile.avatar.bodyShape, cancellationToken);

                    UnEquipCurrentBodyShape(false);
                    EquipBodyShape(bodyShape, false);

                    model.skinColor = userProfile.avatar.skinColor;
                    model.hairColor = userProfile.avatar.hairColor;
                    model.eyesColor = userProfile.avatar.eyeColor;
                    model.forceRender = new HashSet<string>(userProfile.avatar.forceRender);
                    model.wearables.Clear();
                    previewEquippedWearables.Clear();

                    int wearablesCount = userProfile.avatar.wearables.Count;

                    for (var i = 0; i < wearablesCount; i++)
                    {
                        string wearableId = userProfile.avatar.wearables[i];

                        if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
                        {
                            try { wearable = await wearablesCatalogService.RequestWearableAsync(wearableId, cancellationToken); }
                            catch (OperationCanceledException) { throw; }
                            catch (Exception e)
                            {
                                Debug.LogError($"Cannot load the wearable {wearableId}");
                                Debug.LogException(e);
                                continue;
                            }
                        }

                        try { EquipWearable(wearable, EquipWearableSource.None, false, false, false); }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception e)
                        {
                            Debug.LogError($"Cannot equip the wearable {wearableId}");
                            Debug.LogException(e);
                        }
                    }

                    avatarSlotsHUDController.Recalculate(model.forceRender);
                    UpdateAvatarModel(model.ToAvatarModel());
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            loadProfileCancellationToken = loadProfileCancellationToken.SafeRestart();
            LoadUserProfileAsync(userProfile, loadProfileCancellationToken.Token).Forget();
        }

        private void UpdateAvatarPreview()
        {
            AvatarModel modelToUpdate = model.ToAvatarModel();

            // We always keep the loaded emotes into the Avatar Preview
            foreach (string emoteId in dataStore.emotesCustomization.currentLoadedEmotes.Get())
                modelToUpdate.emotes.Add(new AvatarModel.AvatarEmoteEntry() { urn = emoteId });

            UpdateAvatarModel(modelToUpdate);
        }

        private void OnNewEmoteAdded(string emoteId) =>
            UpdateAvatarPreview();

        private void OnEmotePreviewed(string emoteId) =>
            view.PlayPreviewEmote(emoteId);

        private void OnEmoteEquipped(string emoteId) =>
            avatarIsDirty = true;

        private void OnEmoteUnEquipped(string emoteId) =>
            avatarIsDirty = true;

        private void EquipBodyShape(WearableItem bodyShape, bool setAsDirty = true)
        {
            if (bodyShape.data.category != WearableLiterals.Categories.BODY_SHAPE)
            {
                Debug.LogError($"Item ({bodyShape.id} is not a body shape");
                return;
            }

            model.bodyShape = bodyShape;
            dataStore.backpackV2.previewBodyShape.Set(bodyShape.id);
            avatarSlotsHUDController.Equip(bodyShape, bodyShape.id, model.forceRender);
            backpackEmotesSectionController.SetEquippedBodyShape(bodyShape.id);
            wearableGridController.Equip(bodyShape.id);
            wearableGridController.UpdateBodyShapeCompatibility(bodyShape.id);

            if (setAsDirty)
                avatarIsDirty = true;
        }

        private void SaveAvatarAndContinueSignupProcess() =>
            SetVisibility(false, saveAvatar: true);

        private UniTask TakeSnapshotsAndSaveAvatarAsync(CancellationToken cancellationToken)
        {
            UniTaskCompletionSource task = new ();

            TakeSnapshots(
                onSuccess: (face256Snapshot, bodySnapshot) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SaveAvatar(face256Snapshot, bodySnapshot);
                    task.TrySetResult();
                },
                onFailed: () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    task.TrySetException(new Exception("Error taking avatar screenshots."));
                });

            return task.Task;
        }

        private void TakeSnapshots(IBackpackEditorHUDView.OnSnapshotsReady onSuccess, Action onFailed)
        {
            view.TakeSnapshotsAfterStopPreviewAnimation(
                onSuccess: (face256Snapshot, bodySnapshot) => onSuccess?.Invoke(face256Snapshot, bodySnapshot),
                onFailed: () => onFailed?.Invoke());
        }

        private void SaveAvatar(Texture2D face256Snapshot, Texture2D bodySnapshot)
        {
            var avatarModel = model.ToAvatarModel();

            // Add the equipped emotes to the avatar model
            List<AvatarModel.AvatarEmoteEntry> emoteEntries = new List<AvatarModel.AvatarEmoteEntry>();
            int equippedEmotesCount = dataStore.emotesCustomization.unsavedEquippedEmotes.Count();

            for (var i = 0; i < equippedEmotesCount; i++)
            {
                var equippedEmote = dataStore.emotesCustomization.unsavedEquippedEmotes[i];

                if (equippedEmote == null)
                    continue;

                emoteEntries.Add(new AvatarModel.AvatarEmoteEntry { slot = i, urn = equippedEmote.id });
            }

            avatarModel.emotes = emoteEntries;

            dataStore.emotesCustomization.equippedEmotes.Set(dataStore.emotesCustomization.unsavedEquippedEmotes.Get());

            userProfileBridge.SendSaveAvatar(avatarModel, face256Snapshot, bodySnapshot, dataStore.common.isSignUpFlow.Get());
            ownUserProfile.OverrideAvatar(avatarModel, face256Snapshot);

            if (dataStore.common.isSignUpFlow.Get())
            {
                dataStore.HUDs.signupVisible.Set(true);
                backpackAnalyticsService.SendAvatarEditSuccessNuxAnalytic();
            }

            avatarIsDirty = false;
        }

        private void SelectWearable(string wearableId)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot pre-visualize wearable {wearableId}");
                return;
            }
        }

        private void EquipWearableFromGrid(string wearableId, EquipWearableSource source)
        {
            AudioScriptableObjects.equip.Play(true);
            EquipWearable(wearableId, source);
        }

        private void UnEquipWearableFromGrid(string wearableId, UnequipWearableSource source)
        {
            AudioScriptableObjects.unequip.Play(true);
            UnEquipWearable(wearableId, source);
        }

        private void OnCategoryFilterRemoved() =>
            view.SetColorPickerVisibility(false);

        private void UnEquipWearableFromSlot(string wearableId, UnequipWearableSource source)
        {
            AudioScriptableObjects.unequip.Play(true);
            UnEquipWearable(wearableId, source);
        }

        private void EquipWearable(string wearableId,
            EquipWearableSource source = EquipWearableSource.None,
            bool setAsDirty = true,
            bool updateAvatarPreview = true,
            bool resetOverride = true)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot equip wearable {wearableId}");
                return;
            }

            EquipWearable(wearable, source, setAsDirty, updateAvatarPreview, resetOverride);
        }

        private void EquipWearable(WearableItem wearable,
            EquipWearableSource source = EquipWearableSource.None,
            bool setAsDirty = true,
            bool updateAvatarPreview = true,
            bool resetOverride = true)
        {
            string wearableId = wearable.id;

            if (wearable.data.category == WearableLiterals.Categories.BODY_SHAPE)
            {
                UnEquipCurrentBodyShape();
                EquipBodyShape(wearable);
            }
            else
            {
                foreach (var w in model.wearables.Values)
                {
                    if (w.data.category != wearable.data.category)
                        continue;

                    UnEquipWearable(w.id, UnequipWearableSource.None);
                    break;
                }

                model.wearables.Add(wearableId, wearable);
                previewEquippedWearables.Add(wearableId);

                if (resetOverride)
                    ResetOverridesOfAffectedCategories(wearable, setAsDirty);

                avatarSlotsHUDController.Equip(wearable, ownUserProfile.avatar.bodyShape, model.forceRender);
                wearableGridController.Equip(wearableId);
            }

            if (setAsDirty)
                avatarIsDirty = true;

            if (source != EquipWearableSource.None)
                backpackAnalyticsService.SendEquipWearableAnalytic(wearable.data.category, wearable.rarity, source);

            if (updateAvatarPreview)
            {
                UpdateAvatarModel(model.ToAvatarModel());
                categoryPendingToPlayEmote = wearable.data.category;
            }
        }

        private void UnEquipCurrentBodyShape(bool setAsDirty = true)
        {
            if (model.bodyShape.id == "NOT_LOADED") return;

            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(model.bodyShape.id, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip body shape {model.bodyShape.id}");
                return;
            }

            UnEquipWearable(wearable, UnequipWearableSource.None, setAsDirty);
        }

        private void UnEquipWearable(string wearableId, UnequipWearableSource source, bool setAsDirty = true, bool updateAvatarPreview = true)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip wearable {wearableId}");
                return;
            }

            UnEquipWearable(wearable, source, setAsDirty, updateAvatarPreview);
        }

        private void UnEquipWearable(WearableItem wearable,
            UnequipWearableSource source = UnequipWearableSource.None,
            bool setAsDirty = true,
            bool updateAvatarPreview = true)
        {
            string wearableId = wearable.id;

            if (source != UnequipWearableSource.None)
                backpackAnalyticsService.SendUnequippedWearableAnalytic(wearable.data.category, wearable.rarity, source);

            ResetOverridesOfAffectedCategories(wearable, setAsDirty);

            avatarSlotsHUDController.UnEquip(wearable.data.category, model.forceRender);
            model.wearables.Remove(wearableId);
            previewEquippedWearables.Remove(wearableId);
            wearableGridController.UnEquip(wearableId);

            if (setAsDirty)
                avatarIsDirty = true;

            if(updateAvatarPreview)
                UpdateAvatarModel(model.ToAvatarModel());
        }

        private void ResetOverridesOfAffectedCategories(WearableItem wearable, bool setAsDirty = true)
        {
            if (wearable.GetHidesList(ownUserProfile.avatar.bodyShape) != null)
                foreach (string s in wearable.GetHidesList(ownUserProfile.avatar.bodyShape))
                    UpdateOverrideHides(s, false, setAsDirty);
        }

        private void ToggleSlot(string slotCategory, bool supportColor, PreviewCameraFocus previewCameraFocus, bool isSelected)
        {
            currentSlotSelected = isSelected ? slotCategory : null;
            view.UpdateHideUnhideStatus(currentSlotSelected, model.forceRender);
            view.SetColorPickerVisibility(isSelected && supportColor);

            if (isSelected && supportColor)
                view.SetColorPickerAsSkinMode(slotCategory == WearableLiterals.Categories.BODY_SHAPE);

            switch (slotCategory)
            {
                case WearableLiterals.Categories.EYES:
                    view.SetColorPickerValue(model.eyesColor);
                    break;
                case WearableLiterals.Categories.HAIR or WearableLiterals.Categories.EYEBROWS or WearableLiterals.Categories.FACIAL_HAIR:
                    view.SetColorPickerValue(model.hairColor);
                    break;
                case WearableLiterals.Categories.BODY_SHAPE:
                    view.SetColorPickerValue(model.skinColor);
                    break;
            }

            view.SetAvatarPreviewFocus(currentSlotSelected != null ? previewCameraFocus : PreviewCameraFocus.DefaultEditing);
        }

        private void OnWearableColorChanged(Color newColor)
        {
            var colorChanged = false;

            switch (currentSlotSelected)
            {
                case WearableLiterals.Categories.EYES:
                    model.eyesColor = newColor;
                    colorChanged = true;
                    break;
                case WearableLiterals.Categories.HAIR or WearableLiterals.Categories.EYEBROWS or WearableLiterals.Categories.FACIAL_HAIR:
                    model.hairColor = newColor;
                    colorChanged = true;
                    break;
                case WearableLiterals.Categories.BODY_SHAPE:
                    model.skinColor = newColor;
                    colorChanged = true;
                    break;
            }

            if (!colorChanged)
                return;

            avatarIsDirty = true;
            UpdateAvatarModel(model.ToAvatarModel());
        }

        private void SetAllColors(Color eyesColor, Color hairColor, Color bodyColor)
        {
            model.eyesColor = eyesColor;
            model.hairColor = hairColor;
            model.skinColor = bodyColor;

            avatarIsDirty = true;
            UpdateAvatarModel(model.ToAvatarModel());
        }

        private void UpdateAvatarModel(AvatarModel avatarModel)
        {
            view.UpdateAvatarPreview(avatarModel);
            outfitsController.UpdateAvatarPreview(model.ToAvatarModel());
        }

        private void OnColorPickerToggled() =>
            backpackAnalyticsService.SendAvatarColorPick();

        private void OnAvatarUpdated()
        {
            if (string.IsNullOrEmpty(categoryPendingToPlayEmote))
                return;

            PlayEquipAnimation(categoryPendingToPlayEmote);
            categoryPendingToPlayEmote = null;
        }

        private void PlayEquipAnimation(string category)
        {
            view.PlayPreviewEmote(
                GetEquipEmoteByCategory(category),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        private string GetEquipEmoteByCategory(string category)
        {
            string equipEmote = category switch
                                {
                                    WearableLiterals.Categories.FEET => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_SHOES, 2),
                                    WearableLiterals.Categories.LOWER_BODY => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_LOWER, 3),
                                    WearableLiterals.Categories.UPPER_BODY => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_UPPER, 3),
                                    WearableLiterals.Categories.EYEWEAR => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.TIARA => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.EARRING => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.HAT => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.TOP_HEAD => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.HELMET => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.MASK => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_ACCESSORIES, 3),
                                    WearableLiterals.Categories.SKIN => GetRandomizedName(WearableLiterals.DefaultEmotes.OUTFIT_UPPER, 3),
                                    _ => string.Empty,
                                };

            return equipEmote;
        }

        private string GetRandomizedName(string baseString, int limit)
        {
            currentAnimationIndexShown = (currentAnimationIndexShown + 1) % limit;
            return baseString + (currentAnimationIndexShown + 1);
        }
    }
}
