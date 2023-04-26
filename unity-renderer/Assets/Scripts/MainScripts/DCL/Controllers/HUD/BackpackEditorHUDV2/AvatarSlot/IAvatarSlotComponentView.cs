using System;

namespace DCL.Backpack
{
    public interface IAvatarSlotComponentView
    {
        event Action<string, bool> OnSelectAvatarSlot;
        event Action<string, string> OnUnEquip;

        void SetIsHidden(bool isHidden, string hiddenBy);
        void SetCategory(string category);
        void SetNftImage(string imageUri);
        void SetRarity(string rarity);
        void SetWearableId(string wearableId);
        void OnPointerClickOnDifferentSlot();
        void ResetSlot();
    }
}
