using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class EquippedEmoteData
    {
        public string id;
        public Sprite cachedThumbnail;
    }

    public class DataStore_EmotesCustomization
    {
        public readonly BaseVariable<bool> isWheelInitialized = new BaseVariable<bool>(false);
        public readonly BaseCollection<EquippedEmoteData> unsavedEquippedEmotes = new BaseCollection<EquippedEmoteData>(new List<EquippedEmoteData> { null, null, null, null, null, null, null, null, null, null });
        public readonly BaseCollection<EquippedEmoteData> equippedEmotes = new BaseCollection<EquippedEmoteData>(new List<EquippedEmoteData> { null, null, null, null, null, null, null, null, null, null });
        public readonly BaseVariable<bool> isEmotesCustomizationSelected = new BaseVariable<bool>(false);
        public readonly BaseCollection<string> currentLoadedEmotes = new BaseCollection<string>();

        public void FilterOutNotOwnedEquippedEmotes(IEnumerable<WearableItem> emotes)
        {
            var filtered = equippedEmotes.Get().Where(x => emotes.Any(y => x.id == y.id) );
            equippedEmotes.Set(filtered);
        }
    }
}