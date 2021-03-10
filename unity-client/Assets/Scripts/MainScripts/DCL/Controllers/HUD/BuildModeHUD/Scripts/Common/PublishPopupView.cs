using TMPro;
using UnityEngine;

public interface IPublishPopupView
{
    void PublishEnd(string message);
    void PublishStart();
}

public class PublishPopupView : MonoBehaviour, IPublishPopupView
{
    [SerializeField] internal GameObject publishingGO;
    [SerializeField] internal GameObject publishingFinishedGO;
    [SerializeField] internal TextMeshProUGUI publishStatusTxt;

    private const string VIEW_PATH = "Common/PublishPopupView";

    internal static PublishPopupView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PublishPopupView>();
        view.gameObject.name = "_PublishPopupView";

        return view;
    }

    public void PublishStart()
    {
        gameObject.SetActive(true);
        publishingGO.SetActive(true);
        publishingFinishedGO.SetActive(false);
    }

    public void PublishEnd(string message)
    {
        publishingGO.SetActive(false);
        publishingFinishedGO.SetActive(true);
        publishStatusTxt.text = message;
    }
}
