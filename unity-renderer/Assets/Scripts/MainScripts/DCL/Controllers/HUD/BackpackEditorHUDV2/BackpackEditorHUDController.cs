using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.EmotesCustomization;
using DCL.Tasks;
using DCLServices.WearablesCatalogService;
using System;
using System.Linq;
using System.Threading;
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
        private readonly IEmotesCatalogService emotesCatalogService;
        private IEmotesCustomizationComponentController emotesCustomizationComponentController;
        private bool isEmotesControllerInitialized;
        private CancellationTokenSource loadEmotesCTS = new ();
        private readonly BackpackEditorHUDModel model = new ();
        private bool avatarIsDirty;
        private float prevRenderScale = 1.0f;

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IWearablesCatalogService wearablesCatalogService,
            IEmotesCatalogService emotesCatalogService)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.wearablesCatalogService = wearablesCatalogService;
            this.emotesCatalogService = emotesCatalogService;
            ownUserProfile.OnUpdate += LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange += SetVisibility;
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange += ConfigureBackpackInFullscreenMenuChanged;
            dataStore.exploreV2.isOpen.OnChange += ExploreV2IsOpenChanged;
            ConfigureBackpackInFullscreenMenuChanged(dataStore.exploreV2.configureBackpackInFullscreenMenu.Get(), null);
            SetVisibility(dataStore.HUDs.avatarEditorVisible.Get());
        }

        public void Dispose()
        {
            loadEmotesCTS.SafeCancelAndDispose();
            loadEmotesCTS = null;

            ownUserProfile.OnUpdate -= LoadUserProfile;
            dataStore.HUDs.avatarEditorVisible.OnChange -= SetVisibility;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            dataStore.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;
            dataStore.emotesCustomization.currentLoadedEmotes.OnAdded -= OnNewEmoteAdded;
            emotesCustomizationComponentController.onEmotePreviewed -= OnPreviewEmote;
            emotesCustomizationComponentController.onEmoteEquipped -= OnEmoteEquipped;
            emotesCustomizationComponentController.onEmoteUnequipped -= OnEmoteUnequipped;

            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                InitializeEmotesControllerIfNeeded();
                LoadEmotes();
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

        private void InitializeEmotesControllerIfNeeded()
        {
            if (isEmotesControllerInitialized)
                return;

            emotesCustomizationComponentController = new EmotesCustomizationComponentController();
            IEmotesCustomizationComponentView emotesSectionView = emotesCustomizationComponentController.Initialize(
                dataStore.emotesCustomization,
                dataStore.emotes,
                dataStore.exploreV2,
                dataStore.HUDs);

            emotesSectionView.viewTransform.SetParent(view.EmotesSectionTransform, false);
            emotesCustomizationComponentController.SetEquippedBodyShape(ownUserProfile.avatar.bodyShape);
            dataStore.HUDs.isAvatarEditorInitialized.Set(true);
            isEmotesControllerInitialized = true;

            dataStore.emotesCustomization.currentLoadedEmotes.OnAdded += OnNewEmoteAdded;
            emotesCustomizationComponentController.onEmotePreviewed += OnPreviewEmote;
            emotesCustomizationComponentController.onEmoteEquipped += OnEmoteEquipped;
            emotesCustomizationComponentController.onEmoteUnequipped += OnEmoteUnequipped;

            LoadUserProfile(ownUserProfile, true);
        }

        private void LoadEmotes()
        {
            async UniTaskVoid LoadEmotesAsync(CancellationToken ct = default)
            {
                try
                {
                    EmbeddedEmotesSO embeddedEmotesSO = await emotesCatalogService.GetEmbeddedEmotes();
                    var baseEmotes = embeddedEmotesSO.emotes;
                    var ownedEmotes = await emotesCatalogService.RequestOwnedEmotesAsync(ownUserProfile.userId, ct);
                    var allEmotes = ownedEmotes == null ? baseEmotes : baseEmotes.Concat(ownedEmotes);
                    dataStore.emotesCustomization.UnequipMissingEmotes(allEmotes);
                    emotesCustomizationComponentController.SetEmotes(allEmotes.ToArray());

                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading emotes: {e.Message}");
                }
            }

            loadEmotesCTS = loadEmotesCTS.SafeRestart();
            LoadEmotesAsync(loadEmotesCTS.Token).Forget();
        }

        private void ExploreV2IsOpenChanged(bool current, bool previous)
        {
            if (current || !avatarIsDirty)
                return;

            avatarIsDirty = false;
            LoadUserProfile(ownUserProfile, true);
            emotesCustomizationComponentController.RestoreEmoteSlots();
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

        private void OnPreviewEmote(string emoteId) =>
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
            emotesCustomizationComponentController.SetEquippedBodyShape(bodyShape.id);
        }

        private void EquipSkinColor(Color color) =>
            model.skinColor = color;

        private void EquipHairColor(Color color) =>
            model.hairColor = color;

        private void EquipEyesColor(Color color) =>
            model.eyesColor = color;

        private void SetRenderScale(bool visible)
        {
            // NOTE(Brian): SSAO doesn't work correctly with the offseted avatar preview if the renderScale != 1.0
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

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
