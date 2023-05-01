using System;

namespace DCL.Backpack
{
    public interface IAvatarSlotComponentView
    {
        event Action<AvatarSlotComponentModel, bool> OnSelectAvatarSlot;
        event Action<string> OnUnEquip;
        event Action<string> OnFocusHiddenBy;

        void SetIsHidden(bool isHidden, string hiddenBy);
        void SetCategory(string category);
        void SetNftImage(string imageUri);
        void SetRarity(string rarity);
        void SetWearableId(string wearableId);
        void OnPointerClickOnDifferentSlot();
        void ResetSlot();
        void SetHideList(string[] hideList);
        string[] GetHideList();
        void ShakeAnimation();
    }
}
