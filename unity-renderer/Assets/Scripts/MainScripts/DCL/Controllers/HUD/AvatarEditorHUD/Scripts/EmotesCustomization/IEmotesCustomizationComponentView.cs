using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.EmotesCustomization
{
    public interface IEmotesCustomizationComponentView
    {
        /// <summary>
        /// It will be triggered when an emote is equipped.
        /// </summary>
        event Action<string, int> onEmoteEquipped;

        /// <summary>
        /// It will be triggered when an emote is unequipped.
        /// </summary>
        event Action<string, int> onEmoteUnequipped;

        /// <summary>
        /// It will be triggered when the sell button of an emote detail card is clicked.
        /// </summary>
        event Action<string> onSellEmoteClicked;

        /// <summary>
        /// It will be triggered when a slot is selected.
        /// </summary>
        event Action<string, int> onSlotSelected;

        /// <summary>
        /// It represents the container transform of the component.
        /// </summary>
        Transform viewTransform { get; }

        /// <summary>
        /// Resturn true if the view is currently active.
        /// </summary>
        bool isActive { get; }

        /// <summary>
        /// Get the current selected slot number.
        /// </summary>
        int selectedSlot { get; }

        /// <summary>
        /// Get the current slots.
        /// </summary>
        List<EmoteSlotCardComponentView> currentSlots { get; }

        /// <summary>
        /// Get the current selected card.
        /// </summary>
        EmoteCardComponentView selectedCard { get; }

        /// <summary>
        /// Clean all the emotes loaded in the grid component.
        /// </summary>
        void CleanEmotes();

        /// <summary>
        /// Add an emote in the emotes grid component.
        /// </summary>
        /// <param name="emote">Emote card (model) to be added.</param>
        EmoteCardComponentView AddEmote(EmoteCardComponentModel emote);

        /// <summary>
        /// Remove an emote from the frid component.
        /// </summary>
        /// <param name="emoteId">Emote id to remove.</param>
        void RemoveEmote(string emoteId);

        /// <summary>
        /// Assign an emote into a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="emoteName">Emote name to assign.</param>
        /// <param name="slotNumber">Slot number to assign the emote.</param>
        /// <param name="selectSlotAfterEquip">Indicates if we want to keep selected the asigned slot or not.</param>
        /// <param name="notifyEvent">Indicates if the new equipped emote event should be notified or not.</param>
        void EquipEmote(string emoteId, string emoteName, int slotNumber, bool selectSlotAfterEquip = true, bool notifyEvent = true);

        /// <summary>
        /// Unassign an emote from a specific slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to unasign.</param>
        /// <param name="slotNumber">Slot number to unassign the emote.</param>
        /// <param name="notifyEvent">Indicates if the new equipped emote event should be notified or not.</param>
        void UnequipEmote(string emoteId, int slotNumber, bool notifyEvent = true);

        /// <summary>
        /// Open the info panel for a specific emote.
        /// </summary>
        /// <param name="emoteModel">Model of the emote.</param>
        /// <param name="backgroundColor">Color to apply to the panel background.</param>
        /// <param name="anchorTransform">Anchor where to place the panel.</param>
        void OpenEmoteInfoPanel(EmoteCardComponentModel emoteModel, Color backgroundColor, Transform anchorTransform);

        /// <summary>
        /// Set the info panel active/inactive.
        /// </summary>
        void SetEmoteInfoPanelActive(bool isActive);

        /// <summary>
        /// Get an emote card by id.
        /// </summary>
        /// <param name="emoteId">Emote id to search.</param>
        /// <returns>An emote card.</returns>
        EmoteCardComponentView GetEmoteCardById(string emoteId);

        /// <summary>
        /// Set the view as active or not.
        /// </summary>
        /// <param name="isActive">True for activating it.</param>
        void SetActive(bool isActive);

        /// <summary>
        /// Get a specific slot component.
        /// </summary>
        /// <param name="slotNumber">Slot number to get.</param>
        /// <returns></returns>
        EmoteSlotCardComponentView GetSlot(int slotNumber);
    }
}