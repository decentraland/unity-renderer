using System;

public interface IPublicationDetailsController
{
    event Action OnCancel;
    event Action OnPublish;

    void Initialize(IPublicationDetailsView publicationDetailsView);
    void Dispose();
    void SetActive(bool isActive);
    void Cancel();
    void Publish();
}

public class PublicationDetailsController : IPublicationDetailsController
{
    public event Action OnCancel;
    public event Action OnPublish;

    internal IPublicationDetailsView publicationDetailsView;

    public void Initialize(IPublicationDetailsView publicationDetailsView)
    {
        this.publicationDetailsView = publicationDetailsView;

        publicationDetailsView.OnCancel += Cancel;
        publicationDetailsView.OnPublish += Publish;
    }

    public void Dispose()
    {
        publicationDetailsView.OnCancel -= Cancel;
        publicationDetailsView.OnPublish -= Publish;
    }

    public void SetActive(bool isActive) { publicationDetailsView.SetActive(isActive); }

    public void Cancel()
    {
        SetActive(false);
        OnCancel?.Invoke();
    }

    public void Publish()
    {
        SetActive(false);
        OnPublish?.Invoke();
    }
}