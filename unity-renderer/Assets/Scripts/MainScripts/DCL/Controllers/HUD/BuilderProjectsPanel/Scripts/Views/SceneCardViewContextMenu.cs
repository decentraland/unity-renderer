using System;
using UnityEngine;
using UnityEngine.UI;

internal class SceneCardViewContextMenu : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] internal GameObject headerContainer;

    [Header("Buttons")]
    [SerializeField] internal Button settingsButton;
    [SerializeField] internal Button duplicateAsProjectButton;
    [SerializeField] internal Button duplicateButton;
    [SerializeField] internal Button downloadButton;
    [SerializeField] internal Button shareButton;
    [SerializeField] internal Button unpublishButton;
    [SerializeField] internal Button deleteButton;
    [SerializeField] internal Button quitContributorButton;

    public event Action<string> OnSettingsPressed;
    public event Action<string> OnDuplicatePressed;
    public event Action<string> OnDownloadPressed;
    public event Action<string> OnSharePressed;
    public event Action<string> OnUnpublishPressed;
    public event Action<string> OnDeletePressed;
    public event Action<string> OnQuitContributorPressed;

    [System.Flags]
    public enum ConfigFlags
    {
        Settings = 1,
        DuplicateAsProject = 2,
        Duplicate = 4,
        Download = 8,
        Share = 16,
        Unpublish = 32,
        Delete = 64,
        QuitContributor = 128
    }

    private const ConfigFlags headerFlags = ConfigFlags.Settings;

    private string sceneId;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        settingsButton.onClick.AddListener(()=>
        {
            OnSettingsPressed?.Invoke(sceneId);
            Hide();
        });
        duplicateAsProjectButton.onClick.AddListener(()=>
        {
            OnDuplicatePressed?.Invoke(sceneId);
            Hide();
        });
        duplicateButton.onClick.AddListener(()=>
        {
            OnDuplicatePressed?.Invoke(sceneId);
            Hide();
        });
        downloadButton.onClick.AddListener(()=>
        {
            OnDownloadPressed?.Invoke(sceneId);
            Hide();
        });
        shareButton.onClick.AddListener(()=>
        {
            OnSharePressed?.Invoke(sceneId);
            Hide();
        });
        unpublishButton.onClick.AddListener(()=>
        {
            OnUnpublishPressed?.Invoke(sceneId);
            Hide();
        });
        deleteButton.onClick.AddListener(()=>
        {
            OnDeletePressed?.Invoke(sceneId);
            Hide();
        });
        quitContributorButton.onClick.AddListener(()=>
        {
            OnQuitContributorPressed?.Invoke(sceneId);
            Hide();
        });
    }

    private void Update()
    {
        HideIfClickedOutside();
    }

    public void Show(string sceneId, bool isSceneDeployed, bool isOwnerOrOperator, bool isContributor)
    {
        ConfigFlags config = isSceneDeployed?
            GetDeployedSceneConfig(isOwnerOrOperator, isContributor) :
            GetProjectSceneConfig(isOwnerOrOperator, isContributor);

        Show(sceneId, config);
    }

    public void Show(string sceneId, ConfigFlags configFlags)
    {
        this.sceneId = sceneId;
        Build(configFlags);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Build(ConfigFlags flags)
    {
        headerContainer.SetActive((flags & headerFlags) != 0);
        settingsButton.gameObject.SetActive((flags & ConfigFlags.Settings) != 0);
        duplicateAsProjectButton.gameObject.SetActive((flags & ConfigFlags.DuplicateAsProject) != 0);
        duplicateButton.gameObject.SetActive((flags & ConfigFlags.Duplicate) != 0);
        downloadButton.gameObject.SetActive((flags & ConfigFlags.Download) != 0);
        shareButton.gameObject.SetActive((flags & ConfigFlags.Share) != 0);
        unpublishButton.gameObject.SetActive((flags & ConfigFlags.Unpublish) != 0);
        deleteButton.gameObject.SetActive((flags & ConfigFlags.Delete) != 0);
        quitContributorButton.gameObject.SetActive((flags & ConfigFlags.QuitContributor) != 0);
    }

    private ConfigFlags GetDeployedSceneConfig(bool isOwnerOrOperator, bool isContributor)
    {
        ConfigFlags config = ConfigFlags.DuplicateAsProject | ConfigFlags.Download;
        if (isOwnerOrOperator)
        {
            config |= ConfigFlags.Settings | ConfigFlags.Unpublish;
        }
        else if (isContributor)
        {
            config |= ConfigFlags.QuitContributor;
        }

        return config;
    }

    private ConfigFlags GetProjectSceneConfig(bool isOwnerOrOperator, bool isContributor)
    {
        ConfigFlags config = ConfigFlags.Duplicate | ConfigFlags.Download | ConfigFlags.Share;
        if (isOwnerOrOperator)
        {
            config |= ConfigFlags.Settings | ConfigFlags.Delete;
        }
        else if (isContributor)
        {
            config |= ConfigFlags.QuitContributor;
        }

        return config;
    }

    private void HideIfClickedOutside()
    {
        if (Input.GetMouseButtonDown(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            Hide();
        }
    }
}
