using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        public static TutorialController i { private set; get; }

#if UNITY_EDITOR
        [Header("Debugging")]
        public bool debugRunTutorialOnStart = false;
        public TutorialStep.Id debugFlagStartingValue;
#endif

        public bool isTutorialEnabled { private set; get; } = false;

        [Header("Stage Controller References")]
        [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();
        [SerializeField] GameObject emailPromptHUDGameObject;

        TutorialStep runningStep = null;
        TutorialStep.Id currentTutorialStepId = TutorialStep.Id.NONE;
        int currentTutorialStepIndex = 0;
        bool initialized = false;
        Canvas chatUIScreen = null;

        public void SetTutorialEnabled()
        {
            if (RenderingController.i)
                RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;

            isTutorialEnabled = true;

            AirdroppingHUDController.OnAirdropFinished += TriggerEmailPrompt;
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
            AirdroppingHUDController.OnAirdropFinished -= TriggerEmailPrompt;

            if (RenderingController.i)
                RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;

            i = null;
        }

        Coroutine executeStepsCoroutine;

        private void StartTutorialFromStep(TutorialStep.Id stepId)
        {
            if (!initialized)
                Initialize();

            if (runningStep != null)
            {
                StopCoroutine(executeStepsCoroutine);

                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);

                runningStep = null;
            }

            executeStepsCoroutine = StartCoroutine(ExecuteSteps(stepId));
        }

        public void SkipToNextStep()
        {
            int nextStepIndex = currentTutorialStepIndex + 1;

            if (nextStepIndex == steps.Count)
                return;

            StartTutorialFromStep(steps[nextStepIndex].stepId);
        }

        private IEnumerator ExecuteSteps(TutorialStep.Id startingStep)
        {
            int startingStepIndex = GetStepIndexFromTutorialStepId(startingStep);

            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                runningStep = Instantiate(stepPrefab).GetComponent<TutorialStep>();

                currentTutorialStepId = runningStep.stepId;
                currentTutorialStepIndex = i;

                UserProfile.GetOwnUserProfile().SetTutorialStepId((int)currentTutorialStepId);

                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                runningStep.OnStepFinished();

                Destroy(runningStep.gameObject);
            }

            currentTutorialStepId = TutorialStep.Id.FINISHED;
            UserProfile.GetOwnUserProfile().SetTutorialStepId((int)currentTutorialStepId);
            runningStep = null;
        }

        private int GetTutorialStepFromProfile()
        {
            return UserProfile.GetOwnUserProfile().tutorialStep;
        }

        private void OnRenderingStateChanged(bool renderingEnabled)
        {
            if (!isTutorialEnabled || !renderingEnabled) return;

            currentTutorialStepId = (TutorialStep.Id)GetTutorialStepFromProfile();

#if UNITY_EDITOR
            if (debugFlagStartingValue != 0)
            {
                currentTutorialStepId = debugFlagStartingValue;
            }
#endif
            if (currentTutorialStepId == TutorialStep.Id.FINISHED || runningStep != null)
                return;

            StartTutorialFromStep(currentTutorialStepId);
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

        private int GetStepIndexFromTutorialStepId(TutorialStep.Id step)
        {
            int result = 0;
            int stepsCount = steps.Count;

            for (int i = 0; i < stepsCount; i++)
            {
                if (steps[i].stepId == step)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        void TriggerEmailPrompt()
        {
            emailPromptHUDGameObject.SetActive(true);
        }
    }
}
