using DCL.Emotes;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAnimator
    {
        bool Prepare(string bodyshapeId, GameObject container);

        void PlayEmote(string emoteId, long timestamps, bool spatial, bool occlude,
            bool forcePlay);

        void StopEmote(bool immediate);
        void EquipEmote(string emoteId, EmoteClipData emoteClipData);
        void UnequipEmote(string emoteId);

        string GetCurrentEmoteId();
    }
}
