using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISceneCatalogView
{
    CatalogAssetPackListView catalogAssetPackList { get; }
    CatalogGroupListView catalogGroupList { get; }
    Toggle category { get; }
    Toggle favorites { get; }
    Toggle assetPack { get; }
    IBIWSearchBarView searchBarView { get; }

    event Action OnHideCatalogClicked;
    event Action OnSceneCatalogBack;

    void Back();
    void CloseCatalog();
    bool IsCatalogOpen();
    bool IsCatalogExpanded();
    void OnHideCatalogClick();
    void SetCatalogTitle(string text);
    void ToggleCatalogExpanse();
    void SetActive(bool isActive);
    void ShowBackButton(bool isActive);
}

public class SceneCatalogView : MonoBehaviour, ISceneCatalogView
{
    public CatalogAssetPackListView catalogAssetPackList => catalogAssetPackListView;
    public CatalogGroupListView catalogGroupList => catalogGroupListView;
    public Toggle category => categoryToggle;
    public Toggle favorites => favoritesToggle;
    public Toggle assetPack => assetPackToggle;
    public IBIWSearchBarView searchBarView  => biwSearchBarView;

    public event Action OnHideCatalogClicked;
    public event Action OnSceneCatalogBack;

    [Header("Prefab References")]
    [SerializeField] internal TextMeshProUGUI catalogTitleTxt;

    [SerializeField] internal CatalogAssetPackListView catalogAssetPackListView;
    [SerializeField] internal CatalogGroupListView catalogGroupListView;
    [SerializeField] internal TMP_InputField searchInputField;
    [SerializeField] internal Toggle categoryToggle;
    [SerializeField] internal Toggle favoritesToggle;
    [SerializeField] internal Toggle assetPackToggle;
    [SerializeField] internal Button hideCatalogBtn;
    [SerializeField] internal Button backgBtn;
    [SerializeField] internal Button toggleCatalogBtn;
    [SerializeField] internal BIWSearchBarView biwSearchBarView;

    [Header("Catalog RectTransforms")]
    [SerializeField] internal RectTransform panelRT;

    [SerializeField] internal RectTransform headerRT;
    [SerializeField] internal RectTransform searchBarRT;
    [SerializeField] internal RectTransform assetPackRT;
    [SerializeField] internal RectTransform categoryRT;

    [Header("MinSize Catalog RectTransforms")]
    [SerializeField] internal RectTransform panelMinSizeRT;

    [SerializeField] internal RectTransform headerMinSizeRT;
    [SerializeField] internal RectTransform searchBarMinSizeRT;
    [SerializeField] internal RectTransform assetPackMinSizeRT;

    [Header("MaxSize Catalog RectTransforms")]
    [SerializeField] internal RectTransform panelMaxSizeRT;

    [SerializeField] internal RectTransform headerMaxSizeRT;
    [SerializeField] internal RectTransform searchBarMaxSizeRT;
    [SerializeField] internal RectTransform assetPackMaxSizeRT;

    internal bool isCatalogExpanded = false;
    internal bool isClosing = false;

    private const string VIEW_PATH = "Common/SceneCatalogView";

    internal static SceneCatalogView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<SceneCatalogView>();
        view.gameObject.name = "_SceneCatalogView";

        return view;
    }

    private void Awake()
    {
        hideCatalogBtn.onClick.AddListener(OnHideCatalogClick);
        backgBtn.onClick.AddListener(Back);
        toggleCatalogBtn.onClick.AddListener(ToggleCatalogExpanse);
    }

    private void OnDestroy()
    {
        hideCatalogBtn.onClick.RemoveListener(OnHideCatalogClick);
        backgBtn.onClick.RemoveListener(Back);
        toggleCatalogBtn.onClick.RemoveListener(ToggleCatalogExpanse);
    }

    private void OnEnable() { AudioScriptableObjects.dialogOpen.Play(); }

    private void OnDisable() { AudioScriptableObjects.dialogClose.Play(); }

    public void ToggleCatalogExpanse()
    {
        if (isCatalogExpanded)
        {
            BIWUtils.CopyRectTransform(panelRT, panelMinSizeRT);
            BIWUtils.CopyRectTransform(headerRT, headerMinSizeRT);
            BIWUtils.CopyRectTransform(searchBarRT, searchBarMinSizeRT);
            BIWUtils.CopyRectTransform(assetPackRT, assetPackMinSizeRT);
            BIWUtils.CopyRectTransform(categoryRT, assetPackMinSizeRT);
            AudioScriptableObjects.dialogClose.Play();
        }
        else
        {
            BIWUtils.CopyRectTransform(panelRT, panelMaxSizeRT);
            BIWUtils.CopyRectTransform(headerRT, headerMaxSizeRT);
            BIWUtils.CopyRectTransform(searchBarRT, searchBarMaxSizeRT);
            BIWUtils.CopyRectTransform(assetPackRT, assetPackMaxSizeRT);
            BIWUtils.CopyRectTransform(categoryRT, assetPackMaxSizeRT);
            AudioScriptableObjects.dialogOpen.Play();
        }

        isCatalogExpanded = !isCatalogExpanded;
    }

    public void OnHideCatalogClick() { OnHideCatalogClicked?.Invoke(); }

    public void ShowBackButton(bool isActive) { backgBtn.gameObject.SetActive(isActive); }

    public void Back() { OnSceneCatalogBack?.Invoke(); }

    public void SetCatalogTitle(string text) { catalogTitleTxt.text = text; }

    public bool IsCatalogOpen() { return gameObject.activeSelf; }

    public bool IsCatalogExpanded() { return isCatalogExpanded; }

    public void CloseCatalog()
    {
        if (gameObject.activeSelf)
            CoroutineStarter.Start(CloseCatalogAfterOneFrame());
    }

    internal IEnumerator CloseCatalogAfterOneFrame()
    {
        isClosing = true;
        yield return null;
        gameObject.SetActive(false);
        isClosing = false;
    }

    public void SetActive(bool isActive)
    {
        if (isActive && isClosing)
            CoroutineStarter.Start(OpenCatalogWhenIsAlreadyClosed());
        else
            gameObject.SetActive(isActive);
    }

    internal IEnumerator OpenCatalogWhenIsAlreadyClosed()
    {
        yield return new WaitUntil(() => !isClosing);
        gameObject.SetActive(true);
    }
}