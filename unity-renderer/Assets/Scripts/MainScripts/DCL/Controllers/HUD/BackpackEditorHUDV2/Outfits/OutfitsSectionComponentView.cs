using System;
using System.Collections;
using System.Collections.Generic;
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

    private void OnEquipOutfit()
    {
        throw new NotImplementedException();
    }

    public override void RefreshControl() { }
}
