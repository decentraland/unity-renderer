using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal interface IProjectsController
{
    event Action<Vector2Int> OnEditorPressed;
    event Action<Dictionary<string, IProjectCardView>> OnProjectsSet;
    void SetProjects(ProjectData[] projects);
    void AddListener(IProjectsListener listener);
    void RemoveListener(IProjectsListener listener);
    Dictionary<string, IProjectCardView> GetProjects();
}

internal class ProjectsController : IProjectsController
{
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<Dictionary<string, IProjectCardView>> OnProjectsSet;

    private Dictionary<string, IProjectCardView> projects = new Dictionary<string, IProjectCardView>();
    private readonly SceneCardView projectCardViewPrefab;
    private readonly Transform defaultParent;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="projectCardViewPrefab">prefab for project's card</param>
    /// <param name="defaultParent">default parent for scene's card</param>
    public ProjectsController(SceneCardView projectCardViewPrefab, Transform defaultParent = null)
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