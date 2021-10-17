using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IButtonComponentView
{
    /// <summary>
    /// Event that will be triggered when the button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; }

    /// <summary>
    /// Fill the model and updates the button with this data.
    /// </summary>
    /// <param name="model">Data to configure the button.</param>
    void Configure(ButtonComponentModel model);

    /// <summary>
    /// Set the button text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetText(string newText);

    /// <summary>
    /// Set the button icon.
    /// </summary>
    /// <param name="newIcon">New Icon. Null for hide the icon.</param>
    void SetIcon(Sprite newIcon);
}

public class ButtonComponentView : BaseComponentView, IButtonComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] internal TMP_Text text;
    [SerializeField] internal Image icon;

    [Header("Configuration")]
    [SerializeField] internal ButtonComponentModel model;

    public Button.ButtonClickedEvent onClick => button?.onClick;

    public virtual void Configure(ButtonComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetText(model.text);
        SetIcon(model.icon);

        onClick.RemoveAllListeners();
        onClick.AddListener(() => model.onClick.Invoke());
    }

    public override void Dispose()
    {
        base.Dispose();

        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
    }

    public void SetText(string newText)
    {
        model.text = newText;

        if (text == null)
            return;

        text.text = newText;
    }

    public void SetIcon(Sprite newIcon)
    {
        model.icon = newIcon;

        if (icon == null)
            return;

        icon.gameObject.SetActive(newIcon != null);
        icon.sprite = newIcon;
    }
}