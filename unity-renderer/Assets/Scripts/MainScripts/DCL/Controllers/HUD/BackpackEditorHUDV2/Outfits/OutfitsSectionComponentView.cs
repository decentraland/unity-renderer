using System;
using UnityEngine;
using UnityEngine.UI;

public class OutfitsSectionComponentView : BaseComponentView
{
    [SerializeField] internal Button backButton;
    [SerializeField] internal OutfitComponentView[] outfitComponentViews;

    public event Action OnBackButtonPressed;

    public override void Awake()
    {
        base.Awake();

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(()=>OnBackButtonPressed?.Invoke());
        foreach (OutfitComponentView outfitComponentView in outfitComponentViews)
        {
            outfitComponentView.OnEquipOutfit += OnEquipOutfit;
        }
    }

    public void ShowOutfits(OutfitItem[] outfits)
    {
        for (int i = 0; i < outfitComponentViews.Length; i++)
        {
            if (i < outfits.Length)
            {
                outfitComponentViews[i].SetIsEmpty(false);
                outfitComponentViews[i].SetOutfit(outfits[i]);
            }
            else
            {
                outfitComponentViews[i].SetIsEmpty(true);
            }
        }
    }

    private void OnEquipOutfit(OutfitItem outfitItem)
    {
    }

    public override void RefreshControl() { }
}
