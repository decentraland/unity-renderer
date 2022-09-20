using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteClipData
    {
        public readonly AnimationClip clip;
        public readonly bool loop;

        public EmoteClipData(AnimationClip clip, bool loop = false)
        {
            this.clip = clip;
            this.loop = loop;
        }

        public EmoteClipData(AnimationClip clip, EmoteDataV0 emoteDataV0)
        {
            this.clip = clip;
            loop = emoteDataV0?.loop ?? false;
        }
    }
}