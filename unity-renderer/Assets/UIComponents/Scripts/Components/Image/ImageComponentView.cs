using DCL.Helpers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface IImageComponentView
{
    /// <summary>
    /// It will be triggered when the sprite has been loaded.
    /// </summary>
    event Action<Sprite> OnLoaded;

    /// <summary>
    /// Set an image directly from a sprite.
    /// </summary>
    /// <param name="sprite">A sprite.</param>
    void SetImage(Sprite sprite);

    /// <summary>
    /// Set an image from a 2D texture,
    /// </summary>
    /// <param name="texture">2D texture.</param>
    void SetImage(Texture2D texture);

    /// <summary>
    /// Set an image from an uri.
    /// </summary>
    /// <param name="uri">Url of the image.</param>
    void SetImage(string uri);

    /// <summary>
    /// Resize the image size to fit into the parent.
    /// </summary>
    /// <param name="fitParent">True to fit the size.</param>
    void SetFitParent(bool fitParent);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator and hiding the image.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class ImageComponentView : BaseComponentView, IImageComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal Image image;
    [SerializeField] internal GameObject loadingIndicator;

    [Header("Configuration")]
    [SerializeField] internal ImageComponentModel model;

    public event Action<Sprite> OnLoaded;

    internal Sprite currentSprite;
    internal ILazyTextureObserver imageObserver = new LazyTextureObserver();
    internal Vector2 lastParentSize;

    public override void Start() { imageObserver.AddListener(OnImageObserverUpdated); }

    private void LateUpdate()
    {
        if (model.fitParent && HasParentSizeChanged())
            SetFitParent(model.fitParent);
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (ImageComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        if (model.sprite != null)
            SetImage(model.sprite);
        else if (model.texture != null)
            SetImage(model.texture);
        else if (!string.IsNullOrEmpty(model.uri))
            SetImage(model.uri);
        else
            SetImage(sprite: null);
    }

    public override void Dispose()
    {
        base.Dispose();

        imageObserver.RemoveListener(OnImageObserverUpdated);
        Destroy(currentSprite);
    }

    public void SetImage(Sprite sprite)
    {
        model.sprite = sprite;

        if (image == null)
            return;

        image.sprite = sprite;
        SetFitParent(model.fitParent);
    }

    public void SetImage(Texture2D texture)
    {
        if (model.texture != texture)
        {
            model.texture = texture;

            if (!Application.isPlaying)
            {
                OnImageObserverUpdated(texture);
                return;
            }

            SetLoadingIndicatorVisible(true);
            imageObserver.RefreshWithTexture(texture);
        }

        SetFitParent(model.fitParent);
    }

    public void SetImage(string uri)
    {
        model.uri = uri;

        if (!Application.isPlaying)
            return;

        SetLoadingIndicatorVisible(true);
        if (!string.IsNullOrEmpty(uri))
            imageObserver.RefreshWithUri(uri);
        else
            OnImageObserverUpdated(null);
    }

    public void SetFitParent(bool fitParent)
    {
        model.fitParent = fitParent;

        if (fitParent)
            ResizeFillParent();
    }

    public void SetLoadingIndicatorVisible(bool isVisible)
    {
        image.enabled = !isVisible;
        loadingIndicator.SetActive(isVisible);
    }

    internal void OnImageObserverUpdated(Texture texture)
    {
        if (Application.isPlaying)
            Destroy(currentSprite);
        else
            DestroyImmediate(currentSprite);

        currentSprite = texture != null ? Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)) : null;
        SetImage(currentSprite);
        SetLoadingIndicatorVisible(false);
        OnLoaded?.Invoke(currentSprite);
    }

    internal void ResizeFillParent()
    {
        RectTransform imageRectTransform = (RectTransform)image.transform;

        imageRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        imageRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        imageRectTransform.pivot = new Vector2(0.5f, 0.5f);
        imageRectTransform.localPosition = Vector2.zero;

        if (transform.parent == null)
            return;

        RectTransform parent = transform.parent as RectTransform;

        float h, w;
        h = parent.rect.height;
        w = h * (image.mainTexture != null ? (image.mainTexture.width / (float)image.mainTexture.height) : 1);

        if ((parent.rect.width - w) > 0)
        {
            w = parent.rect.width;
            h = w * (image.mainTexture != null ? (image.mainTexture.height / (float)image.mainTexture.width) : 1);
        }

        imageRectTransform.sizeDelta = new Vector2(w, h);
    }

    internal bool HasParentSizeChanged()
    {
        Transform imageParent = transform.parent;
        if (imageParent != null)
        {
            Vector2 currentParentSize = ((RectTransform)imageParent).rect.size;

            if (lastParentSize != currentParentSize)
            {
                lastParentSize = currentParentSize;
                return true;
            }
        }

        return false;
    }
}