using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IDropdownComponentView
{
    /// <summary>
    /// Set the dropdown title.
    /// </summary>
    /// <param name="newText">New title.</param>
    void SetTitle(string newText);

    /// <summary>
    /// Open the options list.
    /// </summary>
    void Open();

    /// <summary>
    /// Closes the options list
    /// </summary>
    void Close();

    /// <summary>
    /// Set the available options of the dropdown.
    /// </summary>
    /// <param name="options">List of options..</param>
    void SetOptions(List<ToggleComponentModel> options);
}

public class DropdownComponentView : BaseComponentView, IDropdownComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] internal TMP_Text title;
    [SerializeField] internal GameObject optionsPanel;
    [SerializeField] internal GridContainerComponentView availableOptions;
    [SerializeField] internal ToggleComponentView togglePrefab;

    [Header("Configuration")]
    [SerializeField] internal DropdownComponentModel model;

    internal bool isOpen = false;

    public void Configure(BaseComponentModel newModel)
    {
        model = (DropdownComponentModel)newModel;
        RefreshControl();
    }

    public override void Awake()
    {
        base.Awake();

        Close();

        button.onClick.AddListener(() => ToggleOptionsList());
    }

    public override void Dispose()
    {
        base.Dispose();

        button.onClick.RemoveAllListeners();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetTitle(model.title);
    }

    public void SetTitle(string newText)
    {
        model.title = newText;

        if (title == null)
            return;

        title.text = newText;
    }

    public void Open()
    {
        optionsPanel.SetActive(true);
        isOpen = true;
    }

    public void Close()
    {
        optionsPanel.SetActive(false);
        isOpen = false;
    }

    internal void ToggleOptionsList()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void SetOptions(List<ToggleComponentModel> options)
    {
    }
}
