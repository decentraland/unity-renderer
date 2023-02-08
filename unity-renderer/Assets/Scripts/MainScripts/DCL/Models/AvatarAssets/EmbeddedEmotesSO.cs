using System;
using UnityEngine;

namespace DCL.Emotes
{
    [CreateAssetMenu(menuName = "DCL/Emotes/EmbeddedEmotes", fileName = "EmbeddedEmotes")]
    public class EmbeddedEmotesSO : ScriptableObject
    {
        public EmbeddedEmote[] emotes;
    }

    [Serializable]
    public class EmbeddedEmote : WearableItem
    {
        public AnimationClip femaleAnimation;
        public AnimationClip maleAnimation;
        public bool dontShowInBackpack;

        public override bool ShowInBackpack() { return !dontShowInBackpack; }
    }
}
