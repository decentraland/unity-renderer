using UnityEngine;
using UnityEngine.UI;

internal class PreviewMenuView : MonoBehaviour
{
    [SerializeField] internal Button menuButton;
    [SerializeField] internal GameObject contentContainer;
    [SerializeField] internal Transform menuList;

    private bool isDestroyed;

    private void Awake()
    {
        menuButton.onClick.AddListener(() =>
        {
            SetVisible(!contentContainer.activeSelf);
        });
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
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