using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used for customize the color of the text and icon of a UI Component.
/// </summary>
[RequireComponent(typeof(BaseComponentView))]
public class UIHelper_ChangeTextAndIconColorOnFocus : MonoBehaviour
{
    [SerializeField] internal TMP_Text textToChange;
    [SerializeField] internal Image iconToChange;
    [SerializeField] internal Color onFocusColor;
    [SerializeField] internal Color onLoseFocusColor;

    internal BaseComponentView componentView;

    private void Awake()
    {
        componentView = GetComponent<BaseComponentView>();
    }

    private void Start()
    {
        componentView.onFocused += ComponentView_onFocused;
        ComponentView_onFocused(componentView.isFocused);
    }

    private void OnDestroy()
    {
        componentView.onFocused -= ComponentView_onFocused;
    }

    internal void ComponentView_onFocused(bool isFocused)
    {
        textToChange.color = isFocused ? onFocusColor : onLoseFocusColor;
        iconToChange.color = isFocused ? onFocusColor : onLoseFocusColor;
    }
}
