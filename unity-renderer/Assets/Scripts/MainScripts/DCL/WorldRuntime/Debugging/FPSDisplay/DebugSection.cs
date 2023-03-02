using UnityEngine;
using UnityEngine.UI;

public class DebugSection : MonoBehaviour
{
    [SerializeField] private Button closeSectionButton;
    [SerializeField] private GameObject sectionContent;

    private RectTransform rootTransform;
    private RectTransform closeSectionButtonTransform;
    void Start()
    {
        rootTransform = transform.parent.GetComponent<RectTransform>();
        closeSectionButton.onClick.AddListener(CloseSection);
        closeSectionButtonTransform = closeSectionButton.GetComponent<RectTransform>();
        CloseSection();
    }
    private void CloseSection()
    {
        sectionContent.SetActive(!sectionContent.activeSelf);
        closeSectionButtonTransform.Rotate(Vector3.forward,180);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootTransform);
    }

    private void OnDestroy()
    {
        closeSectionButton.onClick.RemoveAllListeners();
    }

}
