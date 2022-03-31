using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.EmotesCustomization
{
    public interface IEmoteCardComponentView
    {
        /// <summary>
        /// Event that will be triggered when the main button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onMainClick { get; }

        /// <summary>
        /// Event that will be triggered when the info button is clicked.
        /// </summary>
        Button.ButtonClickedEvent onInfoClick { get; }

        /// <summary>
        /// It will be triggered when an emote card is selected.
        /// </summary>
        event Action<string> onEmoteSelected;

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
        /// Set the emote description in the card.
        /// </summary>
        /// <param name="description">New emote description.</param>
        void SetEmoteDescription(string description);

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
        /// Set the emote as assigned in selected slot or not.
        /// </summary>
        /// <param name="isAssigned">True for select it.</param>
        void SetEmoteAsAssignedInSelectedSlot(bool isAssigned);

        /// <summary>
        /// Assign a slot number to the emote.
        /// </summary>
        /// <param name="slotNumber">Slot number to assign.</param>
        void AssignSlot(int slotNumber);

        /// <summary>
        /// Unassign a slot number.
        /// </summary>
        void UnassignSlot();

        /// <summary>
        /// Set the emote as selected.
        /// </summary>
        /// <param name="isSelected">True for selecting it.</param>
        void SetEmoteAsSelected(bool isSelected);

        /// <summary>
        /// Set the type of rarity in the card.
        /// </summary>
        /// <param name="rarity">New rarity.</param>
        void SetRarity(string rarity);

        /// <summary>
        /// Set the emote as L2 or not.
        /// </summary>
        /// <param name="isInL2">True for set it as L2.</param>
        void SetIsInL2(bool isInL2);

        /// <summary>
        /// Set the emote as collectible or not.
        /// </summary>
        /// <param name="isInL2">True for set it as collectible.</param>
        void SetIsCollectible(bool isCollectible);

        /// <summary>
        /// Set the emote card as loading state or not.
        /// </summary>
        /// <param name="isLoading">True for setting it as loading state.</param>
        void SetAsLoading(bool isLoading);
    }
}