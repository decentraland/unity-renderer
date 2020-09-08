using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarMoreMenu : MonoBehaviour
{
    [Header("Menu Animation")]
    [SerializeField] internal ShowHideAnimator moreMenuAnimator;

    [Header("Collapse Button Config")]
    [SerializeField] internal Button collapseBarButton;
    [SerializeField] internal GameObject collapseIcon;
    [SerializeField] internal GameObject collapseText;
    [SerializeField] internal GameObject expandIcon;
    [SerializeField] internal GameObject expandText;

    [Header("Hide UI Button Config")]
    [SerializeField] internal Button hideUIButton;

    [Header("Controls Button Config")]
    [SerializeField] internal Button controlsButton;
    [SerializeField] internal InputAction_Trigger controlsToggleAction;

    private TaskbarHUDView view;

    public void Initialize(TaskbarHUDView view)
    {
        this.view = view;

        collapseBarButton.gameObject.SetActive(true);
        hideUIButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(false);

        collapseBarButton.onClick.AddListener(() =>
        {
            ToggleCollapseBar();
        });

        hideUIButton.onClick.AddListener(() =>
        {
            ToggleHideUI();
        });
    }

    internal void ActivateControlsButton()
    {
        controlsButton.gameObject.SetActive(true);

        controlsButton.onClick.AddListener(() =>
        {
            ToggleControls();
        });
    }

    internal void ShowMoreMenu(bool visible, bool instant = false)
    {
        if (visible)
            moreMenuAnimator.Show(instant);
        else
            moreMenuAnimator.Hide(instant);
    }

    private void ToggleCollapseBar()
    {
        view.ShowBar(!view.isBarVisible);
        ShowMoreMenu(false);

        collapseIcon.SetActive(view.isBarVisible);
        collapseText.SetActive(view.isBarVisible);
        expandIcon.SetActive(!view.isBarVisible);
        expandText.SetActive(!view.isBarVisible);

        view.moreButton.SetToggleState(false);
    }

    private void ToggleHideUI()
    {
        CommonScriptableObjects.allUIHidden.Set(!CommonScriptableObjects.allUIHidden.Get());
        view.moreButton.SetToggleState(false);
    }

    private void ToggleControls()
    {
        controlsToggleAction.RaiseOnTriggered();
        view.moreButton.SetToggleState(false);
    }
}
