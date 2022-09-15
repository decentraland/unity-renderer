using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAnimator
    {
        bool Prepare(string bodyshapeId, GameObject container);
        void PlayEmote(string emoteId, long timestamps);
        void EquipEmote(string emoteId, AnimationClip clip);
        void UnequipEmote(string emoteId);
    }
}