using System;

public interface IOutfitComponentView
{
    event Action<OutfitItem> OnEquipOutfit;
    event Action<int> OnSaveOutfit;
    event Action<int> OnDiscardOutfit;

    void SetOutfit(OutfitItem outfitItem);
    void SetIsEmpty(bool isEmpty);
}
