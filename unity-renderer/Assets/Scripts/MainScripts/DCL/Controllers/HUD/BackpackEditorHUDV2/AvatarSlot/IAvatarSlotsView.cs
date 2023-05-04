using System;

namespace DCL.Backpack
{
    public interface IAvatarSlotsView
    {
        delegate void ToggleAvatarSlotDelegate(string slotCategory, bool supportColor, bool isSelected);
        event ToggleAvatarSlotDelegate OnToggleAvatarSlot;

        event Action<string> OnUnequipFromSlot;

        void CreateAvatarSlotSection(string sectionName, bool addSeparator);
        void RebuildLayout();
        void AddSlotToSection(string sectionName, string slotCategory);
        void DisablePreviousSlot(string category);
        void SetSlotContent(string category, WearableItem wearableItem, string bodyShape);
        void ResetCategorySlot(string category);
    }
}
