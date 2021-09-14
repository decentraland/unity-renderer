using UnityEngine;

[RequireComponent(typeof(ButtonComponentView))]
public class GoToLinkAction : MonoBehaviour
{
    public string urlToGo;

    private ButtonComponentView button;

    private void Start()
    {
        button = GetComponent<ButtonComponentView>();
        button.onClick.AddListener(GoToUrl);
    }

    private void OnDestroy() { button.onClick.RemoveAllListeners(); }

    internal void GoToUrl()
    {
        if (string.IsNullOrEmpty(urlToGo))
            return;

        Application.OpenURL(urlToGo);
    }
}