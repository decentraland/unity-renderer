using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class AvatarSlotsHUDController : IHUD
    {
        private readonly AvatarSlotsDefinitionSO avatarSlotsDefinition;
        private readonly IAvatarSlotsView avatarSlotsView;
        private string lastSelectedSlot;

        public AvatarSlotsHUDController(IAvatarSlotsView avatarSlotsView)
        {
            this.avatarSlotsView = avatarSlotsView;
            avatarSlotsView.OnToggleAvatarSlot += ToggleSlot;
            avatarSlotsDefinition = Resources.Load<AvatarSlotsDefinitionSO>("AvatarSlotsDefinition");
            GenerateSlots();
            avatarSlotsView.SetSlotsAsHidden(new []{"hat","mask"}, "upper_body");
        }

        private void GenerateSlots()
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

        private void ToggleSlot(string slotCategory, bool isSelected)
        {
            if (isSelected && !string.IsNullOrEmpty(lastSelectedSlot))
                avatarSlotsView.DisablePreviousSlot(lastSelectedSlot);

            lastSelectedSlot = isSelected ? slotCategory : "";
        }

        public void Dispose()
        {
        }

        public void SetVisibility(bool visible)
        {
        }
    }
}
