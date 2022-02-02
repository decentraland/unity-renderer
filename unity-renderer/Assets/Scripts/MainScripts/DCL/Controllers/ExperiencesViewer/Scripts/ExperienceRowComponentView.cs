using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IExperienceRowComponentView
{
    /// <summary>
    /// Event that will be triggered when the Show PEX UI button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onShowPEXUIClick { get; }

    /// <summary>
    /// Event that will be triggered when the Hide PEX UI button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onHidePEXUIClick { get; }

    /// <summary>
    /// Event that will be triggered when the Start PEX button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onStartPEXClick { get; }

    /// <summary>
    /// Event that will be triggered when the Start PEX button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onStopPEXClick { get; }

    /// <summary>
    /// Set an icon from a 2D texture,
    /// </summary>
    /// <param name="texture">2D texture.</param>
    void SetIcon(Texture2D texture);

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
    [SerializeField] internal ButtonComponentView showPEXUIButton;
    [SerializeField] internal ButtonComponentView hidePEXUIButton;
    [SerializeField] internal ButtonComponentView startPEXButton;
    [SerializeField] internal ButtonComponentView stopPEXButton;
    [SerializeField] internal Image backgroundImage;

    [Header("Configuration")]
    [SerializeField] internal ExperienceRowComponentModel model;

    internal Color originalBackgroundColor;
    internal Color onHoverColor;

    public Button.ButtonClickedEvent onShowPEXUIClick => showPEXUIButton?.onClick;
    public Button.ButtonClickedEvent onHidePEXUIClick => hidePEXUIButton?.onClick;
    public Button.ButtonClickedEvent onStartPEXClick => startPEXButton?.onClick;
    public Button.ButtonClickedEvent onStopPEXClick => stopPEXButton?.onClick;

    public override void Awake()
    {
        base.Awake();

        originalBackgroundColor = backgroundImage.color;
        RefreshControl();
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

        SetIcon(model.icon);
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

    public void SetIcon(Texture2D texture)
    {
        model.icon = texture;

        if (iconImage == null)
            return;

        iconImage.SetImage(texture);
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
}