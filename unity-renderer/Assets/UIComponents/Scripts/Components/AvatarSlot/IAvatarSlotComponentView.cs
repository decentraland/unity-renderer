using System;

public interface IAvatarSlotComponentView
{
    event Action<string> OnSelectAvatarSlot;

    void SetCategory(string category);
    void SetNftImage(string imageUri);
    void SetRarity(string rarity);
}
