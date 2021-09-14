using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISectionSelectorComponentView
{
    /// <summary>
    /// Fill the model and updates the section selector with this data.
    /// </summary>
    /// <param name="model">Data to configure the section selector.</param>
    void Configure(SectionSelectorComponentModel model);

    /// <summary>
    /// Set the items of the grid.
    /// </summary>
    /// <param name="items">List of UI components.</param>
    void SetSections(List<SectionToggleModel> sections);

    /// <summary>
    /// Get a section of the section selector.
    /// </summary>
    /// <param name="index">Index of the list of sections.</param>
    /// <returns>A specific section toggle.</returns>
    ISectionToggle GetSection(int index);
}

public class SectionSelectorComponentView : BaseComponentView, ISectionSelectorComponentView
{
    [Header("Prefab References")]
    [SerializeField] private SectionToggle sectionToggleTemplate;

    [Header("Configuration")]
    [SerializeField] protected SectionSelectorComponentModel model;

    private List<ISectionToggle> instantiatedSections = new List<ISectionToggle>();

    public override void Initialize()
    {
        base.Initialize();
        Configure(model);
    }

    public void Configure(SectionSelectorComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetSections(model.sections);
    }

    public void SetSections(List<SectionToggleModel> sections)
    {
        model.sections = sections;

        RemoveAllIntantiatedSections();

        for (int i = 0; i < sections.Count; i++)
        {
            CreateSection(sections[i], $"Section_{i}");
        }
    }

    public ISectionToggle GetSection(int index)
    {
        if (index >= instantiatedSections.Count)
            return null;

        return instantiatedSections[index];
    }

    internal void CreateSection(SectionToggleModel newSectionModel, string name)
    {
        if (Application.isEditor)
        {
            if (isActiveAndEnabled)
                StartCoroutine(IntantiateSectionToggleOnEditor(newSectionModel, name));
        }
        else
        {
            IntantiateSectionToggle(newSectionModel, name);
        }
    }

    internal void IntantiateSectionToggle(SectionToggleModel newSectionModel, string name)
    {
        if (sectionToggleTemplate == null)
            return;

        SectionToggle newGO = Instantiate(sectionToggleTemplate, transform);
        newGO.name = name;
        newGO.SetInfo(newSectionModel);
        newGO.gameObject.SetActive(true);
        instantiatedSections.Add(newGO);
    }

    internal IEnumerator IntantiateSectionToggleOnEditor(SectionToggleModel sectionModel, string name)
    {
        yield return null;
        IntantiateSectionToggle(sectionModel, name);
    }

    internal void RemoveAllIntantiatedSections()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetInstanceID() == sectionToggleTemplate.gameObject.GetInstanceID())
                continue;

            if (Application.isEditor)
            {
                if (isActiveAndEnabled)
                    StartCoroutine(DestroyGameObjectOnEditor(child.gameObject));
            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        instantiatedSections.Clear();
    }

    internal IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }

    public void HasClicked() { Debug.Log("CLICKED!!"); }
}