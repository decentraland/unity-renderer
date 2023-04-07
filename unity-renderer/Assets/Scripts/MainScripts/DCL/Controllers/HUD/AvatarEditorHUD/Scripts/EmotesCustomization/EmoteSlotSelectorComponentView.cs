using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCL.EmotesCustomization
{
    public class EmoteSlotSelectorComponentView : BaseComponentView, IEmoteSlotSelectorComponentView, IComponentModelConfig<EmoteSlotSelectorComponentModel>
    {
        [Header("Prefab References")]
        [SerializeField] internal GridContainerComponentView emotesSlots;

        [Header("Configuration")]
        [SerializeField] internal EmoteSlotSelectorComponentModel model;

        public int selectedSlot => model.selectedSlot;

        public event Action<int, string> onSlotSelected;

        public void Start()
        {
            ConfigureSlotButtons();
        }

        public void Configure(EmoteSlotSelectorComponentModel newModel)
        {
            if (model == newModel)
                return;

            model = newModel;
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

        public void AssignEmoteIntoSlot(int slotNumber, string emoteId, string emoteName, Sprite pictureSprite, string pictureUri, string rarity)
        {
            List<EmoteSlotCardComponentView> currentSlots = GetAllSlots();
            foreach (EmoteSlotCardComponentView slot in currentSlots)
            {
                if (slot.model.slotNumber == slotNumber)
                {
                    slot.SetEmoteName(emoteName);
                    if (pictureSprite != null)
                    {
                        slot.SetEmotePicture(pictureSprite);
                        slot.model.pictureUri = pictureUri;
                    }
                    else
                        slot.SetEmotePicture(pictureUri);
                    slot.SetEmoteId(emoteId);
                    slot.SetRarity(rarity);
                }
                else if (slot.model.emoteId == emoteId)
                {
                    slot.SetEmoteId(string.Empty);
                    slot.SetEmoteName(string.Empty);
                    slot.SetRarity(string.Empty);
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
            IEnumerable<EmoteSlotCardComponentView> currentSlots = emotesSlots
                .GetItems()
                .Select(x => x as EmoteSlotCardComponentView);

            foreach (EmoteSlotCardComponentView slot in currentSlots)
            {
                slot.onClick.RemoveAllListeners();
            }
        }

        internal List<EmoteSlotCardComponentView> GetAllSlots()
        {
            if (emotesSlots == null)
            {
                Debug.LogError("EmotesSlotSelectorComponentView: emotesSlots are accessed before serialized reference was initialized");
                return new List<EmoteSlotCardComponentView>();
            }

            return emotesSlots
                .GetItems()
                .Select(x => x as EmoteSlotCardComponentView)
                .ToList();
        }
    }
}
