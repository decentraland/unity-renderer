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
        void SetSlotNftImage(string category, string imageUrl);
        void SetWearableId(string category, string wearableId);
        void SetSlotRarity(string category, string rarity);
        void DisablePreviousSlot(string category);
        void SetSlotsAsHidden(string[] slotsToHide, string hiddenBy);
        void ResetCategorySlot(string category);
    }
}
