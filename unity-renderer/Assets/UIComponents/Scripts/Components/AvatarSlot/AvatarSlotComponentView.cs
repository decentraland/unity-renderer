using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarSlotComponentView : BaseComponentView, IPointerClickHandler, IAvatarSlotComponentView, IComponentModelConfig<AvatarSlotComponentModel>
{
    [Header("Configuration")]
    [SerializeField] internal AvatarSlotComponentModel model;

    [SerializeField] internal NftTypeIconSO typeIcons;
    [SerializeField] internal NftRarityBackgroundSO rarityBackgrounds;
    [SerializeField] internal Image typeImage;
    [SerializeField] internal ImageComponentView nftImage;
    [SerializeField] internal Image backgroundRarityImage;
    [SerializeField] internal Image focusedImage;
    [SerializeField] internal Image selectedImage;
    [SerializeField] private GameObject emptySlot;
    [SerializeField] private GameObject hiddenSlot;
    [SerializeField] private GameObject tooltipContainer;
    [SerializeField] private TMP_Text tooltipText;

    public event Action<string> OnSelectAvatarSlot;

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetCategory(model.category);
        SetNftImage(model.imageUri);
        SetRarity(model.rarity);
    }

    public void Configure(AvatarSlotComponentModel newModel)
    {
        if (model == newModel)
            return;

        model = newModel;
        RefreshControl();
    }

    public void SetCategory(string category)
    {
        model.category = category;
        typeImage.sprite = typeIcons.GetTypeImage(category);
        tooltipText.text = category;
    }

    public void SetNftImage(string imageUri)
    {
        model.imageUri = imageUri;

        if (string.IsNullOrEmpty(imageUri))
        {
            nftImage.SetImage(Texture2D.grayTexture);
            emptySlot.SetActive(true);
            return;
        }

        emptySlot.SetActive(false);
        nftImage.SetImage(imageUri);
    }

    public void SetRarity(string rarity)
    {
        model.rarity = rarity;
        backgroundRarityImage.sprite = rarityBackgrounds.GetRarityImage(rarity);
    }

    public override void OnFocus()
    {
        focusedImage.enabled = true;
        tooltipContainer.SetActive(true);
    }

    public override void OnLoseFocus()
    {
        focusedImage.enabled = false;
        tooltipContainer.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSelectAvatarSlot?.Invoke(model.category);
        selectedImage.enabled = true;
    }
}
