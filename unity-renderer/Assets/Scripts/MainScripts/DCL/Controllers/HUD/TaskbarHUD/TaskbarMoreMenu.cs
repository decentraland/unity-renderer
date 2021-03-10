using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskbarMoreMenu : MonoBehaviour
{
    [Header("Menu Animation")]
    [SerializeField] internal ShowHideAnimator moreMenuAnimator;
    [SerializeField] internal float timeBetweenAnimations = 0.01f;

    [Header("Collapse Button Config")]
    [SerializeField] internal TaskbarMoreMenuButton collapseBarButton;
    [SerializeField] internal GameObject collapseIcon;
    [SerializeField] internal GameObject collapseText;
    [SerializeField] internal GameObject expandIcon;
    [SerializeField] internal GameObject expandText;

    [Header("Other Buttons Config")]
    [SerializeField] internal TaskbarMoreMenuButton hideUIButton;
    [SerializeField] internal TaskbarMoreMenuButton controlsButton;
    [SerializeField] internal InputAction_Trigger controlsToggleAction;
    [SerializeField] internal TaskbarMoreMenuButton helpAndSupportButton;
    [SerializeField] internal TaskbarMoreMenuButton tutorialButton;
    [SerializeField] internal TaskbarMoreMenuButton dayModeButton;
    [SerializeField] internal TaskbarMoreMenuButton nightModeButton;

    private TaskbarHUDView view;
    internal List<TaskbarMoreMenuButton> sortedButtonsAnimations = new List<TaskbarMoreMenuButton>();
    internal Coroutine moreMenuAnimationsCoroutine;

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

        SortButtonsAnimations();

        RenderProfileManifest.i.OnChangeProfile += OnChangeProfile;
        OnChangeProfile(RenderProfileManifest.i.currentProfile);

        dayModeButton.mainButton.onClick.AddListener(() =>
        {
            RenderProfileManifest.i.currentProfile = RenderProfileManifest.i.defaultProfile;
            RenderProfileManifest.i.currentProfile.Apply();
            view.moreButton.SetToggleState(false);
        });

        nightModeButton.mainButton.onClick.AddListener(() =>
        {
            RenderProfileManifest.i.currentProfile = RenderProfileManifest.i.nightProfile;
            RenderProfileManifest.i.currentProfile.Apply();
            view.moreButton.SetToggleState(false);
        });

        collapseBarButton.mainButton.onClick.AddListener(() => { ToggleCollapseBar(); });

        hideUIButton.mainButton.onClick.AddListener(() => { ToggleHideUI(); });

        tutorialButton.mainButton.onClick.AddListener(() => { OnRestartTutorial?.Invoke(); });
    }

    private void SortButtonsAnimations()
    {
        sortedButtonsAnimations.Add(helpAndSupportButton);
        sortedButtonsAnimations.Add(controlsButton);
        sortedButtonsAnimations.Add(hideUIButton);
        sortedButtonsAnimations.Add(nightModeButton);
        sortedButtonsAnimations.Add(dayModeButton);
        sortedButtonsAnimations.Add(tutorialButton);
        sortedButtonsAnimations.Add(collapseBarButton);
    }

    private void OnChangeProfile(RenderProfileWorld profile)
    {
        if (profile == RenderProfileManifest.i.defaultProfile)
        {
            dayModeButton.gameObject.SetActive(false);
            nightModeButton.gameObject.SetActive(true);

            if (moreMenuAnimator.isVisible)
                nightModeButton.PlayAnimation(TaskbarMoreMenuButton.AnimationStatus.Visible);
        }
        else
        {
            dayModeButton.gameObject.SetActive(true);
            nightModeButton.gameObject.SetActive(false);

            if (moreMenuAnimator.isVisible)
                dayModeButton.PlayAnimation(TaskbarMoreMenuButton.AnimationStatus.Visible);
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

        controlsButton.mainButton.onClick.AddListener(() =>
        {
            controlsToggleAction.RaiseOnTriggered();
            view.moreButton.SetToggleState(false);
        });
    }

    internal void ActivateHelpAndSupportButton()
    {
        helpAndSupportButton.gameObject.SetActive(true);

        helpAndSupportButton.mainButton.onClick.AddListener(() =>
        {
            view.controller.helpAndSupportHud.SetVisibility(true);
            view.moreButton.SetToggleState(false);
        });
    }

    internal void ShowMoreMenu(bool visible, bool instant = false)
    {
        CoroutineStarter.Stop(moreMenuAnimationsCoroutine);
        moreMenuAnimationsCoroutine = CoroutineStarter.Start(PlayMoreMenuAnimations(visible, instant));
        OnMoreMenuOpened?.Invoke(visible);
    }

    private IEnumerator PlayMoreMenuAnimations(bool visible, bool instant = false)
    {
        if (visible)
        {
            moreMenuAnimator.Show(instant);

            for (int i = 0; i < sortedButtonsAnimations.Count; i++)
            {
                if (!sortedButtonsAnimations[i].gameObject.activeInHierarchy ||
                    sortedButtonsAnimations[i].lastPlayedAnimation == TaskbarMoreMenuButton.AnimationStatus.In)
                {
                    continue;
                }

                sortedButtonsAnimations[i].PlayAnimation(TaskbarMoreMenuButton.AnimationStatus.In);
                yield return new WaitForSeconds(timeBetweenAnimations);
            }
        }
        else
        {
            for (int j = sortedButtonsAnimations.Count - 1; j >= 0; j--)
            {
                if (!sortedButtonsAnimations[j].gameObject.activeInHierarchy ||
                    sortedButtonsAnimations[j].lastPlayedAnimation == TaskbarMoreMenuButton.AnimationStatus.Out)
                {
                    continue;
                }

                sortedButtonsAnimations[j].PlayAnimation(TaskbarMoreMenuButton.AnimationStatus.Out);
                yield return new WaitForSeconds(timeBetweenAnimations);
            }

            if (sortedButtonsAnimations.Count > 0)
                yield return new WaitForSeconds(sortedButtonsAnimations[0].GetAnimationLenght());

            moreMenuAnimator.Hide(instant);
        }
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