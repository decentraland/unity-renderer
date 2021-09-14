using UnityEngine;

[RequireComponent(typeof(ButtonComponentView))]
public class GoToLinkAction : MonoBehaviour
{
    public string urlToGo;

    private ButtonComponentView button;

    private void Start()
    {
        button = GetComponent<ButtonComponentView>();
        button.onButtonClick.AddListener(GoToUrl);
    }

    private void OnDestroy() { button.onButtonClick.RemoveAllListeners(); }

    internal void GoToUrl()
    {
        if (string.IsNullOrEmpty(urlToGo))
            return;

        Application.OpenURL(urlToGo);
    }
}