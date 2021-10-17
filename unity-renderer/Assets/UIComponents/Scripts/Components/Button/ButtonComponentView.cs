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

    public override void SetModel(BaseComponentModel newModel) { model = (ButtonComponentModel)newModel; }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetText(model.text);
        SetIcon(model.icon);
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