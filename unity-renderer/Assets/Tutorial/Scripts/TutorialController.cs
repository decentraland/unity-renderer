using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static TutorialController i { private set; get; }

    public const float DEFAULT_STAGE_IDLE_TIME = 20f;

#if UNITY_EDITOR
    [Header("Debugging")]
    public bool debugRunTutorialOnStart = false;
    public TutorialStep.Id debugFlagStartingValue;
    [Space()]
#endif

    [Header("Stage Controller References")]
    [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();

    public bool isTutorialEnabled { private set; get; } = false;

    private TutorialStep runningStep = null;


    private int currentTutorialStep = 0;
    private bool initialized = false;
    private Canvas chatUIScreen = null;

    public void SetTutorialEnabled()
    {
        if (RenderingController.i)
            RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;

        isTutorialEnabled = true;
    }

    private void Awake()
    {
        i = this;
    }

#if UNITY_EDITOR
    private void Start()
    {
        if (!debugRunTutorialOnStart)
            return;

        isTutorialEnabled = true;

        if (!RenderingController.i)
        {
            OnRenderingStateChanged(true);
        }
        else
        {
            RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
        }
    }
#endif

    private void OnDestroy()
    {
        if (RenderingController.i)
            RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;

        i = null;
    }

    private void StartTutorialFromStep(int stepId)
    {
        if (!initialized)
            Initialize();

        if (runningStep != null)
            return;

        StartCoroutine(ExecuteSteps((TutorialStep.Id)stepId));
    }

    private IEnumerator ExecuteSteps(TutorialStep.Id startingStep)
    {
        int startingStepIndex = 0;
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].stepId == startingStep)
            {
                startingStepIndex = i;
                break;
            }
        }

        for (int i = startingStepIndex; i < steps.Count; i++)
        {
            runningStep = steps[i];
            currentTutorialStep = (int)runningStep.stepId;

            var stepInstance = Instantiate(runningStep);

            UserProfile.GetOwnUserProfile().SetTutorialFlag(currentTutorialStep);

            stepInstance.OnStepStart();
            yield return stepInstance.OnStepExecute();
            stepInstance.OnStepFinished();

            Destroy(stepInstance);
        }

        currentTutorialStep = (int)TutorialStep.Id.FINISHED;
        UserProfile.GetOwnUserProfile().SetTutorialFlag(currentTutorialStep);
        runningStep = null;
    }

    private int GetTutorialStepFromProfile()
    {
        return UserProfile.GetOwnUserProfile().tutorialStep;
    }

    private void OnRenderingStateChanged(bool renderingEnabled)
    {
        if (!isTutorialEnabled || !renderingEnabled) return;

        currentTutorialStep = GetTutorialStepFromProfile(); // TODO: get flag from user profile

#if UNITY_EDITOR
        if (debugFlagStartingValue != 0)
        {
            currentTutorialStep = (int)debugFlagStartingValue;
        }
#endif
        if (currentTutorialStep == (int)TutorialStep.Id.FINISHED)
            return;

        StartTutorialFromStep(currentTutorialStep);
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        CacheChatScreen();
    }

    private void CacheChatScreen()
    {
        if (chatUIScreen == null && DCL.SceneController.i)
        {
            ParcelScene uiScene = DCL.SceneController.i.loadedScenes[DCL.SceneController.i.globalSceneId];
            chatUIScreen = uiScene.uiScreenSpace.canvas;
        }
    }

    public void SetChatVisible(bool visible)
    {
        if (chatUIScreen != null)
        {
            chatUIScreen.enabled = visible;
        }
    }
}
