using DCL.Emotes;
using System;
using System.Collections.Generic;

namespace AvatarSystem
{
    public interface IAvatarEmotesController : IDisposable
    {
        void LoadEmotes(string bodyShapeId, IEnumerable<WearableItem> emotes);

        void PlayEmote(string emoteId, long timestamps, bool spatialSound = true);

        void EquipEmote(string emoteId, IEmoteReference emoteReference);

        void UnEquipEmote(string emoteId);

        event Action<string, IEmoteReference> OnEmoteEquipped;
        event Action<string> OnEmoteUnequipped;

        bool TryGetEquippedEmote(string bodyShape, string emoteId, out IEmoteReference emoteReference);
    }
}
