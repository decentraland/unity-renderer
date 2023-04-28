using DCL.Quest;
using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardComponentView : BaseComponentView<QuestRewardComponentModel>, IQuestRewardComponentView
{
    [SerializeField] internal TMP_Text rewardName;
    [SerializeField] internal TMP_Text rewardQuantity;
    [SerializeField] internal Image typeImage;
    [SerializeField] internal ImageComponentView nftImage;
    [SerializeField] internal NFTTypeIconsAndColors nftTypesIcons;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image rarityBackgroundImage;

    public override void RefreshControl()
    {
        SetName(model.name);
        SetQuantity(model.quantity);
        SetType(model.type);
        SetRarity(model.rarity);
        SetImage(model.imageUri);
    }

    public void SetName(string name)
    {
        model.name = name;
        rewardName.text = name;
    }

    public void SetQuantity(int quantity)
    {
        model.quantity = quantity;
        rewardQuantity.text = quantity <= 999 ? $"{quantity} remaining" : ">999 remaining";
    }

    public void SetType(string type)
    {
        model.type = type;
        typeImage.sprite = nftTypesIcons.GetTypeImage(type);
    }

    public void SetRarity(string rarity)
    {
        model.rarity = rarity;
        Color rarityColor = nftTypesIcons.GetColor(rarity);
        backgroundImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 1f);
        rarityBackgroundImage.color = rarityColor;
    }

    public void SetImage(string imageUri)
    {
        model.imageUri = imageUri;
        nftImage.SetImage(imageUri);
    }
}
