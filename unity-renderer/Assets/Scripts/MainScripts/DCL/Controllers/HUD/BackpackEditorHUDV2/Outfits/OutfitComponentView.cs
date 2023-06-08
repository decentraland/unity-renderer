using System;
using System.Collections;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class OutfitComponentView : BaseComponentView<OutfitComponentModel>, IOutfitComponentView
{
    [SerializeField] internal GameObject emptyState;
    [SerializeField] internal GameObject filledState;
    [SerializeField] internal GameObject[] hoverStates;
    [SerializeField] internal GameObject[] normalStates;
    [SerializeField] internal Button equipButton;
    [SerializeField] internal Button saveOutfitButton;
    [SerializeField] internal Button discardOutfitButton;

    public event Action<OutfitItem> OnEquipOutfit;
    public event Action OnSaveOutfit;
    public event Action OnDiscardOutfit;

    public override void Awake()
    {
        base.Awake();

        InitializeButtonEvents();
    }

    private void InitializeButtonEvents()
    {
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => OnEquipOutfit?.Invoke(model.outfitItem));
        saveOutfitButton.onClick.RemoveAllListeners();
        saveOutfitButton.onClick.AddListener(() => OnSaveOutfit?.Invoke());
        discardOutfitButton.onClick.RemoveAllListeners();
        discardOutfitButton.onClick.AddListener(() => OnDiscardOutfit?.Invoke());
    }

    public override void RefreshControl()
    {
        SetOutfit(model.outfitItem);
    }

    public void SetOutfit(OutfitItem outfitItem)
    {
        model.outfitItem = outfitItem;
    }

    public void SetIsEmpty(bool isEmpty)
    {
        emptyState.SetActive(isEmpty);
        filledState.SetActive(!isEmpty);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        foreach (GameObject hoverState in hoverStates)
            hoverState.SetActive(true);
        foreach (GameObject normalState in normalStates)
            normalState.SetActive(false);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        foreach (GameObject normalState in normalStates)
            normalState.SetActive(true);
        foreach (GameObject hoverState in hoverStates)
            hoverState.SetActive(false);
    }
}
