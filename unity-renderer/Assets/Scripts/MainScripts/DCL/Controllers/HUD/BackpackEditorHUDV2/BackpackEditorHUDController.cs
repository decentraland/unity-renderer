using DCLServices.WearablesCatalogService;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController : IHUD
    {
        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly BackpackEmotesSectionController backpackEmotesSectionController;
        private readonly BackpackEditorHUDModel model = new ();
        private bool avatarIsDirty;
        private float prevRenderScale = 1.0f;

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            BackpackEmotesSectionController backpackEmotesSectionController)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.backpackEmotesSectionController = backpackEmotesSectionController;

            ownUserProfile.OnUpdate += LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange += SetVisibility;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;
            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);
            dataStore.exploreV2.isOpen.OnChange += ExploreV2IsOpenChanged;
            backpackEmotesSectionController.OnNewEmoteAdded += OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed += OnEmotePreviewed;
            backpackEmotesSectionController.OnEmoteEquipped += OnEmoteEquipped;
            backpackEmotesSectionController.OnEmoteUnequipped += OnEmoteUnequipped;
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get());
        }

        public void Dispose()
        {
            ownUserProfile.OnUpdate -= LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange -= SetVisibility;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            dataStore.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;

            backpackEmotesSectionController.OnNewEmoteAdded -= OnNewEmoteAdded;
            backpackEmotesSectionController.OnEmotePreviewed -= OnEmotePreviewed;
            backpackEmotesSectionController.OnEmoteEquipped -= OnEmoteEquipped;
            backpackEmotesSectionController.OnEmoteUnequipped -= OnEmoteUnequipped;
            backpackEmotesSectionController.Dispose();

            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                backpackEmotesSectionController.LoadEmotes();
                LoadUserProfile(ownUserProfile, true);
                view.Show();
            }
            else
            {
                view.Hide();
                view.ResetPreviewEmote();
            }

            SetRenderScale(visible);
        }

        private void SetVisibility(bool current, bool _) =>
            SetVisibility(current);

        private void ConfigureBackpackInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void ExploreV2IsOpenChanged(bool current, bool previous)
        {
            if (current || !avatarIsDirty)
                return;

            avatarIsDirty = false;
            LoadUserProfile(ownUserProfile, true);
            backpackEmotesSectionController.RestoreEmoteSlots();
        }

        private void LoadUserProfile(UserProfile userProfile) =>
            LoadUserProfile(userProfile, false);

        private void LoadUserProfile(UserProfile userProfile, bool forceLoading)
        {
            bool avatarEditorNotVisible = CommonScriptableObjects.rendererState.Get() && !view.isVisible;
            bool isPlaying = !Application.isBatchMode;

            if (!forceLoading && isPlaying && avatarEditorNotVisible) return;
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

            if (!DataStore.i.common.isPlayerRendererLoaded.Get()) return;

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

        private void OnEmoteEquipped(string emoteId)
        {
            wearablesCatalogService.WearablesCatalog.TryGetValue(emoteId, out WearableItem equippedEmote);

            if (equippedEmote != null && equippedEmote.IsEmote())
                avatarIsDirty = true;
        }

        private void OnEmoteUnequipped(string emoteId)
        {
            wearablesCatalogService.WearablesCatalog.TryGetValue(emoteId, out WearableItem unequippedEmote);

            if (unequippedEmote != null && unequippedEmote.IsEmote())
                avatarIsDirty = true;
        }

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

        private void SetRenderScale(bool visible)
        {
            // NOTE(Brian): SSAO doesn't work correctly with the offset avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (asset == null)
                return;

            if (visible)
            {
                prevRenderScale = asset.renderScale;
                asset.renderScale = 1.0f;
            }
            else
                asset.renderScale = prevRenderScale;
        }
    }
}
