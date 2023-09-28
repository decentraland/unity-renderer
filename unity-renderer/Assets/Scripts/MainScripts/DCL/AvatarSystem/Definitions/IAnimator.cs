using DCL.Emotes;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAnimator
    {
        bool Prepare(string bodyshapeId, GameObject container);
        void PlayEmote(string emoteId, long timestamps, bool spatial, float volume, bool occlude,
            bool ignoreTimestamp);
        void StopEmote();
        void EquipEmote(string emoteId, EmoteClipData emoteClipData);
        void UnequipEmote(string emoteId);

    }
}
