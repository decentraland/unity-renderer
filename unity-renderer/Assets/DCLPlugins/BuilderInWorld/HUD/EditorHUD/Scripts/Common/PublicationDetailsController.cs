using System;
using UnityEngine;

public interface IPublicationDetailsController
{
    event Action OnCancel;
    event Action OnConfirm;

    void Initialize(IPublicationDetailsView publicationDetailsView);
    void Dispose();
    void SetActive(bool isActive);
    void Cancel();
    void Publish(string sceneName, string sceneDescription);
    void ValidatePublicationInfo(string sceneName);
    void SetDefaultPublicationInfo();
    void SetCustomPublicationInfo(string sceneName, string sceneDescription);
    void SetPublicationScreenshot(Texture2D sceneScreenshot);
    string GetSceneName();
    string GetSceneDescription();
    Texture2D GetSceneScreenshotTexture();
}

public class PublicationDetailsController : IPublicationDetailsController
{
    public event Action OnCancel;
    public event Action OnConfirm;

    internal const string DEFAULT_SCENE_NAME = "My new place";
    internal const string DEFAULT_SCENE_DESC = "";

    internal IPublicationDetailsView publicationDetailsView;
    internal bool isValidated = false;

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

    public void Publish(string sceneName, string sceneDescription)
    {
        if (!isValidated)
            return;

        SetActive(false);
        OnConfirm?.Invoke();
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

    public void SetCustomPublicationInfo(string sceneName, string sceneDescription)
    {
        publicationDetailsView.SetSceneName(sceneName);
        publicationDetailsView.SetSceneDescription(sceneDescription);
    }

    public void SetPublicationScreenshot(Texture2D sceneScreenshot) { publicationDetailsView.SetPublicationScreenshot(sceneScreenshot); }

    public string GetSceneName() { return publicationDetailsView.GetSceneName(); }

    public string GetSceneDescription() { return publicationDetailsView.GetSceneDescription(); }

    public Texture2D GetSceneScreenshotTexture() { return publicationDetailsView.GetSceneScreenshotTexture(); }
}