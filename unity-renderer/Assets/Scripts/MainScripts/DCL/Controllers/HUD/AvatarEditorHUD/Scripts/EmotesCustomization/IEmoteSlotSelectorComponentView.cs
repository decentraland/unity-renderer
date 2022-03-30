using System;
using UnityEngine;

namespace DCL.EmotesCustomization
{
    public interface IEmoteSlotSelectorComponentView
    {
        /// <summary>
        /// Get the current selected slot number.
        /// </summary>
        int selectedSlot { get; }

        /// <summary>
        /// It will be triggered when a slot is selected. It returns the selected slot number and the assigned emote id.
        /// </summary>
        event Action<int, string> onSlotSelected;

        /// <summary>
        /// Select a slot.
        /// </summary>
        /// <param name="slotNumber">Slot number to select.</param>
        void SelectSlot(int slotNumber);

        /// <summary>
        /// Assign an emote into a specific slot.
        /// </summary>
        /// <param name="slotNumber">Slot number to assign the emote.</param>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="emoteName">Emote name to assign.</param>
        /// <param name="pictureSprite">Emote picture to set.</param>
        /// <param name="pictureUri">Emote picture uri to set (if no sprite is passed).</param>
        /// <param name="rarity">Emote rarity to set.</param>
        void AssignEmoteIntoSlot(int slotNumber, string emoteId, string emoteName, Sprite pictureSprite, string pictureUri, string rarity);
    }
}