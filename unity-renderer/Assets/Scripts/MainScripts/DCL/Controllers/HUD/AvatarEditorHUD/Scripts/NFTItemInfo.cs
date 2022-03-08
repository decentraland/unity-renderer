using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EmotesCustomization;

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
        public List<string> iconIds;
        public string description;
        public int issuedId;
        public int issuedTotal;
        public bool isInL2;

        public bool Equals(Model other)
        {
            return name == other.name
                   && thumbnail == other.thumbnail
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
    [SerializeField] internal TextMeshProUGUI rarityName;
    [SerializeField] internal Button sellButton;
    [SerializeField] internal Button closeButton;

    private Model currentModel;
    private AssetPromise_Texture thumbnailPromise;

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

    private void UpdateItemThumbnail(Asset_Texture texture)
    {
        if (thumbnail.sprite != null)
        {
            Destroy(thumbnail.sprite);
        }

        thumbnail.sprite = ThumbnailsManager.CreateSpriteFromTexture(texture.texture);
        thumbnail.preserveAspect = true;
    }

    private void GetThumbnail()
    {
        if (currentModel == null)
            return;

        //NOTE(Brian): Get before forget to prevent referenceCount == 0 and asset unload
        var newThumbnailPromise = ThumbnailsManager.GetThumbnail(currentModel.thumbnail, UpdateItemThumbnail);
        ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
        thumbnailPromise = newThumbnailPromise;
    }

    private void ForgetThumbnail()
    {
        ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
        thumbnailPromise = null;
    }

    private void OnEnable() { GetThumbnail(); }

    private void OnDisable() { ForgetThumbnail(); }
}