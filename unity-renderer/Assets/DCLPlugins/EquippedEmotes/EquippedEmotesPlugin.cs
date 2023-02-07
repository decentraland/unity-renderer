using DCL.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.EquippedEmotes
{
    /// <summary>
    /// Plugin feature that initialize the Equipped Emotes feature.
    /// </summary>
    public class EquippedEmotesInitializerPlugin : IPlugin
    {
        internal const string PLAYER_PREFS_EQUIPPED_EMOTES_KEY = "EquippedNFTEmotes";

        internal DataStore_EmotesCustomization emotesCustomizationDataStore => DataStore.i.emotesCustomization;
        internal DataStore_Common commonDataStore => DataStore.i.common;
        internal DataStore_FeatureFlag featureFlagsDataStore => DataStore.i.featureFlags;
        internal UserProfile ownUserProfile;

        public EquippedEmotesInitializerPlugin()
        {
            ownUserProfile = UserProfile.GetOwnUserProfile();
            ownUserProfile.OnUpdate += OnOwnUserProfileUpdated;
            LoadDefaultEquippedEmotes();

            LoadEquippedEmotesFromLocalStorage();

            emotesCustomizationDataStore.equippedEmotes.OnSet += OnEquippedEmotesSet;
            emotesCustomizationDataStore.equippedEmotes.OnAdded += OnEquippedEmoteAddedOrRemoved;
            emotesCustomizationDataStore.equippedEmotes.OnRemoved += OnEquippedEmoteAddedOrRemoved;
            commonDataStore.isSignUpFlow.OnChange += OnSignupFlowChanged;
        }

        private void OnSignupFlowChanged(bool current, bool previous)
        {
            if (current)
                LoadDefaultEquippedEmotes();
        }

        private void OnOwnUserProfileUpdated(UserProfile userProfile)
        {
            if (userProfile == null || userProfile.avatar == null || userProfile.avatar.emotes.Count == 0)
                return;

            List<string> equippedEmotes = new List<string> (Enumerable.Repeat((string) null, 10));
            foreach (AvatarModel.AvatarEmoteEntry avatarEmoteEntry in userProfile.avatar.emotes)
            {
                equippedEmotes[avatarEmoteEntry.slot] = avatarEmoteEntry.urn;
            }
            SetEquippedEmotes(equippedEmotes);
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

        internal void LoadDefaultEquippedEmotes() { SetEquippedEmotes(GetDefaultEmotes()); }

        internal void LoadEquippedEmotesFromLocalStorage()
        {
            List<string> storedEquippedEmotes;

            try
            {
                storedEquippedEmotes = JsonConvert.DeserializeObject<List<string>>(PlayerPrefsBridge.GetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY));
            }
            catch
            {
                storedEquippedEmotes = null;
            }

            if (storedEquippedEmotes == null)
                storedEquippedEmotes = GetDefaultEmotes();
            
            SetEquippedEmotes(storedEquippedEmotes);
        }

        internal void OnEquippedEmotesSet(IEnumerable<EquippedEmoteData> equippedEmotes)
        {
          
        }

        internal void OnEquippedEmoteAddedOrRemoved(EquippedEmoteData equippedEmote)
        {
          
        }

        internal void SaveEquippedEmotesInLocalStorage()
        {
            List<string> emotesIdsToStore = new List<string>();
            foreach (EquippedEmoteData equippedEmoteData in emotesCustomizationDataStore.equippedEmotes.Get())
            {
                emotesIdsToStore.Add(equippedEmoteData != null ? equippedEmoteData.id : null);
            }

            // TODO: We should avoid static calls and create injectable interfaces
            PlayerPrefsBridge.SetString(PLAYER_PREFS_EQUIPPED_EMOTES_KEY, JsonConvert.SerializeObject(emotesIdsToStore));
            PlayerPrefsBridge.Save();
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
            emotesCustomizationDataStore.equippedEmotes.OnSet -= OnEquippedEmotesSet;
            emotesCustomizationDataStore.equippedEmotes.OnAdded -= OnEquippedEmoteAddedOrRemoved;
            emotesCustomizationDataStore.equippedEmotes.OnRemoved -= OnEquippedEmoteAddedOrRemoved;
            commonDataStore.isSignUpFlow.OnChange -= OnSignupFlowChanged;
            if (ownUserProfile != null)
                ownUserProfile.OnUpdate -= OnOwnUserProfileUpdated;
        }
    }
}