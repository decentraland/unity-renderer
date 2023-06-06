using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutfitComponentView : BaseComponentView, IOutfitComponentView
{
    [SerializeField] internal GameObject emptyState;
    [SerializeField] internal GameObject filledState;
    [SerializeField] internal GameObject[] hoverStates;
    [SerializeField] internal GameObject[] normalStates;
    [SerializeField] internal Button equipButton;
    [SerializeField] internal Button saveOutfitButton;
    [SerializeField] internal Button discardOutfitButton;

    public event Action OnEquipOutfit;
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
        equipButton.onClick.AddListener(() => OnEquipOutfit?.Invoke());
        saveOutfitButton.onClick.RemoveAllListeners();
        saveOutfitButton.onClick.AddListener(() => OnSaveOutfit?.Invoke());
        discardOutfitButton.onClick.RemoveAllListeners();
        discardOutfitButton.onClick.AddListener(() => OnDiscardOutfit?.Invoke());
    }

    public override void RefreshControl()
    {
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
