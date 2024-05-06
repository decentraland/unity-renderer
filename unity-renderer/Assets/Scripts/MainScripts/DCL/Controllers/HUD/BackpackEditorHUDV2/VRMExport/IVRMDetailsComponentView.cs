using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IVRMDetailsComponentView
    {
        event Action OnBackButtonPressed;
        event Action<VRMItemModel, UnequipWearableSource> OnWearableUnequipped;
        event Action<VRMItemModel, EquipWearableSource> OnWearableEquipped;
        void FillVRMBlockingWearablesList(List<NFTDataDTO> itemsToDisplay);
    }
}
