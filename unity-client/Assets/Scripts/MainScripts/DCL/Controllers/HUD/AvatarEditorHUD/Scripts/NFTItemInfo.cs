using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
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
        public List<string> iconIds;
        public string description;
        public int issuedId;
        public int issuedTotal;

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
            var iconsIds = wearable.representations.SelectMany(x => x.bodyShapes).ToList();
            iconsIds.Add(wearable.category);

            return new Model()
            {
                name = wearable.GetName(),
                thumbnail = wearable.baseUrl + wearable.thumbnail,
                iconIds = iconsIds,
                description = wearable.description,
                issuedId = wearable.issuedId,
                issuedTotal = wearable.GetIssuedCountFromRarity(wearable.rarity)
            };
        }
    }

    [SerializeField] internal TextMeshProUGUI name;
    [SerializeField] internal Image thumbnail;
    [SerializeField] internal IconToGameObjectMap[] icons;
    [SerializeField] internal TextMeshProUGUI description;
    [SerializeField] internal TextMeshProUGUI minted;

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

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    private void UpdateItemThumbnail(Asset_Texture texture)
    {
        if (thumbnail.sprite != null)
        {
            Destroy(thumbnail.sprite);
        }

        thumbnail.sprite = ThumbnailsManager.CreateSpriteFromTexture(texture.texture);
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

    private void OnEnable()
    {
        GetThumbnail();
    }

    private void OnDisable()
    {
        ForgetThumbnail();
    }
}