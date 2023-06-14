using System;
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
    [SerializeField] internal RawImage outfitPreviewImage;
    [SerializeField] private int outfitIndex;

    public event Action<OutfitItem> OnEquipOutfit;
    public event Action<int> OnSaveOutfit;
    public event Action OnDiscardOutfit;

    public override void Awake()
    {
        base.Awake();

        InitializeButtonEvents();
        /*
        SetOutfit(new OutfitItem()
        {
            outfit = new OutfitItem.Outfit()
            {
                wearables = new []{
                    "urn:decentraland:matic:collections-v2:0xd96b73293f278d1d5ee440a2fd859679170dbbdb:0",
                    "urn:decentraland:matic:collections-v2:0x26ea2f6a7273a2f28b410406d1c13ff7d4c9a162:6",
                    "urn:decentraland:matic:collections-v2:0x26ea2f6a7273a2f28b410406d1c13ff7d4c9a162:0"},
                bodyShape = "urn:decentraland:off-chain:base-avatars:BaseFemale",
                eyes = new OutfitItem.ElementColor(){color = new Color(0,0,0)},
                hair = new OutfitItem.ElementColor(){color = new Color(0,0,0)},
                skin = new OutfitItem.ElementColor(){color = new Color(0,0,0)},
            }
        });

        SetIsEmpty(false);*/
    }

    private void InitializeButtonEvents()
    {
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(() => OnEquipOutfit?.Invoke(model.outfitItem));
        saveOutfitButton.onClick.RemoveAllListeners();
        saveOutfitButton.onClick.AddListener(() => OnSaveOutfit?.Invoke(outfitIndex));
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

    public void SetOutfitPreviewImage(Texture bodyTexture)
    {
        outfitPreviewImage.texture = bodyTexture;
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
