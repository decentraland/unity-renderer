using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IToggleComponentView
{
    /// <summary>
    /// Event that will be triggered when the toggle changes.
    /// </summary>
    event Action<bool, string> OnSelectedChanged;

    /// <summary>
    /// Id asociated to the toogle.
    /// </summary>
    string id { get; }

    /// <summary>
    /// On/Off state of the toggle.
    /// </summary>
    bool isOn { get; set; }

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
    [SerializeField] internal Toggle toggle;
    [SerializeField] internal TMP_Text text;
    [SerializeField] GameObject activeOn = null;
    [SerializeField] GameObject activeOff = null;

    [Header("Configuration")]
    [SerializeField] internal ToggleComponentModel model;

    public event Action<bool, string> OnSelectedChanged;

    public string id
    {
        get => model.id;
        set => model.id = value;
    }

    public bool isOn 
    { 
        get => toggle.isOn;
        set
        {
            model.isOn = value;

            if (toggle == null)
                return;

            toggle.isOn = value;
        }
    }

    public override void Awake()
    {
        base.Awake();

        toggle.onValueChanged.AddListener(ToggleChanged);
    }

    private void ToggleChanged(bool isOn) 
    {
        if (activeOn)
            activeOn.gameObject.SetActive(isOn);
        if (activeOff)
            activeOff.gameObject.SetActive(!isOn);

        OnSelectedChanged?.Invoke(isOn, model.id);
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (ToggleComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        id = model.id;
        isOn = model.isOn;
        SetText(model.text);
        SetTextActive(model.isTextActive);
    }

    public void SetText(string newText)
    {
        model.text = newText;

        if (text == null)
            return;

        text.text = newText;
    }

    public override void Dispose()
    {
        base.Dispose();

        toggle.onValueChanged.RemoveAllListeners();
    }

    public bool IsInteractable() { return toggle.interactable; }

    public void SetInteractable(bool isActive) { toggle.interactable = isActive; }

    public void SetTextActive(bool isActive) 
    {
        model.isTextActive = isActive;
        text.gameObject.SetActive(isActive); 
    }
}
