using System;

public interface IOutfitComponentView
{
    event Action<OutfitItem> OnEquipOutfit;
    event Action OnSaveOutfit;
    event Action OnDiscardOutfit;

    void SetOutfit(OutfitItem outfitItem);
    void SetIsEmpty(bool isEmpty);
}
