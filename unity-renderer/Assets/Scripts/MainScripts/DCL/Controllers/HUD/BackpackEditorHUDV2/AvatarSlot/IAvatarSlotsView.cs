using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IAvatarSlotsView
    {
        event Action<string, bool> OnToggleAvatarSlot;
        event Action<string> OnUnequipFromSlot;

        void CreateAvatarSlotSection(string sectionName, bool addSeparator);
        void RebuildLayout();
        void AddSlotToSection(string sectionName, string slotCategory);
        void DisablePreviousSlot(string category);
        void SetSlotContent(string category, WearableItem wearableItem, string bodyShape);
        void ResetCategorySlot(string category);
    }
}
