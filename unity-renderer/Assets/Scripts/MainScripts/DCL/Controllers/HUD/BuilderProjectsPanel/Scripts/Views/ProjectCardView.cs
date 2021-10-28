using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;
using DCL.Helpers;
using UnityEngine.UI;

internal interface IProjectCardView : IDisposable
{
    event Action<Vector2Int> OnEditorPressed;
    event Action<ProjectData> OnSettingsPressed;
    event Action<ProjectData, IProjectCardView> OnExpandMenuPressed;
    ProjectData projectData { get; }
    ISearchInfo searchInfo { get; }
    Vector3 contextMenuButtonPosition { get; }
    void Setup(ProjectData sceneData);
    void SetParent(Transform parent);
    void SetToDefaultParent();
    void ConfigureDefaultParent(Transform parent);
    void SetActive(bool active);
    void SetSiblingIndex(int index);
}

internal class ProjectCardView : MonoBehaviour, IProjectCardView
{
    static readonly Vector3 CONTEXT_MENU_OFFSET = new Vector3(6.24f, 12f, 0);

    public event Action<Vector2Int> OnEditorPressed;
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

    public void Setup(ProjectData sceneData) { this.sceneData = sceneData; }

    public void Dispose() { }

    public void SetParent(Transform parent)
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