using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IExperienceRowComponentView
{
    /// <summary>
    /// Event that will be triggered when the Show/Hide PEX UI button is clicked.
    /// </summary>
    event Action<string, bool> onShowPEXUI;

    /// <summary>
    /// Event that will be triggered when the Start(Stop PEX button is clicked.
    /// </summary>
    event Action<string, bool> onStartPEX;

    /// <summary>
    /// Set the PEX id.
    /// </summary>
    /// <param name="id">A string</param>
    void SetId(string id);

    /// <summary>
    /// Set an icon image from an uri.
    /// </summary>
    /// <param name="uri">Url of the icon image.</param>
    void SetIcon(string uri);

    /// <summary>
    /// Set the name label.
    /// </summary>
    /// <param name="name">A string.</param>
    void SetName(string name);

    /// <summary>
    /// Set the PEX UI as visible or not.
    /// </summary>
    /// <param name="isPlaying">True for set the PEX UI as visible.</param>
    void SetUIVisibility(bool isVisible);

    /// <summary>
    /// Set the PEX as playing or not.
    /// </summary>
    /// <param name="isPlaying">True for set it as playing.</param>
    void SetAsPlaying(bool isPlaying);

    /// <summary>
    /// Set the background color of the row.
    /// </summary>
    /// <param name="color">Color to apply.</param>
    void SetRowColor(Color color);

    /// <summary>
    /// Set the background color of the row when it is hovered.
    /// </summary>
    /// <param name="color">Color to apply.</param>
    void SetOnHoverColor(Color color);
}

public class ExperienceRowComponentView : BaseComponentView, IExperienceRowComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView iconImage;
    [SerializeField] internal TMP_Text nameText;
    [SerializeField] internal GameObject showHideUIButtonsContainer;
    [SerializeField] internal ButtonComponentView showPEXUIButton;
    [SerializeField] internal ButtonComponentView hidePEXUIButton;
    [SerializeField] internal ButtonComponentView startPEXButton;
    [SerializeField] internal ButtonComponentView stopPEXButton;
    [SerializeField] internal Image backgroundImage;

    [Header("Configuration")]
    [SerializeField] internal ExperienceRowComponentModel model;

    public event Action<string, bool> onShowPEXUI;
    public event Action<string, bool> onStartPEX;

    internal Color originalBackgroundColor;
    internal Color onHoverColor;

    public override void Awake()
    {
        base.Awake();

        originalBackgroundColor = backgroundImage.color;
        ConfigureRowButtons();
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (ExperienceRowComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetId(model.id);
        SetIcon(model.iconUri);
        SetName(model.name);
        SetUIVisibility(model.isUIVisible);
        SetAsPlaying(model.isPlaying);
        SetRowColor(model.backgroundColor);
        SetOnHoverColor(model.onHoverColor);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        backgroundImage.color = onHoverColor;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        backgroundImage.color = originalBackgroundColor;
    }

    public void SetId(string id)
    {
        model.id = id;
    }

    public void SetIcon(string uri)
    {
        model.iconUri = uri;

        if (iconImage == null)
            return;

        iconImage.SetImage(uri);
    }

    public void SetName(string name)
    {
        model.name = name;

        if (nameText == null)
            return;

        nameText.text = name;
    }

    public void SetUIVisibility(bool isVisible)
    {
        model.isUIVisible = isVisible;

        if (showPEXUIButton != null)
            showPEXUIButton.gameObject.SetActive(!isVisible);

        if (hidePEXUIButton != null)
            hidePEXUIButton.gameObject.SetActive(isVisible);
    }

    public void SetAsPlaying(bool isPlaying)
    {
        model.isPlaying = isPlaying;

        if (startPEXButton != null)
            startPEXButton.gameObject.SetActive(!isPlaying);

        if (stopPEXButton != null)
            stopPEXButton.gameObject.SetActive(isPlaying);

        if (showHideUIButtonsContainer != null)
            showHideUIButtonsContainer.SetActive(isPlaying);
    }

    public void SetRowColor(Color color)
    {
        model.backgroundColor = color;

        if (backgroundImage == null)
            return;

        backgroundImage.color = color;
        originalBackgroundColor = color;
    }

    public void SetOnHoverColor(Color color)
    {
        model.onHoverColor = color;
        onHoverColor = color;
    }

    public override void Dispose()
    {
        base.Dispose();

        showPEXUIButton?.onClick.RemoveAllListeners();
        hidePEXUIButton?.onClick.RemoveAllListeners();
        startPEXButton?.onClick.RemoveAllListeners();
        stopPEXButton?.onClick.RemoveAllListeners();
    }

    internal void ConfigureRowButtons()
    {
        showPEXUIButton?.onClick.AddListener(() =>
        {
            SetUIVisibility(true);
            onShowPEXUI?.Invoke(model.id, true);
        });
        hidePEXUIButton?.onClick.AddListener(() =>
        {
            SetUIVisibility(false);
            onShowPEXUI?.Invoke(model.id, false);
        });
        startPEXButton?.onClick.AddListener(() =>
        {
            SetAsPlaying(true);
            onStartPEX?.Invoke(model.id, true);
        });
        stopPEXButton?.onClick.AddListener(() =>
        {
            SetAsPlaying(false);
            onStartPEX?.Invoke(model.id, false);
        });
    }
}