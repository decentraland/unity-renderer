using System;

public interface IPublicationDetailsController
{
    event Action OnCancel;
    event Action<string, string> OnPublish;

    void Initialize(IPublicationDetailsView publicationDetailsView);
    void Dispose();
    void SetActive(bool isActive);
    void Cancel();
    void Publish(string sceneName, string sceneDescription);
    void ValidatePublicationInfo(string sceneName);
    void SetDefaultPublicationInfo();
}

public class PublicationDetailsController : IPublicationDetailsController
{
    public event Action OnCancel;
    public event Action<string, string> OnPublish;

    internal const string DEFAULT_SCENE_NAME = "My new place";

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
        OnPublish?.Invoke(sceneName, sceneDescription);
    }

    public void ValidatePublicationInfo(string sceneName)
    {
        isValidated = sceneName.Length > 0;
        publicationDetailsView.SetSceneNameValidationActive(!isValidated);
        publicationDetailsView.SetPublishButtonActive(isValidated);
    }

    public void SetDefaultPublicationInfo() { publicationDetailsView.SetSceneName(DEFAULT_SCENE_NAME); }
}