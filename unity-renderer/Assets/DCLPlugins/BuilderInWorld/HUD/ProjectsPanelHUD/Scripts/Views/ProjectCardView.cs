using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
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
    event Action OnExpandMenuPressed;

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
    /// Set the scenes of the project
    /// </summary>
    /// <param name="scenes"></param>
    void SetScenes(List<Scene> scenes);
    
    /// <summary>
    /// This setup the project data 
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
    void SetThumbnail(string thumbnailUrl, string filename);
    void SetThumbnail(Texture2D thumbnailTexture);
}

internal class ProjectCardView : MonoBehaviour, IProjectCardView
{
    static readonly Vector3 CONTEXT_MENU_OFFSET = new Vector3(6.24f, 12f, 0);

    internal const string NOT_PUBLISHED = "NOT PUBLISHED";
    internal const string PUBLISHED_IN = "PUBLISHED IN";
    
    internal const float SCENE_CARD_SIZE = 84;
    internal const float SCENE_CARD_ITEM_PADDING = 18;
    internal const float SCENE_CARD_TOTAL_PADDING = 36;

    public event Action<ProjectData> OnEditorPressed;
    public event Action<ProjectData> OnSettingsPressed;
    public event Action OnExpandMenuPressed;

    [Header("Design Variables")]

    [SerializeField] private  float animationSpeed = 6f;

    [Header("Project References")]
    [SerializeField] internal Color syncColor;
    [SerializeField] internal Color desyncColor;
    [SerializeField] private Texture2D defaultThumbnail;
    [SerializeField] private GameObject projectSceneCardViewPrefab;
    
    
    [Header("Prefab references")]
    [SerializeField] internal Image syncImage;
    
    [SerializeField] internal Button contextMenuButton;
    [SerializeField] internal Button expandButton;
    [SerializeField] internal Button editorButton;
    
    [SerializeField] internal RectTransform scenesContainer;
    [SerializeField] internal VerticalLayoutGroup layoutGroup;

    [SerializeField] private GameObject loadingImgGameObject;
    [SerializeField] private GameObject publishedGameObject;
    [SerializeField] private RectTransform downButtonTransform;
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
    private bool scenesAreVisible = false;
    private bool isDestroyed = false;
    
    private RectTransform rectTransform;

    internal List<Scene> scenesDeployedFromProject = new List<Scene>();
    internal List<IProjectSceneCardView> sceneCardViews = new List<IProjectSceneCardView>();

    private void Awake()
    {
        editorButton.onClick.AddListener(EditorButtonClicked);    
        expandButton.onClick.AddListener(ExpandButtonPressed);    
        contextMenuButton.onClick.AddListener(EditorButtonClicked);

        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        Dispose();
    }

    public void Dispose()
    {
        AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
        editorButton.onClick.RemoveAllListeners();
        if(!isDestroyed)
            Destroy(gameObject);
    }
    
    public void Setup(ProjectData projectData)
    {
        this.projectData = projectData;
        SetName(projectData.title);
        SetSize(projectData.rows,projectData.cols);
        
        SetThumbnail(projectData.id,projectData.thumbnail);
        
        ((IProjectCardView)this).searchInfo.SetId(projectData.id);
    }

    public void SetScenes(List<Scene> scenes)
    {
        scenesDeployedFromProject = scenes;
        publishedGameObject.gameObject.SetActive(true);
        if (scenes.Count == 0)
        {
            syncImage.enabled = false;
            projectSyncTxt.text = NOT_PUBLISHED;
            downButtonTransform.gameObject.SetActive(false);
        }
        else
        {
            downButtonTransform.gameObject.SetActive(true);
            bool isSync = true;
            long projectTimestamp = BIWUtils.ConvertToMilisecondsTimestamp(projectData.updated_at);
            foreach (Scene scene in scenes)
            {
                if (scene.deployTimestamp < projectTimestamp)
                {
                    isSync = false;
                    break;
                }
            }
            if (isSync)
                syncImage.color = syncColor;
            else
                syncImage.color = desyncColor;
            
            projectSyncTxt.text = PUBLISHED_IN;
        }
    }

    internal void ExpandButtonPressed()
    {
        if(scenesDeployedFromProject.Count == 0 )
            return;
        
        downButtonTransform.Rotate(Vector3.forward,180);
        
        scenesAreVisible = !scenesAreVisible;
        ScenesVisiblitityChange(scenesAreVisible);
        
        float amountToIncrease = sceneCardViews.Count * SCENE_CARD_SIZE + SCENE_CARD_TOTAL_PADDING + SCENE_CARD_ITEM_PADDING * (sceneCardViews.Count-1);
        
        if (scenesAreVisible)
        {
            layoutGroup.padding.bottom = 18;
            AddRectTransformHeight(rectTransform,amountToIncrease);
            AddRectTransformHeight(scenesContainer,amountToIncrease);
        }
        else
        {
            layoutGroup.padding.bottom = 0;
            AddRectTransformHeight(rectTransform, -amountToIncrease);
            AddRectTransformHeight(scenesContainer, -amountToIncrease);
        }

        OnExpandMenuPressed?.Invoke();
    }

    private void AddRectTransformHeight(RectTransform rectTransform, float height)
    {
        CoroutineStarter.Start(ChangeHeightAnimation(rectTransform, height));
    }
    
    private void ScenesVisiblitityChange(bool isVisible)
    {
        if(sceneCardViews.Count == 0)
            InstantiateScenesCards();

        foreach (IProjectSceneCardView scene in sceneCardViews)
        {
            scene.SetActive(isVisible);
        }
    }

    private void InstantiateScenesCards()
    {
        long projectTimestamp = BIWUtils.ConvertToMilisecondsTimestamp(projectData.updated_at);
        foreach (Scene scene in scenesDeployedFromProject)
        {
            bool isSync = scene.deployTimestamp < projectTimestamp;

            IProjectSceneCardView cardView = Instantiate(projectSceneCardViewPrefab, scenesContainer).GetComponent<ProjectSceneCardView>();
            cardView.Setup(scene,isSync);
            sceneCardViews.Add(cardView);
        }
    }

    internal void EditorButtonClicked()
    {
        OnEditorPressed?.Invoke(projectData);
    }

    public void SetThumbnail(string thumbnailId,string thumbnailEndpoint)
    {
        if (this.thumbnailId == thumbnailId)
            return;
        
        string projectThumbnailUrl = "";
        if (!string.IsNullOrEmpty(thumbnailId))
        {
            projectThumbnailUrl = BIWUrlUtils.GetBuilderProjecThumbnailUrl(thumbnailId,thumbnailEndpoint);
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
        projectSizeTxt.text = GetSizeText(rows,columns);
        ((IProjectCardView)this).searchInfo.SetSize(rows * columns);
    }

    internal string GetSizeText(int rows, int columns)
    {
        return (rows * ParcelSettings.PARCEL_SIZE) + "x" + (columns * ParcelSettings.PARCEL_SIZE)  + "m";
    }

    IEnumerator ChangeHeightAnimation(RectTransform rectTransform, float height)
    {
        float time = 0;

        Vector2 rect2 = rectTransform.sizeDelta;
        float objective  = rect2.y + height;

        while (time < 1)
        {
            time += Time.deltaTime * animationSpeed/scenesDeployedFromProject.Count;
            rect2.y = Mathf.Lerp(rect2.y,objective,time);
            rectTransform.sizeDelta = rect2;
            yield return null;
        }
    }
}