using DCL;
using DCL.Emotes;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAnimator
    {
        bool Prepare(string bodyshapeId, GameObject container);
        void PlayEmote(string emoteId, long timestamps);
        void EquipEmote(string emoteId, EmoteClipData emoteClipData);
        void UnequipEmote(string emoteId);
    }
}