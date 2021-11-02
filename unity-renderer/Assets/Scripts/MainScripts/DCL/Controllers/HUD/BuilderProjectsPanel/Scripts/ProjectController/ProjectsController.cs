using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

/// <summary>
/// This interface is responsible to handle the projects of builder in world panel.
/// It will handle the listeners that will add/remove projects to the list   
/// </summary>
internal interface IProjectsController
{
    /// <summary>
    /// This action will be called when the user want to edit a project
    /// </summary>
    event Action<ProjectData> OnEditorPressed;

    /// <summary>
    /// This action will be called each time that we changes the project list
    /// </summary>
    event Action<Dictionary<string, IProjectCardView>> OnProjectsSet;

    /// <summary>
    /// This will set the project list
    /// </summary>
    /// <param name="projects"></param>
    void SetProjects(ProjectData[] projects);

    /// <summary>
    /// This will add a listener that will be responsible to add/remove projects
    /// </summary>
    /// <param name="listener"></param>
    void AddListener(IProjectsListener listener);

    /// <summary>
    /// This will remove a listener that will be responsible to add/remove projects 
    /// </summary>
    /// <param name="listener"></param>
    void RemoveListener(IProjectsListener listener);

    /// <summary>
    /// This will get all projects
    /// </summary>
    /// <returns>Dictionary of all the projects indexed by their id</returns>
    Dictionary<string, IProjectCardView> GetProjects();
}

internal class ProjectsController : IProjectsController
{
    public event Action<ProjectData> OnEditorPressed;
    public event Action<Dictionary<string, IProjectCardView>> OnProjectsSet;

    private Dictionary<string, IProjectCardView> projects = new Dictionary<string, IProjectCardView>();
    private readonly ProjectCardView projectCardViewPrefab;
    private readonly Transform defaultParent;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="projectCardViewPrefab">prefab for project's card</param>
    /// <param name="defaultParent">default parent for scene's card</param>
    public ProjectsController(ProjectCardView projectCardViewPrefab, Transform defaultParent = null)
    {
        this.projectCardViewPrefab = projectCardViewPrefab;
        this.defaultParent = defaultParent;
    }

    public void SetProjects(ProjectData[] projects)
    {
        this.projects = new Dictionary<string, IProjectCardView>();
        foreach (var project in projects)
        {
            this.projects.Add(project.id, CreateCardView());
        }
        OnProjectsSet?.Invoke(this.projects);
    }

    public void AddListener(IProjectsListener listener)
    {
        OnProjectsSet += listener.OnSetProjects;
        listener.OnSetProjects(projects);
    }

    public void RemoveListener(IProjectsListener listener) { }

    public Dictionary<string, IProjectCardView> GetProjects() { return projects; }

    private IProjectCardView CreateCardView()
    {
        ProjectCardView projectCardView = GameObject.Instantiate(projectCardViewPrefab).GetComponent<ProjectCardView>();
        IProjectCardView view = projectCardView;

        view.SetActive(false);
        view.ConfigureDefaultParent(defaultParent);
        view.SetToDefaultParent();

        view.OnEditorPressed += OnEditorPressed;
        view.OnSettingsPressed += OnSceneSettingsPressed;

        return view;
    }

    private void DestroyCardView(IProjectCardView projectCardView)
    {
        // NOTE: there is actually no need to unsubscribe here, but, just in case...
        projectCardView.OnEditorPressed -= OnEditorPressed;
        projectCardView.OnSettingsPressed -= OnSceneSettingsPressed;

        projectCardView.Dispose();
    }

    private void OnSceneSettingsPressed(ProjectData sceneData) {  }
}