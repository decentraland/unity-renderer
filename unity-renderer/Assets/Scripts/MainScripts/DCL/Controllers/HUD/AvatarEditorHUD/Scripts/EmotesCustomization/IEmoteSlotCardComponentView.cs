using UnityEngine;
using UnityEngine.UI;

namespace DCL.EmotesCustomization
{
    public interface IEmoteSlotCardComponentView
    {
        /// <summary>
        /// Event that will be triggered when the card is clicked.
        /// </summary>
        Button.ButtonClickedEvent onClick { get; }

        /// <summary>
        /// Set the emote id in the card.
        /// </summary>
        /// <param name="id">New emote id.</param>
        void SetEmoteId(string id);

        /// <summary>
        /// Set the emote name in the card.
        /// </summary>
        /// <param name="name">New emote name.</param>
        void SetEmoteName(string name);

        /// <summary>
        /// Set the emote picture directly from a sprite.
        /// </summary>
        /// <param name="sprite">Emote picture (sprite).</param>
        void SetEmotePicture(Sprite sprite);

        /// <summary>
        /// Set the emote picture from an uri.
        /// </summary>
        /// <param name="uri">Emote picture (url).</param>
        void SetEmotePicture(string uri);

        /// <summary>
        /// Set the emote as selected or not.
        /// </summary>
        /// <param name="isSelected">True for select it.</param>
        void SetEmoteAsSelected(bool isSelected);

        /// <summary>
        /// Set the slot number in the card.
        /// </summary>
        /// <param name="slotNumber">Slot number of the card (between 0 and 9).</param>
        void SetSlotNumber(int slotNumber);

        /// <summary>
        /// Set the emote separator as active or not.
        /// </summary>
        /// <param name="isActive">True for activating it.</param>
        void SetSeparatorActive(bool isActive);

        /// <summary>
        /// Set the type of rarity in the card.
        /// </summary>
        /// <param name="rarity">New rarity.</param>
        void SetRarity(string rarity);
    }
}