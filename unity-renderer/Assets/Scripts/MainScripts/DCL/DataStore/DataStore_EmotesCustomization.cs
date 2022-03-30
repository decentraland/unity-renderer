using System.Collections.Generic;
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
    }
}