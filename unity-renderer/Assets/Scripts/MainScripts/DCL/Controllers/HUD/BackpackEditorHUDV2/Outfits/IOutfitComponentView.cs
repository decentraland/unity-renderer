using System;

public interface IOutfitComponentView
{
    event Action OnEquipOutfit;
    event Action OnSaveOutfit;
    event Action OnDiscardOutfit;

    void SetIsEmpty(bool isEmpty);
}
