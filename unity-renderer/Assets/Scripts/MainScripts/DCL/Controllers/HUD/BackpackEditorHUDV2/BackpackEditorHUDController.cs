using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController
    {
        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;
        private readonly RendererState rendererState;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly BackpackEmotesSectionController backpackEmotesSectionController;
        private readonly BackpackAnalyticsController backpackAnalyticsController;
        private readonly BackpackEditorHUDModel model = new ();
        private bool avatarIsDirty;

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            RendererState rendererState,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            BackpackEmotesSectionController backpackEmotesSectionController,
            BackpackAnalyticsController backpackAnalyticsController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.rendererState = rendererState;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.backpackEmotesSectionController = backpackEmotesSectionController;
            this.backpackAnalyticsController = backpackAnalyticsController;

            ownUserProfile.OnUpdate += LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange += OnBackpackVisibleChanged;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;
            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);
            backpackEmotesSectionController.OnNewEmoteAdded += OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed += OnEmotePreviewed;
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get(), false);
        }

        public void Dispose()
        {
            ownUserProfile.OnUpdate -= LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange -= OnBackpackVisibleChanged;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;

            backpackEmotesSectionController.OnNewEmoteAdded -= OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed -= OnEmotePreviewed;
            backpackEmotesSectionController.Dispose();

            view.Dispose();
        }

        private void OnBackpackVisibleChanged(bool current, bool _) =>
            SetVisibility(current);

        private void SetVisibility(bool visible, bool saveAvatar = true)
        {
            if (visible)
            {
                avatarIsDirty = false;
                backpackEmotesSectionController.RestoreEmoteSlots();
                backpackEmotesSectionController.LoadEmotes();
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

            EquipBodyShape(bodyShape);
            EquipSkinColor(userProfile.avatar.skinColor);
            EquipHairColor(userProfile.avatar.hairColor);
            EquipEyesColor(userProfile.avatar.eyeColor);

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

                model.wearables.Add(wearable);
            }
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

            if (model.bodyShape == bodyShape)
                return;

            model.bodyShape = bodyShape;
            backpackEmotesSectionController.SetEquippedBodyShape(bodyShape.id);
        }

        private void EquipSkinColor(Color color) =>
            model.skinColor = color;

        private void EquipHairColor(Color color) =>
            model.hairColor = color;

        private void EquipEyesColor(Color color) =>
            model.eyesColor = color;

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

            backpackAnalyticsController.SendNewEquippedWearablesAnalytics(ownUserProfile.avatar.wearables, avatarModel.wearables);
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
    }
}
