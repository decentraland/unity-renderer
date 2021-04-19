using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// It represents a Portable Experience item in the taskbar.
/// </summary>
public class PortableExperienceTaskbarItem : MonoBehaviour
{
    [SerializeField]
    private TaskbarButton button;

    [SerializeField]
    private TextMeshProUGUI tooltipText;

    [SerializeField]
    private CanvasGroup tooltipTextContainerCanasGroup;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private GameObject loading;

    [SerializeField]
    private PortableExperienceContextMenu contextMenu;

    public TaskbarButton mainButton { get => button; }

    internal void ConfigureItem(
        string portableExperienceId,
        string portableExperienceName,
        string portableExperienceIconUrl,
        TaskbarHUDController taskbarController)
    {
        tooltipText.text = portableExperienceName;
        button.Initialize();
        contextMenu.ConfigureMenu(portableExperienceId, portableExperienceName, taskbarController);

        if (!string.IsNullOrEmpty(portableExperienceIconUrl))
        {
            icon.enabled = false;
            loading.SetActive(true);
            ThumbnailsManager.GetThumbnail(portableExperienceIconUrl, OnIconReady);
        }
    }

    private void OnIconReady(Asset_Texture iconAsset)
    {
        if (iconAsset != null)
        {
            icon.sprite = ThumbnailsManager.CreateSpriteFromTexture(iconAsset.texture);
            icon.enabled = true;
            loading.SetActive(false);
        }
    }

    internal void ShowContextMenu(bool visible)
    {
        tooltipTextContainerCanasGroup.alpha = visible ? 0f : 1f;
        contextMenu.ShowMenu(visible);
    }
}
