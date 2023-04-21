using System;

namespace DCL.Backpack
{
    public interface IAvatarSlotComponentView
    {
        event Action<string, bool> OnSelectAvatarSlot;

        void SetIsHidden(bool isHidden, string hiddenBy);

        void SetCategory(string category);

        void SetNftImage(string imageUri);

        void SetRarity(string rarity);

        void OnPointerClickOnDifferentSlot();
    }
}
