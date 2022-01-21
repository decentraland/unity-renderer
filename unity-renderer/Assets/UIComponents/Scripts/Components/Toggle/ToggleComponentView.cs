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
    /// Set the toggle as on or off.
    /// </summary>
    bool isOn { get; set; }

    /// <summary>
    /// Set the toggle text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetText(string newText);

    /// <summary>
    /// Set the toggle id.
    /// </summary>
    /// <param name="newId">New id.</param>
    void SetId(string newId);
}

public class ToggleComponentView : BaseComponentView, IToggleComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal Toggle toggle;
    [SerializeField] internal TMP_Text toggleText;

    [Header("Configuration")]
    [SerializeField] internal ToggleComponentModel model;

    public event Action<bool, string> OnSelectedChanged;

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

        toggle.onValueChanged.AddListener((isOn) => OnSelectedChanged?.Invoke(isOn, model.id));
    }

    public override void Dispose()
    {
        base.Dispose();

        toggle.onValueChanged.RemoveAllListeners();
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

        isOn = model.isOn;
        SetText(model.text);
        SetId(model.id);
    }

    public void SetText(string newText)
    {
        model.text = newText;

        if (toggleText == null)
            return;

        toggleText.text = newText;
    }

    public void SetId(string newId)
    {
        model.id = newId;
    }
}
