using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IToggleComponentView
{
    /// <summary>
    /// Event that will be triggered when the toggle is clicked.
    /// </summary>
    Toggle.ToggleEvent onToggleChange { get; }

    /// <summary>
    /// Set the toggle text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetText(string newText);

    /// <summary>
    /// Set toggle text active
    /// </summary>
    /// <param name="isActive">Is active</param>
    void SetTextActive(bool isActive);

    /// <summary>
    /// Set the toggle clickable or not.
    /// </summary>
    /// <param name="isInteractable">Clickable or not</param>
    void SetInteractable(bool isInteractable);

    /// <summary>
    /// Return if the toggle is Interactable or not
    /// </summary>
    bool IsInteractable();
}

public class ToggleComponentView : BaseComponentView, IToggleComponentView, IComponentModelConfig
{

    [Header("Prefab References")]
    [SerializeField] private Toggle toggle;
    [SerializeField] internal TMP_Text text;
    [SerializeField] internal Image icon;

    [Header("Configuration")]
    [SerializeField] internal ToggleComponentModel model;

    public Toggle.ToggleEvent onToggleChange => toggle?.onValueChanged;

    public void Configure(BaseComponentModel newModel)
    {
        model = (ToggleComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetTextActive(model.isTextActive);
        SetText(model.text);
    }

    public bool IsInteractable() { return toggle.interactable; }

    public void SetInteractable(bool isActive) { toggle.interactable = isActive; }

    public void SetTextActive(bool isActive) { text.gameObject.SetActive(isActive); }

    public void SetText(string newText)
    {
        model.text = newText;

        if (text == null)
            return;

        text.text = newText;
    }
}
