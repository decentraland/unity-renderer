using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// It represents the context menu for a Portable Experience item.
/// </summary>
public class PortableExperienceContextMenu : MonoBehaviour
{
    [SerializeField]
    private ShowHideAnimator menuAnimator;

    [SerializeField]
    private TextMeshProUGUI menuTitle;

    [SerializeField]
    private Button killButton;

    private TaskbarHUDController taskbarController;
    private string portableExperienceId;

    internal void ConfigureMenu(string portableExperienceId, string portableExperienceName, TaskbarHUDController taskbarController)
    {
        this.portableExperienceId = portableExperienceId;
        this.taskbarController = taskbarController;

        ShowMenu(false, true);

        menuTitle.text = portableExperienceName;
        killButton.onClick.AddListener(KillPortableExperience);
    }

    private void OnDestroy()
    {
        killButton.onClick.RemoveListener(KillPortableExperience);
    }

    internal void ShowMenu(bool visible, bool instant = false)
    {
        if (visible)
        {
            if (!menuAnimator.gameObject.activeInHierarchy)
            {
                menuAnimator.gameObject.SetActive(true);
            }

            menuAnimator.Show(instant);
        }
        else
        {
            if (!menuAnimator.gameObject.activeInHierarchy)
            {
                menuAnimator.gameObject.SetActive(false);
            }
            else
            {
                menuAnimator.Hide(instant);
            }
        }
    }

    private void KillPortableExperience()
    {
        taskbarController.KillPortableExperience(portableExperienceId);
        ShowMenu(false);
    }
}
