using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListToggleButton : MonoBehaviour
{
    public bool toggleOnAwake = false;
    public Button toggleButton;
    public Transform toggleButtonIcon;
    public RectTransform containerRectTransform;

    void Awake()
    {
        toggleButton.onClick.AddListener(Toggle);

        if (toggleOnAwake)
            Toggle();
    }

    public void Toggle(bool toggled)
    {
        containerRectTransform.gameObject.SetActive(toggled);
        var absScale = Mathf.Abs(toggleButtonIcon.localScale.y);
        var scale = toggled ? absScale : -absScale;
        toggleButtonIcon.localScale = new Vector3(toggleButtonIcon.localScale.x, scale, 1f);
        Utils.ForceRebuildLayoutImmediate(containerRectTransform);
    }

    private void Toggle()
    {
        Toggle(!containerRectTransform.gameObject.activeSelf);
    }
}