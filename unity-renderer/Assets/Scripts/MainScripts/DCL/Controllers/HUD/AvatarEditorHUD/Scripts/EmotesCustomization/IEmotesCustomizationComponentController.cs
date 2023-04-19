using System;
using UnityEngine;

namespace DCL.EmotesCustomization
{
    public interface IEmotesCustomizationComponentController : IDisposable
    {
        /// <summary>
        /// It will be triggered when an emote should be previewed.
        /// </summary>
        event Action<string> onEmotePreviewed;

        /// <summary>
        /// It will be triggered when an emote is equipped.
        /// </summary>
        event Action<string> onEmoteEquipped;

        /// <summary>
        /// It will be triggered when an emote is unequipped.
        /// </summary>
        event Action<string> onEmoteUnequipped;

        /// <summary>
        /// It will be triggered when the sell button is clicked for an emote.
        /// </summary>
        event Action<string> onEmoteSell;

        /// <summary>
        /// Set the owned emotes
        /// </summary>
        /// <param name="ownedEmotes"></param>
        void SetEmotes(WearableItem[] ownedEmotes);

        /// <summary>
        /// Set the current equipped bodyshape
        /// </summary>
        /// <param name="bodyShapeId"></param>
        void SetEquippedBodyShape(string bodyShapeId);

        /// <summary>
        /// Restore the emote slots with the stored data.
        /// </summary>
        void RestoreEmoteSlots();
    }
}
