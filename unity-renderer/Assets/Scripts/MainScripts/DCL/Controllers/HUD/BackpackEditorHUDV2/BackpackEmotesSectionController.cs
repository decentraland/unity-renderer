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
    public class BackpackEmotesSectionController : IBackpackEmotesSectionController
    {
        public event Action<string> OnNewEmoteAdded;
        public event Action<string> OnEmotePreviewed;

        private DataStore dataStore;
        private IUserProfileBridge userProfileBridge;
        private IEmotesCatalogService emotesCatalogService;
        private IEmotesCustomizationComponentController emotesCustomizationComponentController;
        private CancellationTokenSource loadEmotesCts = new ();

        public BackpackEmotesSectionController(
            DataStore dataStore,
            Transform emotesSectionTransform,
            IUserProfileBridge userProfileBridge,
            IEmotesCatalogService emotesCatalogService)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.emotesCatalogService = emotesCatalogService;

            emotesCustomizationComponentController = new EmotesCustomizationComponentController(
                dataStore.emotesCustomization,
                dataStore.emotes,
                dataStore.exploreV2,
                dataStore.HUDs,
                emotesSectionTransform);

            emotesCustomizationComponentController.SetEquippedBodyShape(userProfileBridge.GetOwn().avatar.bodyShape);

            dataStore.emotesCustomization.currentLoadedEmotes.OnAdded += NewEmoteAdded;
            emotesCustomizationComponentController.onEmotePreviewed += EmotePreviewed;
        }

        public void Dispose()
        {
            loadEmotesCts.SafeCancelAndDispose();
            loadEmotesCts = null;

            dataStore.emotesCustomization.currentLoadedEmotes.OnAdded -= NewEmoteAdded;
            emotesCustomizationComponentController.onEmotePreviewed -= EmotePreviewed;
        }

        public void LoadEmotes()
        {
            async UniTaskVoid LoadEmotesAsync(CancellationToken ct = default)
            {
                try
                {
                    EmbeddedEmotesSO embeddedEmotesSo = await emotesCatalogService.GetEmbeddedEmotes();
                    var baseEmotes = embeddedEmotesSo.emotes;
                    var ownedEmotes = await emotesCatalogService.RequestOwnedEmotesAsync(userProfileBridge.GetOwn().userId, ct);
                    var allEmotes = ownedEmotes == null ? baseEmotes : baseEmotes.Concat(ownedEmotes).ToArray();
                    dataStore.emotesCustomization.UnequipMissingEmotes(allEmotes);
                    emotesCustomizationComponentController.SetEmotes(allEmotes);

                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading emotes: {e.Message}");
                }
            }

            loadEmotesCts = loadEmotesCts.SafeRestart();
            LoadEmotesAsync(loadEmotesCts.Token).Forget();
        }

        public void RestoreEmoteSlots() =>
            emotesCustomizationComponentController.RestoreEmoteSlots();

        public void SetEquippedBodyShape(string bodyShapeId) =>
            emotesCustomizationComponentController.SetEquippedBodyShape(bodyShapeId);

        private void NewEmoteAdded(string emoteId) =>
            OnNewEmoteAdded?.Invoke(emoteId);

        private void EmotePreviewed(string emoteId) =>
            OnEmotePreviewed?.Invoke(emoteId);
    }
}
