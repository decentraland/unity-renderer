using DCL.Emotes;
using DCLServices.EmotesService.Domain;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAnimator
    {
        bool Prepare(string bodyshapeId, GameObject container);
        void PlayEmote(string emoteId, long timestamps, bool spatial, float volume, bool occlude);
        void StopEmote();
        void EquipEmote(string emoteId, EmoteAnimationData emoteAnimationData);
        void UnequipEmote(string emoteId);

    }
}
