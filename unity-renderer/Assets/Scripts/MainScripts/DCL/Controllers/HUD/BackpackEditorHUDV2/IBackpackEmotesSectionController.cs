using System;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEmotesSectionController : IDisposable
    {
        event Action<string> OnNewEmoteAdded;
        event Action<string> OnEmotePreviewed;
        event Action<string> OnEmoteEquipped;
        event Action<string> OnEmoteUnequipped;

        void Initialize(DataStore dataStore, Transform emotesSectionTransform, IUserProfileBridge userProfileBridge, IEmotesCatalogService emotesCatalogService);
        void LoadEmotes();
        void RestoreEmoteSlots();
        void SetEquippedBodyShape(string bodyShapeId);
    }
}
