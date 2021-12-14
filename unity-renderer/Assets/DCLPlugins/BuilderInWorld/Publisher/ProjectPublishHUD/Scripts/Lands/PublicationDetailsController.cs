using System;
using DCL.Builder;
using UnityEngine;

public interface IPublicationDetailsController
{
    event Action OnCancel;
    event Action<IBuilderScene> OnConfirm;

    /// <summary>
    /// Init the controller with the default view
    /// </summary>
    void Initialize();

    /// <summary>
    /// Init the view with an specific view
    /// </summary>
    /// <param name="publicationDetailsView"></param>
    void Initialize(IPublicationDetailsView publicationDetailsView);

    /// <summary>
    /// This will start the flow of the publishing
    /// </summary>
    /// <param name="scene"></param>
    void StartPublishFlow(IBuilderScene scene);
    void Dispose();
    void SetPublicationScreenshot(Texture2D sceneScreenshot);
}

public class PublicationDetailsController : IPublicationDetailsController
{
    public event Action OnCancel;
    public event Action<IBuilderScene> OnConfirm;

    internal const string PREFAB_PATH = "Land/LandPublisherView";
    internal const string DEFAULT_SCENE_NAME = "My new place";
    internal const string DEFAULT_SCENE_DESC = "";

    internal IPublicationDetailsView publicationDetailsView;
    internal bool isValidated = false;
    internal IBuilderScene sceneToPublish;

    public void Initialize()
    {
        var template = Resources.Load<PublicationDetailsView>(PREFAB_PATH);
        Initialize(GameObject.Instantiate(template));
    }

    public void Initialize(IPublicationDetailsView publicationDetailsView)
    {
        this.publicationDetailsView = publicationDetailsView;

        publicationDetailsView.OnCancel += Cancel;
        publicationDetailsView.OnPublish += Publish;
        publicationDetailsView.OnSceneNameChange += ValidatePublicationInfo;

        SetDefaultPublicationInfo();
        ValidatePublicationInfo(publicationDetailsView.currentSceneName);
    }

    public void Dispose()
    {
        publicationDetailsView.OnCancel -= Cancel;
        publicationDetailsView.OnPublish -= Publish;
        publicationDetailsView.OnSceneNameChange -= ValidatePublicationInfo;
    }

    public void SetActive(bool isActive) { publicationDetailsView.SetActive(isActive); }

    public void Cancel()
    {
        SetActive(false);
        OnCancel?.Invoke();
    }

    public void StartPublishFlow(IBuilderScene scene)
    {
        sceneToPublish = scene;
        SetActive(true);
    }

    public void Publish()
    {
        if (!isValidated)
            return;

        sceneToPublish.manifest.project.title = publicationDetailsView.GetSceneName();
        sceneToPublish.manifest.project.description = publicationDetailsView.GetSceneDescription();

        SetActive(false);
        OnConfirm?.Invoke(sceneToPublish);
    }

    public void ValidatePublicationInfo(string sceneName)
    {
        isValidated = sceneName.Length > 0;
        publicationDetailsView.SetSceneNameValidationActive(!isValidated);
        publicationDetailsView.SetPublishButtonActive(isValidated);
    }

    public void SetDefaultPublicationInfo()
    {
        publicationDetailsView.SetSceneName(DEFAULT_SCENE_NAME);
        publicationDetailsView.SetSceneDescription(DEFAULT_SCENE_DESC);
    }

    public void SetCustomPublicationInfo(IBuilderScene scene)
    {
        publicationDetailsView.SetSceneName(scene.manifest.project.title);
        publicationDetailsView.SetSceneDescription(scene.manifest.project.description);
        SetPublicationScreenshot(scene.sceneScreenshotTexture);
    }

    public void SetPublicationScreenshot(Texture2D sceneScreenshot) { publicationDetailsView.SetPublicationScreenshot(sceneScreenshot); }
}