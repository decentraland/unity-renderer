using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ITagComponentView
{
    /// <summary>
    /// Set the tag text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetText(string newText);

    /// <summary>
    /// Set the tag icon.
    /// </summary>
    /// <param name="newIcon">New Icon. Null for hide the icon.</param>
    void SetIcon(Sprite newIcon);
}

public class TagComponentView : BaseComponentView, ITagComponentView, IComponentModelConfig<TagComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal Image icon;
    [SerializeField] internal TMP_Text text;

    [Header("Configuration")]
    [SerializeField] internal TagComponentModel model;

    public void Configure(TagComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

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

        icon.enabled = newIcon != null;
        icon.sprite = newIcon;
    }
}