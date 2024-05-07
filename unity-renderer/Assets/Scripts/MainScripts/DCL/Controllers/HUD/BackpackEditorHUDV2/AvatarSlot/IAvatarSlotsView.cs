using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IAvatarSlotsView
    {
        delegate void ToggleAvatarSlotDelegate(string slotCategory, bool supportColor, PreviewCameraFocus previewCameraFocus, bool isSelected);
        event ToggleAvatarSlotDelegate OnToggleAvatarSlot;
        public event Action<string, bool> OnHideUnhidePressed;
        event Action<string> OnUnequipFromSlot;

        void CreateAvatarSlotSection(string sectionName, bool addSeparator);
        void RebuildLayout();
        void AddSlotToSection(string sectionName, string slotCategory, bool allowUnEquip);
        void DisablePreviousSlot(string category);
        void SetSlotContent(string category, WearableItem wearableItem, string bodyShape, HashSet<string> forceRender);
        void ResetCategorySlot(string category, HashSet<string> forceRender);
        void RecalculateHideList(HashSet<string> forceRender);
        void SetHideUnhideStatus(string slotCategory, bool isOverridden);
        void Select(string category, bool notify);
        void UnSelectAll(bool notify);
    }
}
