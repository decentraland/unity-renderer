using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHUD_DialogBox : MonoBehaviour
{
    public TextMeshProUGUI dialogText;
    public Button cancelButton;
    public Button confirmButton;

    public void Awake()
    {

    }

    internal void SetText(string text)
    {
        dialogText.text = text;
    }

    internal void Show(System.Action onConfirm = null, System.Action onCancel = null)
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => { onConfirm?.Invoke(); Hide(); });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => { onCancel?.Invoke(); Hide(); });

        gameObject.SetActive(true);
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }
}
