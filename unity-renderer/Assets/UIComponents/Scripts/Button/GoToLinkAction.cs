using UnityEngine;

[RequireComponent(typeof(ButtonComponentView))]
public class GoToLinkAction : MonoBehaviour
{
    public string urlToGo;

    private ButtonComponentView button;

    private void Awake()
    {
        button = GetComponent<ButtonComponentView>();
        button.SetOnClickAction(GoToUrl);
    }

    internal void GoToUrl()
    {
        if (string.IsNullOrEmpty(urlToGo))
            return;

        Application.OpenURL(urlToGo);
    }
}