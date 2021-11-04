using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISectionSelectorComponentView
{
    /// <summary>
    /// Set the sections of the selector.
    /// </summary>
    /// <param name="sections">List of UI components.</param>
    void SetSections(List<SectionToggleModel> sections);

    /// <summary>
    /// Get a section of the section selector.
    /// </summary>
    /// <param name="index">Index of the list of sections.</param>
    /// <returns>A specific section toggle.</returns>
    ISectionToggle GetSection(int index);

    /// <summary>
    /// Get all the sections of the section selector.
    /// </summary>
    /// <returns>The list of sections.</returns>
    List<ISectionToggle> GetAllSections();
}

public class SectionSelectorComponentView : BaseComponentView, ISectionSelectorComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal SectionToggle sectionToggleTemplate;

    [Header("Configuration")]
    [SerializeField] internal SectionSelectorComponentModel model;

    internal List<ISectionToggle> instantiatedSections = new List<ISectionToggle>();

    public void Configure(BaseComponentModel newModel)
    {
        model = (SectionSelectorComponentModel)newModel;
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

        RemoveAllInstantiatedSections();

        for (int i = 0; i < sections.Count; i++)
        {
            CreateSection(sections[i], $"Section_{i}");
        }

        for (int i = 0; i < instantiatedSections.Count; i++)
        {
            if (i == 0)
            {
                instantiatedSections[i].SelectToggle();
                instantiatedSections[i].SetSelectedVisuals();
            }
            else
            {
                instantiatedSections[i].SetUnselectedVisuals();
            }
        }
    }

    public ISectionToggle GetSection(int index)
    {
        if (index >= instantiatedSections.Count)
            return null;

        return instantiatedSections[index];
    }

    public List<ISectionToggle> GetAllSections() { return instantiatedSections; }

    internal void CreateSection(SectionToggleModel newSectionModel, string name)
    {
        if (sectionToggleTemplate == null)
            return;

        SectionToggle newGO = Instantiate(sectionToggleTemplate, transform);
        newGO.name = name;
        newGO.SetInfo(newSectionModel);
        newGO.gameObject.SetActive(true);
        instantiatedSections.Add(newGO);
    }

    internal void RemoveAllInstantiatedSections()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject == sectionToggleTemplate.gameObject)
                continue;

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

        instantiatedSections.Clear();
    }

    internal IEnumerator DestroyGameObjectOnEditor(GameObject go)
    {
        yield return null;
        DestroyImmediate(go);
    }
}