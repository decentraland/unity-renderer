using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IDropdownComponentView
{
    /// <summary>
    /// Set the dropdown as multiselect or not.
    /// </summary>
    bool isMultiselect { get; set; }

    /// <summary>
    /// Event that will be triggered when the toggle changes.
    /// </summary>
    event Action<bool, string> OnOptionSelectionChanged;

    /// <summary>
    /// Open the options list.
    /// </summary>
    void Open();

    /// <summary>
    /// Closes the options list
    /// </summary>
    void Close();

    /// <summary>
    /// Set the dropdown title.
    /// </summary>
    /// <param name="newText">New title.</param>
    void SetTitle(string newText);

    /// <summary>
    /// Set the available options of the dropdown.
    /// </summary>
    /// <param name="options">List of options..</param>
    void SetOptions(List<ToggleComponentModel> options);

    /// <summary>
    /// Get an option of the dropdown.
    /// </summary>
    /// <param name="index">Index of the list of options.</param>
    /// <returns>A specific option toggle.</returns>
    IToggleComponentView GetOption(int index);

    /// <summary>
    /// Get all the options of the dropdown.
    /// </summary>
    /// <returns>The list of options.</returns>
    List<IToggleComponentView> GetAllOptions();
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

    public bool isMultiselect 
    {
        get => model.isMultiselect;
        set => model.isMultiselect = value;
    }

    public event Action<bool, string> OnOptionSelectionChanged;

    internal bool isOpen = false;

    public override void Awake()
    {
        base.Awake();

        RefreshControl();
        Close();

        button.onClick.AddListener(() => ToggleOptionsList());

        OnOptionSelectionChanged += (isOn, id) =>
        {
            if (isOn)
                Debug.Log($"Option {id} ON");
            else
                Debug.Log($"Option {id} OFF");
        };
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (DropdownComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        isMultiselect = model.isMultiselect;
        SetTitle(model.title);
        SetOptions(model.options);
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

    public void SetTitle(string newText)
    {
        model.title = newText;

        if (title == null)
            return;

        title.text = newText;
    }

    public void SetOptions(List<ToggleComponentModel> options)
    {
        model.options = options;

        RemoveAllInstantiatedOptions();

        for (int i = 0; i < options.Count; i++)
        {
            CreateOption(options[i], $"Option_{i}");
        }
    }

    public IToggleComponentView GetOption(int index)
    {
        if (index >= availableOptions.GetItems().Count)
            return null;

        return availableOptions.GetItems()[index] as IToggleComponentView;
    }

    public List<IToggleComponentView> GetAllOptions() 
    {
        return availableOptions.GetItems()
            .Select(x => x as IToggleComponentView)
            .ToList();
    }

    public override void Dispose()
    {
        base.Dispose();

        button.onClick.RemoveAllListeners();
    }

    internal void ToggleOptionsList()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    internal void CreateOption(ToggleComponentModel newOptionModel, string name)
    {
        if (togglePrefab == null)
            return;

        ToggleComponentView newGO = Instantiate(togglePrefab);
        newGO.name = name;
        newGO.gameObject.SetActive(true);
        newGO.Configure(newOptionModel);
        availableOptions.AddItem(newGO);

        newGO.OnSelectedChanged += OnOptionSelected;
    }

    internal void OnOptionSelected(bool isOn, string optionId)
    {
        OnOptionSelectionChanged?.Invoke(isOn, optionId);

        if (isOn && !isMultiselect)
        {
            List<IToggleComponentView> allOptions = GetAllOptions();
            foreach (IToggleComponentView option in allOptions)
            {
                if (option.id != optionId)
                    option.isOn = false;
            }
        }
    }

    internal void RemoveAllInstantiatedOptions()
    {
        availableOptions.RemoveItems();

        foreach (Transform child in availableOptions.transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                if (isActiveAndEnabled)
                    StartCoroutine(DestroyGameObjectOnEditor(child.gameObject));
            }
        }
    }

    internal IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }

    [ContextMenu("Test SANTI")]
    public void TestSanti()
    {
        List<ToggleComponentModel> test = new List<ToggleComponentModel>();
        for (int i = 0; i < 20; i++)
        {
            test.Add(new ToggleComponentModel
            {
                id = i.ToString(),
                isOn = false,
                text = "Option" + i.ToString()
            });
        }

        isMultiselect = false;
        SetOptions(test);
    }

    [ContextMenu("Test SANTI Multiselect")]
    public void TestSanti2()
    {
        List<ToggleComponentModel> test = new List<ToggleComponentModel>();
        for (int i = 0; i < 20; i++)
        {
            test.Add(new ToggleComponentModel
            {
                id = i.ToString(),
                isOn = false,
                text = "Option" + i.ToString()
            });
        }

        isMultiselect = true;
        SetOptions(test);
    }
}
