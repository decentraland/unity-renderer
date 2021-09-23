using UnityEngine;
using UnityEngine.UI;

public interface IImageComponentView
{
    /// <summary>
    /// Fill the model and updates the image with this data.
    /// </summary>
    /// <param name="model">Data to configure the image.</param>
    void Configure(ImageComponentModel model);

    /// <summary>
    /// Set an image.
    /// </summary>
    /// <param name="sprite">A sprite.</param>
    void SetImage(Sprite sprite);

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

    public override void Initialize()
    {
        base.Initialize();
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

        SetImage(model.sprite);
    }

    public void SetImage(Sprite sprite)
    {
        model.sprite = sprite;

        if (image == null)
            return;

        image.sprite = sprite;
    }

    public void SetLoadingIndicatorVisible(bool isVisible)
    {
        image.enabled = !isVisible;
        loadingIndicator.SetActive(isVisible);
    }
}