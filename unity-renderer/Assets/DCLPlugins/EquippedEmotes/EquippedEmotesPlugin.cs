using DCL.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.EquippedEmotes
{
    /// <summary>
    /// Plugin feature that initialize the Equipped Emotes feature.
    /// </summary>
    public class EquippedEmotesInitializerPlugin : IPlugin
    {
        internal const string EMOTES_CUSTOMIZATION_FEATURE_FLAG = "emotes_customization";
        internal const string PLAYER_PREFS_EQUIPPED_EMOTES_KEY = "EquippedNFTEmotes";

        internal DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;
        internal DataStore_FeatureFlag featureFlagsDataStore => DataStore.i.featureFlags;

        public EquippedEmotesInitializerPlugin()
        {
            LoadDefaultEquippedEmotes();

            featureFlagsDataStore.flags.OnChange += OnFeatureFlagsChanged;
            OnFeatureFlagsChanged(featureFlagsDataStore.flags.Get(), null);

            emotesCustomizationDataStore.equippedEmotes.OnSet += OnEquippedEmotesSet;
            emotesCustomizationDataStore.equippedEmotes.OnAdded += OnEquippedEmoteAddedOrRemoved;
            emotesCustomizationDataStore.equippedEmotes.OnRemoved += OnEquippedEmoteAddedOrRemoved;
        }

        internal List<string> GetDefaultEmotes()
        {
            return new List<string>
            {
                "handsair",
                "wave",
                "fistpump",
                "dance",
                "raiseHand",
                "clap",
                "money",
                "kiss",
                "headexplode",
                "shrug"
            };
        }

        internal void OnFeatureFlagsChanged(FeatureFlag current, FeatureFlag previous)
        {
            if (!current.IsFeatureEnabled(EMOTES_CUSTOMIZATION_FEATURE_FLAG))
                return;

            featureFlagsDataStore.flags.OnChange -= OnFeatureFlagsChanged;
            LoadEquippedEmotesFromLocalStorage();
        }

        internal void LoadDefaultEquippedEmotes() { SetEquippedEmotes(GetDefaultEmotes()); }

        internal void LoadEquippedEmotesFromLocalStorage()
        {
            List<string> storedEquippedEmotes;

            try
            {
                storedEquippedEmotes = JsonConvert.DeserializeObject<List<string>>(PlayerPrefsUtils.GetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY));
            }
            catch
            {
                storedEquippedEmotes = null;
            }

            if (storedEquippedEmotes == null)
                storedEquippedEmotes = GetDefaultEmotes();
            
            SetEquippedEmotes(storedEquippedEmotes);
        }

        internal void OnEquippedEmotesSet(IEnumerable<EquippedEmoteData> equippedEmotes) { SaveEquippedEmotesInLocalStorage(); }

        internal void OnEquippedEmoteAddedOrRemoved(EquippedEmoteData equippedEmote) { SaveEquippedEmotesInLocalStorage(); }

        internal void SaveEquippedEmotesInLocalStorage()
        {
            if (!featureFlagsDataStore.flags.Get().IsFeatureEnabled(EMOTES_CUSTOMIZATION_FEATURE_FLAG))
                return;

            List<string> emotesIdsToStore = new List<string>();
            foreach (EquippedEmoteData equippedEmoteData in emotesCustomizationDataStore.equippedEmotes.Get())
            {
                emotesIdsToStore.Add(equippedEmoteData != null ? equippedEmoteData.id : null);
            }

            // TODO: We should avoid static calls and create injectable interfaces
            PlayerPrefsUtils.SetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY, JsonConvert.SerializeObject(emotesIdsToStore));
            PlayerPrefsUtils.Save();
        }

        internal void SetEquippedEmotes(List<string> storedEquippedEmotes)
        {
            List<EquippedEmoteData> storedEquippedEmotesData = new List<EquippedEmoteData>();
            foreach (string emoteId in storedEquippedEmotes)
            {
                storedEquippedEmotesData.Add(
                    string.IsNullOrEmpty(emoteId) ? null : new EquippedEmoteData { id = emoteId, cachedThumbnail = null });
            }
            emotesCustomizationDataStore.equippedEmotes.Set(storedEquippedEmotesData);
        }

        public void Dispose()
        {
            featureFlagsDataStore.flags.OnChange -= OnFeatureFlagsChanged;
            emotesCustomizationDataStore.equippedEmotes.OnSet -= OnEquippedEmotesSet;
            emotesCustomizationDataStore.equippedEmotes.OnAdded -= OnEquippedEmoteAddedOrRemoved;
            emotesCustomizationDataStore.equippedEmotes.OnRemoved -= OnEquippedEmoteAddedOrRemoved;
        }
    }
}