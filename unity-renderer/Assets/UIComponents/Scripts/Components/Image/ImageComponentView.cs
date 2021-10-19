using DCL.Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;

public interface IImageComponentView
{
    /// <summary>
    /// It will be triggered when the sprite has been loaded.
    /// </summary>
    event Action<Sprite> OnLoaded;

    /// <summary>
    /// Fill the model and updates the image with this data.
    /// </summary>
    /// <param name="model">Data to configure the image.</param>
    void Configure(ImageComponentModel model);

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
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator and hiding the image.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class ImageComponentView : BaseComponentView, IImageComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Image image;
    [SerializeField] internal GameObject loadingIndicator;

    [Header("Configuration")]
    [SerializeField] internal ImageComponentModel model;

    public event Action<Sprite> OnLoaded;

    internal Sprite currentSprite;
    internal ILazyTextureObserver imageObserver = new LazyTextureObserver();

    public override void PostInitialization()
    {
        imageObserver.AddListener(OnImageObserverUpdated);
        Configure(model);
    }

    public void Configure(ImageComponentModel model)
    {
        this.model = model;
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
    }

    public void SetImage(Texture2D texture)
    {
        model.texture = texture;

        if (!Application.isPlaying)
        {
            OnImageObserverUpdated(texture);
            return;
        }

        imageObserver.RefreshWithTexture(texture);
        SetLoadingIndicatorVisible(true);
    }

    public void SetImage(string uri)
    {
        model.uri = uri;

        if (!Application.isPlaying)
            return;

        imageObserver.RefreshWithUri(uri);
        SetLoadingIndicatorVisible(true);
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

        currentSprite = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        SetImage(currentSprite);
        SetLoadingIndicatorVisible(false);
        OnLoaded?.Invoke(currentSprite);
    }
}