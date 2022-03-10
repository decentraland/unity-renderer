using System.Collections.Generic;
using DCL.Emotes;
using UnityEngine;

namespace DCL
{
    public class DataStore_Emotes
    {
        public readonly BaseRefCountedCollection<string> emotesOnUse = new BaseRefCountedCollection<string>();
        public readonly BaseDictionary<(string bodyshapeId, string emoteId), AnimationClip> animations = new BaseDictionary<(string bodyshapeId, string emoteId), AnimationClip>();
    }
}