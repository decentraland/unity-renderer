using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using System;
using DCL;
using UnityEngine.EventSystems;

public class CatalogItemAdapter : MonoBehaviour, IBeginDragHandler,IEndDragHandler,IDragHandler
{
    public RawImage thumbnailImg;
    public Image favImg;
    public CanvasGroup canvasGroup;
    public Color offFavoriteColor, onFavoriteColor;

    public System.Action<SceneObject> OnSceneObjectClicked;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;
    public System.Action<SceneObject, CatalogItemAdapter,BaseEventData> OnAdapterStartDrag;
    public System.Action<PointerEventData> OnAdapterDrag, OnAdapterEndDrag;

    SceneObject sceneObject;

    string loadedThumbnailURL;
    AssetPromise_Texture loadedThumbnailPromise;


    public SceneObject GetContent()
    {
        return sceneObject;
    }

    public void SetContent(SceneObject sceneObject)
    {
        this.sceneObject = sceneObject;

        if(sceneObject.isFavorite) favImg.color = onFavoriteColor;
        else favImg.color = offFavoriteColor;
        GetThumbnail();
    }

    private void GetThumbnail()
    {
        var url = sceneObject?.ComposeThumbnailUrl();

        if (url == loadedThumbnailURL)
            return;

        if (sceneObject == null || string.IsNullOrEmpty(url))
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
        OnAdapterStartDrag?.Invoke(sceneObject,this, baseEventData);
    }

    public void FavoriteIconClicked()
    {
        
        OnSceneObjectFavorite?.Invoke(sceneObject, this);
    }

    public void SceneObjectClicked()
    {
        OnSceneObjectClicked?.Invoke(sceneObject);
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
