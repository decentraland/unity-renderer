using UnityEngine;

namespace DCL
{
    public class DataStore_Emotes
    {
        public readonly BaseCollection<string> equippedEmotes = new BaseCollection<string>();
        public readonly BaseDictionary<(string id, string bodyshapeId), AnimationClip> animations = new BaseDictionary<(string id, string bodyshapeId), AnimationClip>();
    }
}