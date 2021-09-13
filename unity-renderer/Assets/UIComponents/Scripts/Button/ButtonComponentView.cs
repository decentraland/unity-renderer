using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IButtonComponentView
{
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

    /// <summary>
    /// Set an action on the onClick button.
    /// </summary>
    /// <param name="action">Action to call after clicking the button.</param>
    void SetOnClickAction(Action action);
}

public class ButtonComponentView : BaseComponentView, IButtonComponentView
{
    [Header("Prefab References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image icon;

    [Header("Configuration")]
    [SerializeField] protected ButtonComponentModel model;

    public virtual void Configure(ButtonComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        SetText(model.text);
        SetIcon(model.icon);
        SetOnClickAction(model.onClickAction);
    }

    public override void Dispose()
    {
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

        icon.enabled = newIcon != null;
        icon.sprite = newIcon;
    }

    public void SetOnClickAction(Action action)
    {
        model.onClickAction = action;

        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }
}