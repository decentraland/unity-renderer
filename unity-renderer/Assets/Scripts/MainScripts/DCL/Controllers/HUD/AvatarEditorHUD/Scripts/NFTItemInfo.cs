using DCL;
using DCL.EmotesCustomization;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NFTItemInfo : MonoBehaviour
{
    [Serializable]
    internal class IconToGameObjectMap
    {
        public string iconId;
        public GameObject gameObject;
    }

    public class Model
    {
        public string name;
        public string thumbnail;
        public Sprite thumbnailSprite;
        public List<string> iconIds;
        public string description;
        public int issuedId;
        public int issuedTotal;
        public bool isInL2;

        public bool Equals(Model other)
        {
            return name == other.name
                   && thumbnail == other.thumbnail
                   && thumbnailSprite == other.thumbnailSprite
                   && iconIds.SequenceEqual(other.iconIds)
                   && description == other.description
                   && issuedId == other.issuedId
                   && issuedTotal == other.issuedTotal;
        }

        public static Model FromWearableItem(WearableItem wearable)
        {
            var iconsIds = wearable.data.representations.SelectMany(x => x.bodyShapes).ToList();
            iconsIds.Add(wearable.data.category);

            return new Model()
            {
                name = wearable.GetName(),
                thumbnail = wearable.baseUrl + wearable.thumbnail,
                thumbnailSprite = wearable.thumbnailSprite,
                iconIds = iconsIds,
                description = wearable.description,
                issuedId = wearable.issuedId,
                issuedTotal = wearable.GetIssuedCountFromRarity(wearable.rarity),
                isInL2 = wearable.IsInL2()
            };
        }

        public static Model FromEmoteItem(EmoteCardComponentModel emote)
        {
            return new Model
            {
                name = emote.name,
                thumbnail = emote.pictureUri,
                thumbnailSprite = emote.pictureSprite,
                iconIds = new List<string>(),
                description = emote.description,
                issuedId = 1,
                issuedTotal = int.MaxValue,
                isInL2 = emote.isInL2
            };
        }
    }

    [SerializeField] internal TextMeshProUGUI name;
    [SerializeField] internal Image thumbnail;
    [SerializeField] internal IconToGameObjectMap[] icons;
    [SerializeField] internal TextMeshProUGUI description;
    [SerializeField] internal TextMeshProUGUI minted;
    [SerializeField] internal GameObject ethNetwork;
    [SerializeField] internal GameObject l2Network;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal Image gradientImage;
    [SerializeField] internal TextMeshProUGUI rarityName;
    [SerializeField] internal Button sellButton;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal GameObject categoryInfoContainer;

    private Model currentModel;

    public Button.ButtonClickedEvent OnCloseButtonClick => closeButton?.onClick;

    public void SetSkin(string rarityName, NFTItemToggleSkin skin)
    {
        this.rarityName.text = rarityName;
        this.rarityName.color = skin.rarityNameColor;
        backgroundImage.color = skin.backgroundColor;
        gradientImage.color = skin.gradientColor;
        
    }
    
    public void SetModel(Model newModel)
    {
        if (newModel == null)
            return;

        if (currentModel != null && newModel.Equals(currentModel))
            return;

        currentModel = newModel;

        name.text = currentModel.name;

        foreach (var icon in icons)
        {
            icon.gameObject.SetActive(currentModel.iconIds.Contains(icon.iconId));
        }

        if (!string.IsNullOrEmpty(currentModel.description))
            description.text = currentModel.description;
        else
            description.text = "No description.";

        minted.text = $"{currentModel.issuedId} / {currentModel.issuedTotal}";

        Utils.InverseTransformChildTraversal<LayoutGroup>((x) =>
        {
            RectTransform rt = x.transform as RectTransform;
            Utils.ForceRebuildLayoutImmediate(rt);
        }, transform);


        ethNetwork.SetActive(!currentModel.isInL2);
        l2Network.SetActive(currentModel.isInL2);

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    public void SetActive(bool active) { gameObject.SetActive(active); }

    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage == null)
            return;

        backgroundImage.color = color;
    }

    public void SetRarityName(string name)
    {
        if (rarityName == null)
            return;

        rarityName.text = name;
    }

    public void SetSellButtonActive(bool isActive) => sellButton.gameObject.SetActive(isActive);

    public void SetCategoryInfoActive(bool isActive) => categoryInfoContainer.SetActive(isActive);

    private void UpdateItemThumbnail(Asset_Texture texture)
    {
        thumbnail.sprite = ThumbnailsManager.GetOrCreateSpriteFromTexture(texture.texture, out _);
        thumbnail.preserveAspect = true;
    }

    private void GetThumbnail()
    {
        if (currentModel == null)
            return;

        if (currentModel.thumbnailSprite != null)
        {
            thumbnail.sprite = currentModel.thumbnailSprite;
            return;
        }
        
        ThumbnailsManager.GetThumbnail(currentModel.thumbnail, UpdateItemThumbnail);
    }

    private void OnEnable() { GetThumbnail(); }
}
