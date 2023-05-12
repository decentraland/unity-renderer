using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController
    {
        private const string EYES_CATEGORY = "eyes";
        private const string HAIR_CATEGORY = "hair";
        private const string EYEBROWS_CATEGORY = "eyebrows";
        private const string FACIAL_HAIR_CATEGORY = "facial_hair";
        private const string BODYSHAPE_CATEGORY = "body_shape";

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

        private BaseCollection<string> previewEquippedWearables => dataStore.backpackV2.previewEquippedWearables;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private readonly BackpackEditorHUDModel model = new ();
        private readonly BackpackEditorHUDModel preEquipModel = new ();

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
            ownUserProfile.OnUpdate += LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange += OnBackpackVisibleChanged;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;

            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);

            backpackEmotesSectionController.OnNewEmoteAdded += OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed += OnEmotePreviewed;

            wearableGridController.OnWearableSelected += SelectWearable;
            wearableGridController.OnWearableEquipped += EquipWearable;
            wearableGridController.OnWearableUnequipped += UnEquipWearable;

            avatarSlotsHUDController.OnToggleSlot += ToggleSlot;
            avatarSlotsHUDController.OnUnequipFromSlot += UnEquipWearable;

            view.SetColorPickerVisibility(false);
            view.OnColorChanged += OnWearableColorChanged;

            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get(), saveAvatar: false);
        }

        public void Dispose()
        {
            ownUserProfile.OnUpdate -= LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange -= OnBackpackVisibleChanged;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;

            backpackEmotesSectionController.OnNewEmoteAdded -= OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed -= OnEmotePreviewed;
            backpackEmotesSectionController.Dispose();

            wearableGridController.OnWearableSelected -= SelectWearable;
            wearableGridController.OnWearableEquipped -= EquipWearable;
            wearableGridController.OnWearableUnequipped -= UnEquipWearable;
            wearableGridController.Dispose();

            avatarSlotsHUDController.OnToggleSlot -= ToggleSlot;
            avatarSlotsHUDController.Dispose();

            view.OnColorChanged -= OnWearableColorChanged;
            view.Dispose();
        }

        private void OnBackpackVisibleChanged(bool current, bool _) =>
            SetVisibility(current, saveAvatar: avatarIsDirty);

        private void SetVisibility(bool visible, bool saveAvatar = true)
        {
            if (visible)
            {
                avatarIsDirty = false;
                backpackEmotesSectionController.RestoreEmoteSlots();
                backpackEmotesSectionController.LoadEmotes();
                wearableGridController.LoadWearables();
                wearableGridController.LoadCollections();
                LoadUserProfile(ownUserProfile, true);

                view.Show();
            }
            else
            {
                if (saveAvatar)
                    TakeSnapshots(
                        onSuccess: (face256Snapshot, bodySnapshot) =>
                        {
                            SaveAvatar(face256Snapshot, bodySnapshot);
                            CloseView();
                        },
                        onFailed: () =>
                        {
                            Debug.LogError("Error taking avatar screenshots.");
                            CloseView();
                        });
                else
                    CloseView();

                wearableGridController.CancelWearableLoading();
            }
        }

        private void CloseView()
        {
            view.Hide();
            view.ResetPreviewEmote();
        }

        private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void LoadUserProfile(UserProfile userProfile) =>
            LoadUserProfile(userProfile, false);

        private void LoadUserProfile(UserProfile userProfile, bool forceLoading)
        {
            bool avatarEditorNotVisible = rendererState.Get() && !view.isVisible;

            if (!forceLoading && avatarEditorNotVisible) return;
            if (userProfile == null) return;
            if (userProfile.avatar == null || string.IsNullOrEmpty(userProfile.avatar.bodyShape)) return;

            wearablesCatalogService.WearablesCatalog.TryGetValue(userProfile.avatar.bodyShape, out var bodyShape);

            if (bodyShape == null) return;
            if (avatarIsDirty) return;

            previewEquippedWearables.Set(userProfile.avatar.wearables);

            UnEquipCurrentBodyShape();
            EquipBodyShape(bodyShape);

            model.skinColor = userProfile.avatar.skinColor;
            model.hairColor = userProfile.avatar.hairColor;
            model.eyesColor = userProfile.avatar.eyeColor;

            model.wearables.Clear();

            int wearablesCount = userProfile.avatar.wearables.Count;

            if (!dataStore.common.isPlayerRendererLoaded.Get()) return;

            for (var i = 0; i < wearablesCount; i++)
            {
                if (!wearablesCatalogService.WearablesCatalog.TryGetValue(userProfile.avatar.wearables[i], out var wearable))
                {
                    Debug.LogError($"Couldn't find wearable with ID {userProfile.avatar.wearables[i]}");
                    continue;
                }

                model.wearables.Add(wearable.id, wearable);
                avatarSlotsHUDController.Equip(wearable, userProfile.avatar.bodyShape);
                wearableGridController.Equip(wearable.id);
            }

            view.UpdateAvatarPreview(model.ToAvatarModel());
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

        private void EquipBodyShape(WearableItem bodyShape)
        {
            if (bodyShape.data.category != WearableLiterals.Categories.BODY_SHAPE)
            {
                Debug.LogError($"Item ({bodyShape.id} is not a body shape");
                return;
            }

            model.bodyShape = bodyShape;
            avatarSlotsHUDController.Equip(bodyShape, bodyShape.id);
            backpackEmotesSectionController.SetEquippedBodyShape(bodyShape.id);
            wearableGridController.Equip(bodyShape.id);
            previewEquippedWearables.Add(bodyShape.id);

            avatarIsDirty = true;
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

            preEquipModel.Update(model);

            foreach (var w in preEquipModel.wearables.Values)
            {
                if (w.data.category != wearable.data.category)
                    continue;

                preEquipModel.wearables.Remove(w.id);
                break;
            }

            preEquipModel.wearables.Add(wearableId, wearable);

            view.UpdateAvatarPreview(preEquipModel.ToAvatarModel());
        }

        private void EquipWearable(string wearableId, EquipWearableSource source)
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
                avatarSlotsHUDController.Equip(wearable, ownUserProfile.avatar.bodyShape);
                wearableGridController.Equip(wearableId);
            }

            avatarIsDirty = true;
            backpackAnalyticsController.SendEquipWearableAnalytic(wearable.data.category, wearable.rarity, source);
            view.UpdateAvatarPreview(model.ToAvatarModel());
        }

        private void UnEquipCurrentBodyShape()
        {
            if (model.bodyShape.id == "NOT_LOADED") return;

            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(model.bodyShape.id, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip body shape {model.bodyShape.id}");
                return;
            }

            UnEquipWearable(wearable, UnequipWearableSource.None);
        }

        private void UnEquipWearable(string wearableId, UnequipWearableSource source)
        {
            if (!wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearable))
            {
                Debug.LogError($"Cannot unequip wearable {wearableId}");
                return;
            }

            UnEquipWearable(wearable, source);
        }

        private void UnEquipWearable(WearableItem wearable, UnequipWearableSource source)
        {
            string wearableId = wearable.id;

            if(source != UnequipWearableSource.None)
                backpackAnalyticsController.SendUnequippedWearableAnalytic(wearable.data.category, wearable.rarity, source);

            avatarSlotsHUDController.UnEquip(wearable.data.category);
            model.wearables.Remove(wearableId);
            previewEquippedWearables.Remove(wearableId);
            wearableGridController.UnEquip(wearableId);

            avatarIsDirty = true;

            view.UpdateAvatarPreview(model.ToAvatarModel());
        }

        private void ToggleSlot(string slotCategory, bool supportColor, bool isSelected)
        {
            currentSlotSelected = isSelected ? slotCategory : null;
            view.SetColorPickerVisibility(isSelected && supportColor);
            if (isSelected && supportColor)
                view.SetColorPickerAsSkinMode(slotCategory == BODYSHAPE_CATEGORY);

            switch (slotCategory)
            {
                case EYES_CATEGORY:
                    view.SetColorPickerValue(model.eyesColor);
                    break;
                case HAIR_CATEGORY or EYEBROWS_CATEGORY or FACIAL_HAIR_CATEGORY:
                    view.SetColorPickerValue(model.hairColor);
                    break;
                case BODYSHAPE_CATEGORY:
                    view.SetColorPickerValue(model.skinColor);
                    break;
            }
        }

        private void OnWearableColorChanged(Color newColor)
        {
            var colorChanged = false;

            switch (currentSlotSelected)
            {
                case EYES_CATEGORY:
                    model.eyesColor = newColor;
                    colorChanged = true;
                    break;
                case HAIR_CATEGORY or EYEBROWS_CATEGORY or FACIAL_HAIR_CATEGORY:
                    model.hairColor = newColor;
                    colorChanged = true;
                    break;
                case BODYSHAPE_CATEGORY:
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
