using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Configuration;
using UnityEngine;
using DCL.Helpers;
using TMPro;
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
    
    void SetName(string name);
    void SetSize(int rows, int columns);
    void SetThumbnail(string thumbnailUrl);
    void SetThumbnail(Texture2D thumbnailTexture);
}

internal class ProjectCardView : MonoBehaviour, IProjectCardView
{
    static readonly Vector3 CONTEXT_MENU_OFFSET = new Vector3(6.24f, 12f, 0);

    public event Action<ProjectData> OnEditorPressed;
    public event Action<ProjectData> OnSettingsPressed;
    public event Action<ProjectData, IProjectCardView> OnExpandMenuPressed;

    [SerializeField] private Color syncColor;
    [SerializeField] private Color desyncColor;
    
    [SerializeField] private Image syncImage;
    
    [SerializeField] internal Button contextMenuButton;
    [SerializeField] internal Button editorButton;
    [SerializeField] private Texture2D defaultThumbnail;

    [SerializeField] private GameObject loadingImgGameObject;
    [SerializeField] internal TextMeshProUGUI projectNameTxt;
    [SerializeField] internal TextMeshProUGUI projectSizeTxt;
    [SerializeField] internal TextMeshProUGUI projectSyncTxt;

    [Space]
    [SerializeField] private RawImageFillParent thumbnail;

    ProjectData IProjectCardView.projectData => projectData;
    ISearchInfo IProjectCardView.searchInfo { get; } = new SearchInfo();
    Vector3 IProjectCardView.contextMenuButtonPosition => contextMenuButton.transform.position + CONTEXT_MENU_OFFSET;

    private ProjectData projectData;
    private ISearchInfo searchInfo;
    private Vector3 contextMenuButtonPosition;

    private string thumbnailId = null;
    private Transform defaultParent;
    private AssetPromise_Texture thumbnailPromise;

    private void Awake()
    {
        editorButton.onClick.AddListener(EditorButtonClicked);    
        contextMenuButton.onClick.AddListener(EditorButtonClicked);    
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
        editorButton.onClick.RemoveAllListeners();
    }
    
    public void Setup(ProjectData projectData)
    {
        this.projectData = projectData;
        SetName(projectData.title);
        SetSize(projectData.rows,projectData.colums);
        
        SetThumbnail(projectData.id);
        
        ((IProjectCardView)this).searchInfo.SetId(projectData.id);
    }

    private void EditorButtonClicked()
    {
        OnEditorPressed?.Invoke(projectData);
    }

    public void SetThumbnail(string thumbnailId)
    {
        if (this.thumbnailId == thumbnailId)
            return;
        
        string projectThumbnailUrl = "";
        if (!string.IsNullOrEmpty(thumbnailId))
        {
            projectThumbnailUrl = BIWUrlUtils.GetBuilderProjecThumbnailUrl().Replace("{id}", thumbnailId);
        }

        this.thumbnailId = thumbnailId;

        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }
        
        if (string.IsNullOrEmpty(projectThumbnailUrl))
        {
            SetThumbnail((Texture2D) null);
            return;
        }

        thumbnailPromise = new AssetPromise_Texture(projectThumbnailUrl);
        thumbnailPromise.OnSuccessEvent += texture => SetThumbnail(texture.texture);
        thumbnailPromise.OnFailEvent += texture => SetThumbnail((Texture2D) null);

        loadingImgGameObject.SetActive(true);
        thumbnail.enabled = false;
        AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
    }

    public void SetThumbnail(Texture2D thumbnailTexture)
    {
        loadingImgGameObject.SetActive(false);
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
        thumbnail.enabled = true;
    }

    public void SetParent(Transform parent)
    {
        if (transform.parent == parent)
            return;

        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }
    
    public void SetToDefaultParent() { transform.SetParent(defaultParent); }
    
    public void ConfigureDefaultParent(Transform parent) { defaultParent = parent; }

    public void SetActive(bool active) { gameObject.SetActive(active); }
    
    public void SetSiblingIndex(int index) { transform.SetSiblingIndex(index); }
    
    public void SetName(string name)
    {
        projectNameTxt.text = name;
        ((IProjectCardView)this).searchInfo.SetName(name);
    }
    
    public void SetSize(int rows, int columns)
    {
        projectSizeTxt.text = rows + "x" + columns;
        ((IProjectCardView)this).searchInfo.SetSize(rows * columns);
    }
}