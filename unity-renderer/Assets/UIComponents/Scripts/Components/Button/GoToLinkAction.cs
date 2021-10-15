using UnityEngine;

[RequireComponent(typeof(ButtonComponentView))]
public class GoToLinkAction : MonoBehaviour
{
    public string urlToGo;

    internal ButtonComponentView button;

    private void Awake()
    {
        button = GetComponent<ButtonComponentView>();

        if (button != null)
            button.onClick.AddListener(GoToUrl);
    }

    internal void GoToUrl()
    {
        if (string.IsNullOrEmpty(urlToGo))
            return;

        Application.OpenURL(urlToGo);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}