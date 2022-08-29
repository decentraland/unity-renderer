using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteClipData
    {
        public readonly AnimationClip Clip;
        public readonly bool Loop;

        public EmoteClipData(AnimationClip clip)
        {
            Clip = clip;
        }

        public EmoteClipData(AnimationClip clip, EmoteDataV0 emoteDataV0)
        {
            Clip = clip;
            Loop = emoteDataV0.loop;
        }
    }
}