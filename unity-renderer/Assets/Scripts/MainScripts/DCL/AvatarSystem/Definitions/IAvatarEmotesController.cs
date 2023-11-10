using DCL.Emotes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatarEmotesController : IDisposable
    {
        void Prepare(string bodyShapeId, GameObject container);

        void LoadEmotes(string bodyShapeId, IEnumerable<WearableItem> emotes);

        void PlayEmote(string emoteId, long timestamps, bool spatial = true, bool occlude = true, bool forcePlay = false);

        void StopEmote(bool immediate);

        void EquipEmote(string emoteId, IEmoteReference emoteReference);

        void UnEquipEmote(string emoteId);

        event Action<string, IEmoteReference> OnEmoteEquipped;
        event Action<string> OnEmoteUnequipped;

        bool TryGetEquippedEmote(string bodyShape, string emoteId, out IEmoteReference emoteReference);

        void AddVisibilityConstraint(string key);

        void RemoveVisibilityConstraint(string key);
    }
}
