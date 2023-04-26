using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class AvatarSlotsHUDController
    {
        public event Action<string, bool, bool> OnToggleSlot;

        private readonly IAvatarSlotsView avatarSlotsView;
        private string lastSelectedSlot;
        internal AvatarSlotsDefinitionSO avatarSlotsDefinition;

        public AvatarSlotsHUDController(IAvatarSlotsView avatarSlotsView)
        {
            this.avatarSlotsView = avatarSlotsView;
            avatarSlotsView.OnToggleAvatarSlot += ToggleSlot;
            avatarSlotsDefinition = Resources.Load<AvatarSlotsDefinitionSO>("AvatarSlotsDefinition");
        }

        public void GenerateSlots()
        {
            for (var i = 0; i < avatarSlotsDefinition.slotsDefinition.Length; i++)
            {
                SerializableKeyValuePair<string, List<string>> section = avatarSlotsDefinition.slotsDefinition[i];
                avatarSlotsView.CreateAvatarSlotSection(section.key, i < avatarSlotsDefinition.slotsDefinition.Length - 1);
                foreach (string avatarSlotSection in section.value)
                    avatarSlotsView.AddSlotToSection(section.key, avatarSlotSection);
            }
            avatarSlotsView.RebuildLayout();
        }

        public void ToggleSlot(string slotCategory, bool supportColor, bool isSelected)
        {
            if (isSelected && !string.IsNullOrEmpty(lastSelectedSlot))
                avatarSlotsView.DisablePreviousSlot(lastSelectedSlot);

            lastSelectedSlot = isSelected ? slotCategory : "";

            OnToggleSlot?.Invoke(slotCategory, supportColor, isSelected);
        }

        public void Dispose()
        {
            avatarSlotsView.OnToggleAvatarSlot -= ToggleSlot;
        }

        public void Equip(WearableItem wearableItem)
        {
            avatarSlotsView.SetSlotRarity(wearableItem.data.category, wearableItem.rarity);
            avatarSlotsView.SetSlotNftImage(wearableItem.data.category, wearableItem.ComposeThumbnailUrl());
        }

        public void UnEquip(string category, string wearableId) =>
            avatarSlotsView.ResetCategorySlot(category);
    }
}
