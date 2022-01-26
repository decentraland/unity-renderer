using UnityEngine;
using UnityEngine.UI;

internal class PreviewMenuView : MonoBehaviour
{
    [SerializeField] private Button menuButton;
    [SerializeField] private GameObject contentContainer;
    [SerializeField] private Transform menuList;

    private void Awake()
    {
        menuButton.onClick.AddListener(() =>
        {
            SetVisible(!contentContainer.activeSelf);
        });
    }

    public void AddMenuItem(Transform item)
    {
        item.SetParent(menuList);
        item.localScale = Vector3.one;
    }

    public void SetVisible(bool on)
    {
        contentContainer.SetActive(on);
    }
}