using System.Collections.Generic;
using DCL.Emotes;
using UnityEngine;

namespace DCL
{
    public class DataStore_Emotes
    {
        public readonly BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesOnUse = new BaseRefCountedCollection<(string bodyshapeId, string emoteId)>();
        public readonly BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations = new BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData>();


    }
}