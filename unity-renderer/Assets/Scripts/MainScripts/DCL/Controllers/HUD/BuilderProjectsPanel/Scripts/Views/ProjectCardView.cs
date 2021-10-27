using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;
using DCL.Helpers;
using UnityEngine.UI;

internal interface IProjectCardView : IDisposable
{
    /// <summary>
    /// Edit project button pressed
    /// </summary>
    event Action<ProjectData> OnEditorPressed;

    /// <summary>
    ///  Setting button pressed
    /// </summary>
    event Action<ProjectData> OnSettingsPressed;

    /// <summary>
    /// Expand button pressed
    /// </summary>
    event Action<ProjectData, IProjectCardView> OnExpandMenuPressed;

    /// <summary>
    /// Data of the project card
    /// </summary>
    ProjectData projectData { get; }

    /// <summary>
    /// Info for the search result
    /// </summary>
    ISearchInfo searchInfo { get; }

    /// <summary>
    /// Position of the context menu button
    /// </summary>
    Vector3 contextMenuButtonPosition { get; }

    /// <summary>
    /// This setup the project data of the 
    /// </summary>
    /// <param name="projectData"></param>
    void Setup(ProjectData projectData);

    /// <summary>
    /// Set Parent of the card
    /// </summary>
    /// <param name="parent"></param>
    void SetParent(Transform parent);

    /// <summary>
    /// Reset to default parent
    /// </summary>
    void SetToDefaultParent();

    /// <summary>
    /// Configure the default parent
    /// </summary>
    /// <param name="parent">default parent to apply</param>
    void ConfigureDefaultParent(Transform parent);

    /// <summary>
    /// Active the card
    /// </summary>
    /// <param name="active"></param>
    void SetActive(bool active);

    /// <summary>
    /// This set the order of the card
    /// </summary>
    /// <param name="index"></param>
    void SetSiblingIndex(int index);
}

internal class ProjectCardView : MonoBehaviour, IProjectCardView
{
    static readonly Vector3 CONTEXT_MENU_OFFSET = new Vector3(6.24f, 12f, 0);

    public event Action<ProjectData> OnEditorPressed;
    public event Action<ProjectData> OnSettingsPressed;
    public event Action<ProjectData, IProjectCardView> OnExpandMenuPressed;

    [SerializeField] internal Button contextMenuButton;
    [SerializeField] private Texture2D defaultThumbnail;

    [Space]
    [SerializeField] private RawImageFillParent thumbnail;

    ProjectData IProjectCardView.projectData => sceneData;
    ISearchInfo IProjectCardView.searchInfo { get; } = new SearchInfo();
    Vector3 IProjectCardView.contextMenuButtonPosition => contextMenuButton.transform.position + CONTEXT_MENU_OFFSET;

    private ProjectData sceneData;
    private ISearchInfo searchInfo;
    private Vector3 contextMenuButtonPosition;

    public void Setup(ProjectData projectData) { this.sceneData = projectData; }
    public void SetParent(Transform parent) { transform.SetParent(parent); }

    public void Dispose() { }

    public void cardSetParent(Transform parent)
    {
        if (transform.parent == parent)
            return;

        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }
    public void SetToDefaultParent() { throw new NotImplementedException(); }
    public void ConfigureDefaultParent(Transform parent) { throw new NotImplementedException(); }

    public void SetActive(bool active) { throw new NotImplementedException(); }
    public void SetSiblingIndex(int index) { throw new NotImplementedException(); }
}