using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class AvatarSlotsHUDController
    {
        public event Action<string, bool, CharacterPreviewController.CameraFocus, float?, bool> OnToggleSlot;

        private readonly IAvatarSlotsView avatarSlotsView;
        private string lastSelectedSlot;
        internal AvatarSlotsDefinitionSO avatarSlotsDefinition;

        public event Action<string, UnequipWearableSource> OnUnequipFromSlot;

        public AvatarSlotsHUDController(IAvatarSlotsView avatarSlotsView)
        {
            this.avatarSlotsView = avatarSlotsView;
            avatarSlotsView.OnToggleAvatarSlot += ToggleSlot;
            avatarSlotsView.OnUnequipFromSlot += (wearableId) => OnUnequipFromSlot?.Invoke(wearableId, UnequipWearableSource.AvatarSlot);
            avatarSlotsDefinition = Resources.Load<AvatarSlotsDefinitionSO>("AvatarSlotsDefinition");
        }

        public void GenerateSlots()
        {
            for (var i = 0; i < avatarSlotsDefinition.slotsDefinition.Length; i++)
            {
                SerializableKeyValuePair<string, List<string>> section = avatarSlotsDefinition.slotsDefinition[i];
                avatarSlotsView.CreateAvatarSlotSection(section.key, i < avatarSlotsDefinition.slotsDefinition.Length - 1);
                foreach (string avatarSlotSection in section.value)
                    avatarSlotsView.AddSlotToSection(section.key, avatarSlotSection, CanAvatarSlotBeUnEquipped(avatarSlotSection));
            }
            avatarSlotsView.RebuildLayout();
        }

        // TODO: this method should be private
        public void ToggleSlot(string slotCategory, bool supportColor, CharacterPreviewController.CameraFocus previewCameraFocus, float? orthoGraphicSize, bool isSelected)
        {
            if (isSelected && !string.IsNullOrEmpty(lastSelectedSlot))
                avatarSlotsView.DisablePreviousSlot(lastSelectedSlot);

            lastSelectedSlot = isSelected ? slotCategory : "";

            OnToggleSlot?.Invoke(slotCategory, supportColor, previewCameraFocus, orthoGraphicSize, isSelected);
        }

        public void Dispose()
        {
            avatarSlotsView.OnToggleAvatarSlot -= ToggleSlot;
        }

        public void Equip(WearableItem wearableItem, string bodyShape)
        {
            avatarSlotsView.SetSlotContent(wearableItem.data.category, wearableItem, bodyShape);
        }

        public void UnEquip(string category) =>
            avatarSlotsView.ResetCategorySlot(category);

        public void ClearSlotSelection(string category)
        {
            avatarSlotsView.DisablePreviousSlot(category);

            if (lastSelectedSlot == category)
                lastSelectedSlot = "";
        }

        private bool CanAvatarSlotBeUnEquipped(string avatarSlotSection) =>
            avatarSlotSection != WearableLiterals.Categories.BODY_SHAPE &&
            avatarSlotSection != WearableLiterals.Categories.EYES &&
            avatarSlotSection != WearableLiterals.Categories.MOUTH;
    }
}
