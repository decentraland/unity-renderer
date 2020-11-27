using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

public class GraphicCardNotification : Notification
{
    // Filling this with the URL will automatically make the button visible
    private const string MORE_INFO_URL = "https://docs.decentraland.org/decentraland/hardware-acceleration/";
    [SerializeField] private Button moreInfoButton;

    private void Awake()
    {
        moreInfoButton.gameObject.SetActive(!string.IsNullOrEmpty(MORE_INFO_URL));
        moreInfoButton.onClick.AddListener(OpenMoreInfoUrl);
    }

    private void OpenMoreInfoUrl()
    {
        WebInterface.OpenURL(MORE_INFO_URL);
        Dismiss();
    }
}
