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
        public readonly BaseVariable<Transform> isInitialized = new BaseVariable<Transform>(null);
        public readonly BaseCollection<EquippedEmoteData> equippedEmotes = new BaseCollection<EquippedEmoteData>(new List<EquippedEmoteData> { null, null, null, null, null, null, null, null, null, null });
        public readonly BaseVariable<bool> isEmotesCustomizationSelected = new BaseVariable<bool>(false);
        public readonly BaseCollection<string> currentLoadedEmotes = new BaseCollection<string>();
        public readonly BaseVariable<string> emoteForPreviewing = new BaseVariable<string>(null);
        public readonly BaseVariable<string> emoteForEquipping = new BaseVariable<string>(null);
        public readonly BaseVariable<string> emoteForUnequipping = new BaseVariable<string>(null);
        public readonly BaseVariable<bool> avatarHasBeenSaved = new BaseVariable<bool>(false);
    }
}