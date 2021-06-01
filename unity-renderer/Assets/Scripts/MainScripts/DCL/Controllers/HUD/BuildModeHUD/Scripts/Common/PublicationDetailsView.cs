using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPublicationDetailsView
{
    event Action OnCancel;
    event Action OnPublish;

    void SetActive(bool isActive);
    void Cancel();
    void Publish();
}

public class PublicationDetailsView : MonoBehaviour, IPublicationDetailsView
{
    public event Action OnCancel;
    public event Action OnPublish;

    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button publishButton;
    [SerializeField] internal TMP_InputField sceneNameInput;
    [SerializeField] internal TMP_InputField sceneDescriptionInput;

    private const string VIEW_PATH = "Common/PublicationDetailsView";

    internal static PublicationDetailsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PublicationDetailsView>();
        view.gameObject.name = "_PublicationDetailsView";

        return view;
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(Cancel);
        cancelButton.onClick.AddListener(Cancel);
        publishButton.onClick.AddListener(Publish);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(Cancel);
        cancelButton.onClick.RemoveListener(Cancel);
        publishButton.onClick.RemoveListener(Publish);
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void Cancel() { OnCancel?.Invoke(); }

    public void Publish() { OnPublish?.Invoke(); }
}