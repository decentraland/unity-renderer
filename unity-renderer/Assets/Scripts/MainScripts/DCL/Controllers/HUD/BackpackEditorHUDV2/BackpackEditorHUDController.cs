using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.EmotesCustomization;
using DCL.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackEditorHUDController : IHUD
    {
        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        private readonly IBackpackEditorHUDView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IEmotesCatalogService emotesCatalogService;
        private IEmotesCustomizationComponentController emotesCustomizationComponentController;
        private bool isEmotesControllerInitialized;
        private CancellationTokenSource loadEmotesCTS = new ();

        public BackpackEditorHUDController(
            IBackpackEditorHUDView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IEmotesCatalogService emotesCatalogService)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.emotesCatalogService = emotesCatalogService;
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

            dataStore.HUDs.avatarEditorVisible.OnChange -= SetVisibility;
            dataStore.exploreV2.configureBackpackInFullscreenMenu.OnChange -= ConfigureBackpackInFullscreenMenuChanged;
            dataStore.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;
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
                view.Hide();
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
            if (!current)
                emotesCustomizationComponentController.RestoreEmoteSlots();
        }
    }
}
