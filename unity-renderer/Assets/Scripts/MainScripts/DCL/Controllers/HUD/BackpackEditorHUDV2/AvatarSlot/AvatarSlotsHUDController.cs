using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Backpack
{
    public class AvatarSlotsHUDController : IHUD
    {
        internal AvatarSlotsDefinitionSO avatarSlotsDefinition;
        internal readonly IAvatarSlotsView avatarSlotsView;
        internal string lastSelectedSlot;

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

        public void ToggleSlot(string slotCategory, bool isSelected)
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
