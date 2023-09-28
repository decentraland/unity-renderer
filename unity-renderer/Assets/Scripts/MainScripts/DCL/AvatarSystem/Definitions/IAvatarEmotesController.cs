using DCL.Emotes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatarEmotesController : IDisposable
    {
        void LoadEmotes(string bodyShapeId, IEnumerable<WearableItem> emotes, GameObject container);

        void PlayEmote(string emoteId, long timestamps, bool spatial = true, bool occlude = true, bool ignoreTimestamp = false);

        void StopEmote();

        void EquipEmote(string emoteId, IEmoteReference emoteReference);

        void UnEquipEmote(string emoteId);

        event Action<string, IEmoteReference> OnEmoteEquipped;
        event Action<string> OnEmoteUnequipped;

        bool TryGetEquippedEmote(string bodyShape, string emoteId, out IEmoteReference emoteReference);
    }
}
