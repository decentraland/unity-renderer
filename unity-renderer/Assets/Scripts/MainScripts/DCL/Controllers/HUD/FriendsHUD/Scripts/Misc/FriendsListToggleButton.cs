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

    void Toggle()
    {
        containerRectTransform.gameObject.SetActive(!containerRectTransform.gameObject.activeSelf);
        toggleButtonIcon.localScale = new Vector3(toggleButtonIcon.localScale.x, -toggleButtonIcon.localScale.y, 1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
    }
}
