using DCL.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DCL.EquippedEmotes
{
    /// <summary>
    /// Plugin feature that initialize the Equipped Emotes feature.
    /// </summary>
    public class EquippedEmotesPlugin : IPlugin
    {
        internal const string PLAYER_PREFS_EQUIPPED_EMOTES_KEY = "EquippedNFTEmotes";

        internal DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;

        public EquippedEmotesPlugin()
        {
            LoadDefaultEquippedEmotes();

            emotesCustomizationDataStore.isEmotesCustomizationInitialized.OnChange += OnEmotesCustomizationInitialized;
            OnEmotesCustomizationInitialized(emotesCustomizationDataStore.isEmotesCustomizationInitialized.Get(), false);

            emotesCustomizationDataStore.equippedEmotes.OnSet += SaveEquippedEmotesInLocalStorage;
        }

        private void OnEmotesCustomizationInitialized(bool current, bool previous)
        {
            if (!current)
                return;

            emotesCustomizationDataStore.isEmotesCustomizationInitialized.OnChange -= OnEmotesCustomizationInitialized;
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

        internal void SaveEquippedEmotesInLocalStorage(IEnumerable<EquippedEmoteData> equippedEmotes)
        {
            if (!emotesCustomizationDataStore.isEmotesCustomizationInitialized.Get())
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
            emotesCustomizationDataStore.isEmotesCustomizationInitialized.OnChange -= OnEmotesCustomizationInitialized;
            emotesCustomizationDataStore.equippedEmotes.OnSet -= SaveEquippedEmotesInLocalStorage;
        }
    }
}