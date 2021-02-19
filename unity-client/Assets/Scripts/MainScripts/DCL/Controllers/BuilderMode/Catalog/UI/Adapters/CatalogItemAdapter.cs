using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using System;
using DCL;
using UnityEngine.EventSystems;
using DCL.Configuration;

public class CatalogItemAdapter : MonoBehaviour, IBeginDragHandler,IEndDragHandler,IDragHandler
{
    public RawImage thumbnailImg;
    public Image favImg;
    public GameObject smartItemGO;
    public CanvasGroup canvasGroup;
    public Color offFavoriteColor, onFavoriteColor;
    public GameObject lockedGO;

    public System.Action<CatalogItem> OnCatalogItemClicked;
    public System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;
    public System.Action<CatalogItem, CatalogItemAdapter,BaseEventData> OnAdapterStartDrag;
    public System.Action<PointerEventData> OnAdapterDrag, OnAdapterEndDrag;

    CatalogItem catalogItem;

    string loadedThumbnailURL;
    AssetPromise_Texture loadedThumbnailPromise;


    public CatalogItem GetContent()
    {
        return catalogItem;
    }

    public void SetContent(CatalogItem catalogItem)
    {
        this.catalogItem = catalogItem;

        if(catalogItem.IsFavorite())
            favImg.color = onFavoriteColor;
        else
            favImg.color = offFavoriteColor;

        smartItemGO.SetActive(catalogItem.IsSmartItem());

        GetThumbnail();

        lockedGO.gameObject.SetActive(false);

        if (catalogItem.IsNFT() && BuilderInWorldNFTController.i.IsNFTInUse(catalogItem.id))
            lockedGO.gameObject.SetActive(true);
    }

    private void GetThumbnail()
    {
        var url = catalogItem?.GetThumbnailUrl();

        if (url == loadedThumbnailURL)
            return;

        if (catalogItem == null || string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise =  new AssetPromise_Texture(url);


        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);


        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
        loadedThumbnailURL = newLoadedThumbnailURL;
    }

    public void EnableDragMode(Vector2 sizeDelta)
    {
        RectTransform newAdapterRT = GetComponent<RectTransform>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        newAdapterRT.sizeDelta = sizeDelta* 0.75f;
    }
    public void SetFavorite(bool isOn)
    {
        if (isOn)
            favImg.color = onFavoriteColor;
        else
            favImg.color = offFavoriteColor;
    }

    public void AdapterStartDragging(BaseEventData baseEventData)
    {
        OnAdapterStartDrag?.Invoke(catalogItem,this, baseEventData);
    }

    public void FavoriteIconClicked()
    {
        
        OnCatalogItemFavorite?.Invoke(catalogItem, this);
    }

    public void SceneObjectClicked()
    {
       if (!lockedGO.gameObject.activeSelf)
            OnCatalogItemClicked?.Invoke(catalogItem);
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (thumbnailImg != null)
        {
            thumbnailImg.enabled = true;
            thumbnailImg.texture = texture.texture;
            favImg.gameObject.SetActive(true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        AdapterStartDragging(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnAdapterDrag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        OnAdapterEndDrag?.Invoke(eventData);
    }
}
