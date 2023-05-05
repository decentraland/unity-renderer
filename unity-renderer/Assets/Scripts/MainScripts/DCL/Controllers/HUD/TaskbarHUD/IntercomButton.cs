using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;
public class IntercomButton : MonoBehaviour
{
    private const string INTERCOM_URL = "https://intercom.decentraland.org/";

    [SerializeField] private Button button;

    private void Awake() =>
        button.onClick.AddListener(OpenIntercom);

    private void OpenIntercom() =>
        WebInterface.OpenURL(INTERCOM_URL);
}
