using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
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
        private readonly BackpackAnalyticsController backpackAnalyticsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly RendererState rendererState;
        private readonly WearableGridController wearableGridController;
        private readonly AvatarSlotsHUDController avatarSlotsHUDController;
        private string currentSlotSelected;
        private bool avatarIsDirty;
        private CancellationTokenSource loadProfileCancellationToken = new ();
        private CancellationTokenSource setVisibilityCancellationToken = new ();

        private BaseCollection<string> previewEquippedWearables => dataStore.backpackV2.previewEquippedWearables;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private readonly BackpackEditorHUDModel model = new ();

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            RendererState rendererState,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            IBackpackEmotesSectionController backpackEmotesSectionController,
            BackpackAnalyticsController backpackAnalyticsController,
            WearableGridController wearableGridController,
            AvatarSlotsHUDController avatarSlotsHUDController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.rendererState = rendererState;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.backpackEmotesSectionController = backpackEmotesSectionController;
            this.backpackAnalyticsController = backpackAnalyticsController;
            this.wearableGridController = wearableGridController;
            this.avatarSlotsHUDController = avatarSlotsHUDController;

            avatarSlotsHUDController.GenerateSlots();
            ownUserProfile.OnUpdate += LoadUserProfileFromProfileUpdate;
            dataStore.HUDs.avatarEditorVisible.OnChange += OnBackpackVisibleChanged;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;

            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);

            backpackEmotesSectionController.OnNewEmoteAdded += OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed += OnEmotePreviewed;

            wearableGridController.OnWearableSelected += SelectWearable;
            wearableGridController.OnWearableEquipped += EquipWearableFromGrid;
            wearableGridController.OnWearableUnequipped += UnEquipWearableFromGrid;

            avatarSlotsHUDController.OnToggleSlot += ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot += UnEquipWearableFromSlot;
            avatarSlotsHUDController.OnHideUnhidePressed += UpdateOverrideHides;

            view.SetColorPickerVisibility(false);
            view.OnContinueSignup += SaveAvatarAndContinueSignupProcess;
            view.OnColorChanged += OnWearableColorChanged;

            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get(), saveAvatar: false);
        }

        public void Dispose()
        {
            ownUserProfile.OnUpdate -= LoadUserProfileFromProfileUpdate;
            dataStore.HUDs.avatarEditorVisible.OnChange -= OnBackpackVisibleChanged;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;

            backpackEmotesSectionController.OnNewEmoteAdded -= OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed -= OnEmotePreviewed;
            backpackEmotesSectionController.Dispose();

            wearableGridController.OnWearableSelected -= SelectWearable;
            wearableGridController.OnWearableEquipped -= EquipWearableFromGrid;
            wearableGridController.OnWearableUnequipped -= UnEquipWearableFromGrid;
            wearableGridController.Dispose();

            avatarSlotsHUDController.OnToggleSlot -= ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot -= UnEquipWearableFromSlot;
            avatarSlotsHUDController.OnHideUnhidePressed -= UpdateOverrideHides;
            avatarSlotsHUDController.Dispose();

            view.OnColorChanged -= OnWearableColorChanged;
            view.OnContinueSignup -= SaveAvatarAndContinueSignupProcess;
            view.Dispose();
        }

        private void UpdateOverrideHides(string category, bool toggleOn)
        {
            if (toggleOn)
                model.hidingOverrideMap.Add(category);
            else
                model.hidingOverrideMap.Remove(category);

            avatarIsDirty = true;
            avatarSlotsHUDController.Recalculate(model.hidingOverrideMap);
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
                    backpackEmotesSectionController.RestoreEmoteSlots();
                    backpackEmotesSectionController.LoadEmotes();
                    wearableGridController.LoadWearables();
                    wearableGridController.LoadCollections();
                    LoadUserProfile(ownUserProfile);
                    view.Show();

                    if (dataStore.common.isSignUpFlow.Get())
                        view.ShowContinueSignup();
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
            view.ResetPreviewEmote();
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

                    model.wearables.Clear();
                    previewEquippedWearables.Clear();

                    int wearablesCount = userProfile.avatar.wearables.Count;

                    for (var i = 0; i < wearablesCount; i++)
                        EquipWearable(userProfile.avatar.wearables[i], EquipWearableSource.None, false, false);

                    view.UpdateAvatarPreview(model.ToAvatarModel());
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogError(e); }
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

            view.UpdateAvatarPreview(modelToUpdate);
        }

        private void OnNewEmoteAdded(string emoteId) =>
            UpdateAvatarPreview();

        private void OnEmotePreviewed(string emoteId) =>
            view.PlayPreviewEmote(emoteId);

        private void EquipBodyShape(WearableItem bodyShape, bool setAsDirty = true)
        {
            if (bodyShape.data.category != WearableLiterals.Categories.BODY_SHAPE)
            {
                Debug.LogError($"Item ({bodyShape.id} is not a body shape");
                return;
            }

            model.bodyShape = bodyShape;
            dataStore.backpackV2.previewBodyShape.Set(bodyShape.id);
            avatarSlotsHUDController.Equip(bodyShape, bodyShape.id, model.hidingOverrideMap);
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
                backpackAnalyticsController.SendAvatarEditSuccessNuxAnalytic();
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

        private void EquipWearableFromGrid(string wearableId, EquipWearableSource source) =>
            EquipWearable(wearableId, source);

        private void UnEquipWearableFromGrid(string wearableId, UnequipWearableSource source) =>
            UnEquipWearable(wearableId, source);

        private void UnEquipWearableFromSlot(string wearableId, UnequipWearableSource source) =>
            UnEquipWearable(wearableId, source);

        private void EquipWearable(string wearableId,
            EquipWearableSource source = EquipWearableSource.None,
            bool setAsDirty = true,
            bool updateAvatarPreview = true)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot equip wearable {wearableId}");
                return;
            }

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

                if(wearable.GetHidesList(ownUserProfile.avatar.bodyShape) != null)
                    foreach (string s in wearable.GetHidesList(ownUserProfile.avatar.bodyShape))
                        UpdateOverrideHides(s,false);

                avatarSlotsHUDController.Equip(wearable, ownUserProfile.avatar.bodyShape, model.hidingOverrideMap);
                wearableGridController.Equip(wearableId);
            }

            if (setAsDirty)
                avatarIsDirty = true;

            if (source != EquipWearableSource.None)
                backpackAnalyticsController.SendEquipWearableAnalytic(wearable.data.category, wearable.rarity, source);

            if (updateAvatarPreview)
                view.UpdateAvatarPreview(model.ToAvatarModel());
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

        private void UnEquipWearable(string wearableId, UnequipWearableSource source, bool setAsDirty = true)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip wearable {wearableId}");
                return;
            }

            UnEquipWearable(wearable, source, setAsDirty);
        }

        private void UnEquipWearable(WearableItem wearable,
            UnequipWearableSource source = UnequipWearableSource.None,
            bool setAsDirty = true)
        {
            string wearableId = wearable.id;

            if (source != UnequipWearableSource.None)
                backpackAnalyticsController.SendUnequippedWearableAnalytic(wearable.data.category, wearable.rarity, source);

            if(wearable.GetHidesList(ownUserProfile.avatar.bodyShape) != null)
                foreach (string s in wearable.GetHidesList(ownUserProfile.avatar.bodyShape))
                    UpdateOverrideHides(s,false);

            avatarSlotsHUDController.UnEquip(wearable.data.category, model.hidingOverrideMap);
            model.wearables.Remove(wearableId);
            previewEquippedWearables.Remove(wearableId);
            wearableGridController.UnEquip(wearableId);

            if (setAsDirty)
                avatarIsDirty = true;

            view.UpdateAvatarPreview(model.ToAvatarModel());
        }

        private void ToggleSlot(string slotCategory, bool supportColor, bool isSelected)
        {
            currentSlotSelected = isSelected ? slotCategory : null;
            view.UpdateHideUnhideStatus(currentSlotSelected, model.hidingOverrideMap);
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
            view.UpdateAvatarPreview(model.ToAvatarModel());
        }
    }
}
