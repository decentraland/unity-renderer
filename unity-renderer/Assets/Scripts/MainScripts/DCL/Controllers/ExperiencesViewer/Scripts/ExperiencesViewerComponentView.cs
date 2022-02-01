using System.Collections.Generic;
using UnityEngine;

public interface IExperiencesViewerComponentView
{
    /// <summary>
    /// Set the available realms component with a list of realms.
    /// </summary>
    /// <param name="realms">List of realms (model) to be loaded.</param>
    void SetAvailableExperiences(List<ExperienceRowComponentModel> realms);

    /// <summary>
    /// Shows/Hides the game object of the Experiences Viewer.
    /// </summary>
    /// <param name="isActive">True to show it.</param>
    void SetVisible(bool isActive);
}

public class ExperiencesViewerComponentView : BaseComponentView, IExperiencesViewerComponentView
{
    [Header("Assets References")]
    [SerializeField] internal ExperienceRowComponentView experienceRowPrefab;

    public override void RefreshControl()
    {

    }

    public void SetAvailableExperiences(List<ExperienceRowComponentModel> realms)
    {
        
    }

    public void SetVisible(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    internal static ExperiencesViewerComponentView Create()
    {
        ExperiencesViewerComponentView experiencesViewerComponentView = Instantiate(Resources.Load<GameObject>("ExperiencesViewer")).GetComponent<ExperiencesViewerComponentView>();
        experiencesViewerComponentView.name = "_ExperiencesViewer";

        return experiencesViewerComponentView;
    }
}
