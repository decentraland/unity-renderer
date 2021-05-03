using DCL;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
internal class LeftMenuSettingsViewReferences
{
    public TextMeshProUGUI titleLabel;
    public RawImageFillParent thumbnail;
    public GameObject coordsContainer;
    public GameObject sizeContainer;
    public GameObject entitiesCountContainer;
    public TextMeshProUGUI coordsText;
    public TextMeshProUGUI sizeText;
    public TextMeshProUGUI entitiesCountText;
    public GameObject avatarThumbnailContainer;
    public RawImage avatarImage;
    public TextMeshProUGUI authorNameText;
    public LeftMenuButtonToggleView adminsMenuToggle;
}

internal class LeftMenuSettingsViewHandler : IDisposable
{
    internal const string PROJECT_TITLE = "Project settings";
    internal const string SCENE_TITLE = "Scene settings";

    private readonly LeftMenuSettingsViewReferences viewReferences;
    private readonly IScenesViewController scenesViewController;
    private readonly Texture defaultThumbnail;

    private AssetPromise_Texture promiseAvatarThumbnail;
    private AssetPromise_Texture promiseSceneThumbnail;

    public LeftMenuSettingsViewHandler(LeftMenuSettingsViewReferences viewReferences, IScenesViewController scenesViewController)
    {
        this.viewReferences = viewReferences;
        this.scenesViewController = scenesViewController;
        
        defaultThumbnail = viewReferences.thumbnail.texture;

        scenesViewController.OnSceneSelected += OnSelectScene;
    }

    public void Dispose()
    {
        scenesViewController.OnSceneSelected -= OnSelectScene;
        
        if (promiseAvatarThumbnail != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(promiseAvatarThumbnail);
        }
        if (promiseSceneThumbnail != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(promiseSceneThumbnail);
        }

        promiseAvatarThumbnail = null;
        promiseSceneThumbnail = null;
    }
    
    void SetProjectData(ISceneData sceneData)
    {
        viewReferences.titleLabel.text = sceneData.isDeployed ? SCENE_TITLE : PROJECT_TITLE;
        viewReferences.coordsContainer.SetActive(sceneData.isDeployed);
        viewReferences.sizeContainer.SetActive(!sceneData.isDeployed);
        viewReferences.entitiesCountContainer.SetActive(!sceneData.isDeployed);
        viewReferences.coordsText.text = $"{sceneData.coords.x},{sceneData.coords.y}";
        viewReferences.sizeText.text = $"{sceneData.size.x},{sceneData.size.y}m";
        viewReferences.entitiesCountText.text = sceneData.entitiesCount.ToString();
        viewReferences.authorNameText.text = sceneData.authorName;
        viewReferences.adminsMenuToggle.gameObject.SetActive(sceneData.isDeployed);

        SetThumbnails(sceneData);
    }

    void SetThumbnails(ISceneData sceneData)
    {
        if (string.IsNullOrEmpty(sceneData.authorThumbnail))
        {
            viewReferences.avatarThumbnailContainer.SetActive(false);
        }
        else
        {
            if (promiseAvatarThumbnail != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(promiseAvatarThumbnail);
            }

            promiseAvatarThumbnail = new AssetPromise_Texture(sceneData.authorThumbnail);
            promiseAvatarThumbnail.OnFailEvent += asset => viewReferences.avatarThumbnailContainer.SetActive(false);
            promiseAvatarThumbnail.OnSuccessEvent += asset =>
            {
                viewReferences.avatarThumbnailContainer.SetActive(true);
                viewReferences.avatarImage.texture = asset.texture;
            };
            AssetPromiseKeeper_Texture.i.Keep(promiseAvatarThumbnail);
        }

        if (string.IsNullOrEmpty(sceneData.thumbnailUrl))
        {
            viewReferences.thumbnail.texture = defaultThumbnail;
        }
        else
        {
            if (promiseSceneThumbnail != null)
            {
                AssetPromiseKeeper_Texture.i.Forget(promiseSceneThumbnail);
            }

            promiseSceneThumbnail = new AssetPromise_Texture(sceneData.thumbnailUrl);
            promiseSceneThumbnail.OnFailEvent += asset => viewReferences.thumbnail.texture = defaultThumbnail;
            promiseSceneThumbnail.OnSuccessEvent += asset =>
            {
                viewReferences.thumbnail.texture = asset.texture;
            };
            AssetPromiseKeeper_Texture.i.Keep(promiseSceneThumbnail);
        }
    }
    
    private void OnSelectScene(ISceneCardView sceneCardView)
    {
        SetProjectData(sceneCardView.sceneData);
    }
}