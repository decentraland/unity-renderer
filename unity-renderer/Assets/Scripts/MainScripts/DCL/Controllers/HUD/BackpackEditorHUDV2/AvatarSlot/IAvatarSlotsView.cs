using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IAvatarSlotsView
    {
        event Action<string, bool> OnToggleAvatarSlot;
        void CreateAvatarSlotSection(string sectionName, bool addSeparator);
        void RebuildLayout();
        void AddSlotToSection(string sectionName, string slotCategory);
        void DisablePreviousSlot(string category);
        void SetSlotsAsHidden(string[] slotsToHide, string hiddenBy);
    }
}
