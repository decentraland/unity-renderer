using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WelcomeHUDView : MonoBehaviour
{
    private const string PREFAB_PATH_WITH_WALLET = "WelcomeHUD";
    private const string PREFAB_PATH_NO_WALLET = "WelcomeNoWalletHUD";

    [SerializeField] internal TextMeshProUGUI headerText1;
    [SerializeField] internal TextMeshProUGUI headerText2;
    [SerializeField] internal TextMeshProUGUI bodyText;
    [SerializeField] internal TextMeshProUGUI buttonText;

    [SerializeField] internal Button_OnPointerDown confirmButton;
    [SerializeField] internal Button_OnPointerDown closeButton;

    UnityAction OnConfirmButtonPressed;
    UnityAction OnCloseButtonPressed;

    public static WelcomeHUDView CreateView(bool hasWallet)
    {
        GameObject prefab;

        if (hasWallet)
            prefab = Resources.Load<GameObject>(PREFAB_PATH_WITH_WALLET);
        else
            prefab = Resources.Load<GameObject>(PREFAB_PATH_NO_WALLET);

        return Instantiate(prefab).GetComponent<WelcomeHUDView>();
    }

    public void Initialize(UnityAction OnConfirm, UnityAction OnClose)
    {
        OnConfirmButtonPressed = OnConfirm;
        OnCloseButtonPressed = OnClose;

        confirmButton.onPointerDown += OnConfirmButtonPressed;
        closeButton.onPointerDown += OnCloseButtonPressed;
    }

    public void CleanUp()
    {
        confirmButton.onPointerDown -= OnConfirmButtonPressed;
        closeButton.onPointerDown -= OnCloseButtonPressed;
    }
}
