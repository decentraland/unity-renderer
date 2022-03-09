using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DataStore_EmotesCustomization
    {
        public readonly BaseVariable<Transform> isInitialized = new BaseVariable<Transform>(null);
        public readonly BaseCollection<string> equippedEmotes = new BaseCollection<string>(new List<string> { null, null, null, null, null, null, null, null, null, null });
        public readonly BaseVariable<bool> isEmotesCustomizationSelected = new BaseVariable<bool>(false);
    }
}