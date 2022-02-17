using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Emotes
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
        /// Assign an emote into the selected slot.
        /// </summary>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="pictureSprite">Emote picture to set.</param>
        void AssignEmoteIntoSelectedSlot(string emoteId, Sprite pictureSprite);

        /// <summary>
        /// Assign an emote into a specific slot.
        /// </summary>
        /// <param name="slotNumber">Slot number to assign the emote.</param>
        /// <param name="emoteId">Emote Id to assign.</param>
        /// <param name="pictureSprite">Emote picture to set.</param>
        void AssignEmoteIntoSlot(int slotNumber, string emoteId, Sprite pictureSprite);
    }

    public class EmoteSlotSelectorComponentView : BaseComponentView, IEmoteSlotSelectorComponentView, IComponentModelConfig
    {
        [Header("Prefab References")]
        [SerializeField] internal GridContainerComponentView emotesSlots;

        [Header("Configuration")]
        [SerializeField] internal EmoteSlotSelectorComponentModel model;

        public int selectedSlot => model.selectedSlot;

        public event Action<int, string> onSlotSelected;

        public override void Start()
        {
            base.Start();

            ConfigureSlotButtons();
        }

        public void Configure(BaseComponentModel newModel)
        {
            model = (EmoteSlotSelectorComponentModel)newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SelectSlot(model.selectedSlot);
            emotesSlots.RefreshControl();
        }

        public override void Dispose()
        {
            base.Dispose();

            UnsubscribeSlotButtons();
        }

        public void SelectSlot(int slotNumber)
        {
            model.selectedSlot = slotNumber;

            if (emotesSlots == null)
                return;

            List<EmoteSlotCardComponentView> currentSlots = GetAllSlots();
            foreach (EmoteSlotCardComponentView slot in currentSlots)
            {
                if (slot.model.slotNumber == slotNumber)
                {
                    slot.SetEmoteAsSelected(true);
                    onSlotSelected?.Invoke(slotNumber, slot.model.emoteId);
                }
                else
                {
                    slot.SetEmoteAsSelected(false);
                }
            }
        }

        public void AssignEmoteIntoSelectedSlot(string emoteId, Sprite pictureSprite)
        {
            EmoteSlotCardComponentView slotToUpdate = GetAllSlots().FirstOrDefault(x => x.model.isSelected);
            if (slotToUpdate != null)
            {
                slotToUpdate.SetEmoteId(emoteId);
                slotToUpdate.SetEmotePicture(pictureSprite);
            }
        }

        public void AssignEmoteIntoSlot(int slotNumber, string emoteId, Sprite pictureSprite)
        {
            List<EmoteSlotCardComponentView> currentSlots = GetAllSlots();
            foreach (EmoteSlotCardComponentView slot in currentSlots)
            {
                if (slot.model.slotNumber == slotNumber)
                {
                    slot.SetEmoteId(emoteId);
                    slot.SetEmotePicture(pictureSprite);
                }
                else if (slot.model.emoteId == emoteId)
                {
                    slot.SetEmoteId(string.Empty);
                }
            }
        }

        internal void ConfigureSlotButtons()
        {
            List<EmoteSlotCardComponentView> currentSlots = GetAllSlots();
            foreach (EmoteSlotCardComponentView slot in currentSlots)
            {
                slot.onClick.AddListener(() => SelectSlot(slot.model.slotNumber));
            }
        }

        internal void UnsubscribeSlotButtons()
        {
            List<EmoteSlotCardComponentView> currentSlots = emotesSlots
                .GetItems()
                .Select(x => x as EmoteSlotCardComponentView)
                .ToList();

            foreach (EmoteSlotCardComponentView slot in currentSlots)
            {
                slot.onClick.RemoveAllListeners();
            }
        }

        internal List<EmoteSlotCardComponentView> GetAllSlots()
        {
            return emotesSlots
                .GetItems()
                .Select(x => x as EmoteSlotCardComponentView)
                .ToList();
        }
    }
}