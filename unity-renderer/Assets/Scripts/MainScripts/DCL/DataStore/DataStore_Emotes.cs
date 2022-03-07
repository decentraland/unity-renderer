using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    [Serializable]
    public class StoredEmote
    {
        public string id;
        public string name;
        public string pictureUri;
    }

    public class DataStore_Emotes
    {
        public readonly BaseVariable<Transform> isInitialized = new BaseVariable<Transform>(null);
        public readonly BaseCollection<StoredEmote> equippedEmotes = new BaseCollection<StoredEmote>(new List<StoredEmote> { null, null, null, null, null, null, null, null, null, null });
        public readonly BaseVariable<bool> isEmotesCustomizationSelected = new BaseVariable<bool>(false);
    }
}