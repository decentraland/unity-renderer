using System.Collections.Generic;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteAnimationsPlugin : IPlugin
    {
        private readonly DataStore_Emotes dataStore;

        public EmoteAnimationsPlugin(DataStore_Emotes dataStore)
        {
            this.dataStore = dataStore;
            this.dataStore.animations.Clear();
            this.dataStore.emotesOnUse.OnRefCountUpdated += OnRefCountUpdated;

            InitializeEmbeddedEmotes();
            InitializeEmotes(this.dataStore.emotesOnUse.GetAllRefCounts());
        }

        private void InitializeEmbeddedEmotes()
        {
            //To avoid circular references in assemblies we hardcode this here instead of using WearableLiterals
            //Embedded Emotes are only temporary until they can be retrieved from the content server
            const string FEMALE = "urn:decentraland:off-chain:base-avatars:BaseFemale";
            const string MALE = "urn:decentraland:off-chain:base-avatars:BaseMale";

            var embeddedEmotes = Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes");

            foreach (EmbeddedEmote embeddedEmote in embeddedEmotes.emotes)
            {
                if (embeddedEmote.maleAnimation != null)
                {
                    //We match the animation id with its name due to performance reasons
                    //Unity's Animation uses the name to play the clips.
                    embeddedEmote.maleAnimation.name = embeddedEmote.id;
                    dataStore.animations.Add((MALE, embeddedEmote.id), embeddedEmote.maleAnimation);
                }

                if (embeddedEmote.femaleAnimation != null)
                {
                    //We match the animation id with its name due to performance reasons
                    //Unity's Animation uses the name to play the clips.
                    embeddedEmote.femaleAnimation.name = embeddedEmote.id;
                    dataStore.animations.Add((FEMALE, embeddedEmote.id), embeddedEmote.femaleAnimation);
                }
            }
            CatalogController.i.EmbedWearables(embeddedEmotes.emotes);
        }

        private void OnRefCountUpdated(string emoteId, int refCount)
        {
            if (refCount > 0)
                LoadEmote(emoteId);
            else
                UnloadEmote(emoteId);
        }

        private void InitializeEmotes(IEnumerable<KeyValuePair<string, int>> refCounts)
        {
            foreach (KeyValuePair<string, int> keyValuePair in refCounts)
            {
                LoadEmote(keyValuePair.Key);
            }
        }

        private void LoadEmote(string id)
        {
            //TODO when working with emotes in the content server
        }

        private void UnloadEmote(string id)
        {
            //TODO when working with emotes in the content server
        }

        public void Dispose() { this.dataStore.emotesOnUse.OnRefCountUpdated -= OnRefCountUpdated; }
    }

}