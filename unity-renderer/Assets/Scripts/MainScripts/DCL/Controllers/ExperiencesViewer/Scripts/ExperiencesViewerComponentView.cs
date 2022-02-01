using System;
using System.Collections.Generic;
using UnityEngine;

public interface IExperiencesViewerComponentView
{
    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action OnCloseButtonPressed;

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

    [Header("Prefab References")]
    [SerializeField] internal ButtonComponentView closeButton;

    public event Action OnCloseButtonPressed;

    public override void Start()
    {
        base.Start();

        closeButton.onClick.AddListener(() => OnCloseButtonPressed?.Invoke());
    }

    public override void RefreshControl()
    {

    }

    public void SetAvailableExperiences(List<ExperienceRowComponentModel> realms)
    {
        
    }

    public void SetVisible(bool isActive) { gameObject.SetActive(isActive); }

    public override void Dispose()
    {
        base.Dispose();

        closeButton.onClick.RemoveAllListeners();
    }

    internal static ExperiencesViewerComponentView Create()
    {
        ExperiencesViewerComponentView experiencesViewerComponentView = Instantiate(Resources.Load<GameObject>("ExperiencesViewer")).GetComponent<ExperiencesViewerComponentView>();
        experiencesViewerComponentView.name = "_ExperiencesViewer";

        return experiencesViewerComponentView;
    }
}
