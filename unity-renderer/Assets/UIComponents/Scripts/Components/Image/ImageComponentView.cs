using DCL.Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    void SetImage(Sprite sprite, bool cleanLastLoadedUri = true);

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
    /// Indicates if we want to cache the last uri request.
    /// </summary>
    /// <param name="isEnabled">True for caching the last uri request.</param>
    void SetLastUriRequestCached(bool isEnabled);

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

public class ImageComponentView : BaseComponentView, IImageComponentView, IComponentModelConfig<ImageComponentModel>
{
    private readonly Vector2 vector2oneHalf = new (0.5f, 0.5f);

    [Header("Prefab References")]
    [SerializeField] internal Image image;
    [SerializeField] internal GameObject loadingIndicator;

    [Header("Configuration")]
    [SerializeField] internal ImageComponentModel model;

    public event Action<Sprite> OnLoaded;

    internal Sprite currentSprite;
    internal ILazyTextureObserver imageObserver = new LazyTextureObserver();
    internal Vector2 lastParentSize;
    internal string currentUriLoading = null;
    internal string lastLoadedUri = null;

    public Image ImageComponent => image;

    public void Start()
    {
        image.useSpriteMesh = false;
        imageObserver.AddListener(OnImageObserverUpdated);
    }

    private void LateUpdate()
    {
        if (model.fitParent && HasParentSizeChanged())
            SetFitParent(model.fitParent);
    }

    public virtual void Configure(ImageComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetLastUriRequestCached(model.lastUriCached);
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
        currentUriLoading = null;
        lastLoadedUri = null;
        imageObserver.RemoveListener(OnImageObserverUpdated);

        DestroyInterntally(currentSprite);

        base.Dispose();
    }

    private static void DestroyInterntally(Object obj)
    {
#if UNITY_EDITOR
        DestroyImmediate(obj);
#else
        Destroy(obj);
#endif
    }

    public void SetImage(Sprite sprite, bool cleanLastLoadedUri = true)
    {
        model.sprite = sprite;

        if (image == null)
            return;

        image.sprite = sprite;

        if (cleanLastLoadedUri)
            lastLoadedUri = null;

        SetFitParent(model.fitParent);
    }

    public void SetImage(Texture2D texture)
    {
        model.texture = texture;

        if (!Application.isPlaying)
        {
            OnImageObserverUpdated(texture);
            return;
        }

        SetLoadingIndicatorVisible(true);
        imageObserver.RefreshWithTexture(texture);

        lastLoadedUri = null;
        SetFitParent(model.fitParent);
    }

    public virtual void SetImage(string uri)
    {
        if (model.lastUriCached && uri == lastLoadedUri)
            return;

        model.uri = uri;

        if (!Application.isPlaying)
            return;

        SetLoadingIndicatorVisible(true);
        if (!string.IsNullOrEmpty(uri))
        {
            currentUriLoading = uri;
            imageObserver.RefreshWithUri(uri);
        }
        else
        {
            lastLoadedUri = null;
            OnImageObserverUpdated(null);
        }
    }

    public void SetLastUriRequestCached(bool isEnabled) { model.lastUriCached = isEnabled; }

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

    internal void OnImageObserverUpdated(Texture2D texture)
    {
        DestroyInterntally(currentSprite);

        currentSprite = texture != null ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), vector2oneHalf, 100, 0, SpriteMeshType.FullRect, Vector4.one, false) : null;

        SetImage(currentSprite, false);
        SetLoadingIndicatorVisible(false);
        lastLoadedUri = currentUriLoading;
        currentUriLoading = null;
        OnLoaded?.Invoke(currentSprite);
    }

    internal void ResizeFillParent()
    {
        RectTransform imageRectTransform = (RectTransform)image.transform;

        imageRectTransform.anchorMin = vector2oneHalf;
        imageRectTransform.anchorMax = vector2oneHalf;
        imageRectTransform.pivot = vector2oneHalf;
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
