using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

public class GraphicCardNotification : Notification
{
    // Filling this with the URL will automatically make the button visible
    private const string MORE_INFO_URL = "https://docs.decentraland.org/decentraland/hardware-acceleration/";
    private const string DONT_SHOW_GRAPHIC_CARD_POPUP_KEY = "DONT_SHOW_GRAPHIC_CARD_POPUP";
    [SerializeField] private Button moreInfoButton;
    [SerializeField] private Toggle dontShowAgain;

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

    protected override void Dismiss()
    {
        PlayerPrefsUtils.SetInt(DONT_SHOW_GRAPHIC_CARD_POPUP_KEY, dontShowAgain.isOn ? 1 : 0);
        base.Dismiss();
    }

    public static bool CanShowGraphicCardPopup() => PlayerPrefs.GetInt(DONT_SHOW_GRAPHIC_CARD_POPUP_KEY, 0) == 0;
}
