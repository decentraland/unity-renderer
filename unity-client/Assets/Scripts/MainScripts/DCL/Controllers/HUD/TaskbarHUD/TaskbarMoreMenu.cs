using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarMoreMenu : MonoBehaviour
{
    [Header("Menu Animation")] [SerializeField]
    internal ShowHideAnimator moreMenuAnimator;

    [Header("Collapse Button Config")] [SerializeField]
    internal Button collapseBarButton;

    [SerializeField] internal GameObject collapseIcon;
    [SerializeField] internal GameObject collapseText;
    [SerializeField] internal GameObject expandIcon;
    [SerializeField] internal GameObject expandText;

    [Header("Other Buttons Config")] [SerializeField]
    internal Button hideUIButton;

    [SerializeField] internal Button controlsButton;
    [SerializeField] internal InputAction_Trigger controlsToggleAction;
    [SerializeField] internal Button helpAndSupportButton;
    [SerializeField] internal Button tutorialButton;
    [SerializeField] internal Button dayModeButton;
    [SerializeField] internal Button nightModeButton;

    private TaskbarHUDView view;

    public event System.Action<bool> OnMoreMenuOpened;
    public event System.Action OnRestartTutorial;

    public void Initialize(TaskbarHUDView view)
    {
        this.view = view;

        CommonScriptableObjects.tutorialActive.OnChange += TutorialActive_OnChange;

        collapseBarButton.gameObject.SetActive(true);
        hideUIButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(false);
        helpAndSupportButton.gameObject.SetActive(false);
        tutorialButton.gameObject.SetActive(true);

        RenderProfileManifest.i.OnChangeProfile += OnChangeProfile;
        OnChangeProfile(RenderProfileManifest.i.currentProfile);

        dayModeButton.onClick.AddListener(() =>
        {
            RenderProfileManifest.i.currentProfile = RenderProfileManifest.i.defaultProfile;
            RenderProfileManifest.i.currentProfile.Apply();
            view.moreButton.SetToggleState(false);
        });

        nightModeButton.onClick.AddListener(() =>
        {
            RenderProfileManifest.i.currentProfile = RenderProfileManifest.i.halloweenProfile;
            RenderProfileManifest.i.currentProfile.Apply();
            view.moreButton.SetToggleState(false);
        });

        collapseBarButton.onClick.AddListener(() => { ToggleCollapseBar(); });

        hideUIButton.onClick.AddListener(() => { ToggleHideUI(); });

        tutorialButton.onClick.AddListener(() => { OnRestartTutorial?.Invoke(); });
    }

    private void OnChangeProfile(RenderProfileWorld profile)
    {
        if (profile == RenderProfileManifest.i.defaultProfile)
        {
            dayModeButton.gameObject.SetActive(false);
            nightModeButton.gameObject.SetActive(true);
        }
        else
        {
            dayModeButton.gameObject.SetActive(true);
            nightModeButton.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.tutorialActive.OnChange -= TutorialActive_OnChange;
        RenderProfileManifest.i.OnChangeProfile -= OnChangeProfile;
    }

    private void TutorialActive_OnChange(bool current, bool previous)
    {
        collapseBarButton.gameObject.SetActive(!current);
        hideUIButton.gameObject.SetActive(!current);
    }

    internal void ActivateControlsButton()
    {
        controlsButton.gameObject.SetActive(true);

        controlsButton.onClick.AddListener(() =>
        {
            controlsToggleAction.RaiseOnTriggered();
            view.moreButton.SetToggleState(false);
        });
    }

    internal void ActivateHelpAndSupportButton()
    {
        helpAndSupportButton.gameObject.SetActive(true);

        helpAndSupportButton.onClick.AddListener(() =>
        {
            view.controller.helpAndSupportHud.SetVisibility(true);
            view.moreButton.SetToggleState(false);
        });
    }

    internal void ShowMoreMenu(bool visible, bool instant = false)
    {
        if (visible)
        {
            if (!moreMenuAnimator.gameObject.activeInHierarchy)
            {
                moreMenuAnimator.gameObject.SetActive(true);
            }

            moreMenuAnimator.Show(instant);
        }
        else
        {
            if (!moreMenuAnimator.gameObject.activeInHierarchy)
            {
                moreMenuAnimator.gameObject.SetActive(false);
            }
            else
            {
                moreMenuAnimator.Hide(instant);
            }
        }

        OnMoreMenuOpened?.Invoke(visible);
    }

    internal void ShowTutorialButton(bool visible)
    {
        tutorialButton.gameObject.SetActive(visible);
    }

    private void ToggleCollapseBar()
    {
        if (CommonScriptableObjects.tutorialActive)
            return;

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
        if (CommonScriptableObjects.tutorialActive)
            return;

        CommonScriptableObjects.allUIHidden.Set(!CommonScriptableObjects.allUIHidden.Get());
        view.moreButton.SetToggleState(false);
    }
}