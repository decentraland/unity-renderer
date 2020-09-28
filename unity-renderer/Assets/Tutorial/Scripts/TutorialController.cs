using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [Flags]
        public enum TutorialFinishStep
        {
            None = 0,
            OldTutorialValue = 99, // NOTE: old tutorial set tutorialStep to 99 when finished
            EmailRequested = 128, // NOTE: old email prompt set tutorialStep to 1289 when finished
            NewTutorialFinished = 256
        }

        public static TutorialController i { get; private set; }

        public HUDController hudController { get => HUDController.i; }

        [Header("General Configuration")]
        [SerializeField] internal float timeBetweenSteps = 0.5f;

        [Header("Tutorial Steps on Genesis Plaza")]
        [SerializeField] internal List<TutorialStep> stepsOnGenesisPlaza = new List<TutorialStep>();

        [Header("Tutorial Steps from Deep Link")]
        [SerializeField] internal List<TutorialStep> stepsFromDeepLink = new List<TutorialStep>();

        [Header("Tutorial Steps on Genesis Plaza (after Deep Link)")]
        [SerializeField] internal List<TutorialStep> stepsOnGenesisPlazaAfterDeepLink = new List<TutorialStep>();

        [Header("3D Model Teacher")]
        [SerializeField] internal Camera teacherCamera;
        [SerializeField] internal RawImage teacherRawImage;
        [SerializeField] internal TutorialTeacher teacher;
        [SerializeField] internal float teacherMovementSpeed = 4f;
        [SerializeField] internal AnimationCurve teacherMovementCurve;

        [Header("Debugging")]
        [SerializeField] internal bool debugRunTutorial = false;
        [SerializeField] internal int debugStartingStepIndex;
        [SerializeField] internal bool debugOpenedFromDeepLink = false;

        internal bool isRunning = false;
        internal bool openedFromDeepLink = false;
        internal bool alreadyOpenedFromDeepLink = false;
        internal bool playerIsInGenesisPlaza = false;
        internal bool markTutorialAsCompleted = false;
        internal TutorialStep runningStep = null;

        private int currentStepIndex;
        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;

        private void Awake()
        {
            i = this;
            ShowTeacher3DModel(false);
        }

        private void Start()
        {
            if (debugRunTutorial)
                SetTutorialEnabled(debugOpenedFromDeepLink.ToString());
        }

        private void OnDestroy()
        {
            SetTutorialDisabled();

            if (hudController != null && hudController.goToGenesisPlazaHud != null)
            {
                hudController.goToGenesisPlazaHud.OnBeforeGoToGenesisPlaza -= GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza;
                hudController.goToGenesisPlazaHud.OnAfterGoToGenesisPlaza -= GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza;
            }
        }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        public void SetTutorialEnabled(string fromDeepLink)
        {
            if (isRunning)
                return;

            isRunning = true;
            openedFromDeepLink = Convert.ToBoolean(fromDeepLink);

            if (hudController != null && hudController.goToGenesisPlazaHud != null)
            {
                hudController.goToGenesisPlazaHud.OnBeforeGoToGenesisPlaza -= GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza;
                hudController.goToGenesisPlazaHud.OnBeforeGoToGenesisPlaza += GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza;
                hudController.goToGenesisPlazaHud.OnAfterGoToGenesisPlaza -= GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza;
                hudController.goToGenesisPlazaHud.OnAfterGoToGenesisPlaza += GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza;
            }

            if (!CommonScriptableObjects.rendererState.Get())
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            else
                OnRenderingStateChanged(true, false);
        }

        /// <summary>
        /// Stop and disables the tutorial controller.
        /// </summary>
        public void SetTutorialDisabled()
        {
            if (executeStepsCoroutine != null)
            {
                StopCoroutine(executeStepsCoroutine);
                executeStepsCoroutine = null;
            }

            if (runningStep != null)
            {
                Destroy(runningStep.gameObject);
                runningStep = null;
            }

            isRunning = false;
            ShowTeacher3DModel(false);

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
        }

        /// <summary>
        /// Starts to execute the tutorial from a specific step (It is needed to call SetTutorialEnabled() before).
        /// </summary>
        /// <param name="stepIndex">First step to be executed.</param>
        public IEnumerator StartTutorialFromStep(int stepIndex)
        {
            if (!isRunning)
                yield break;

            if (runningStep != null)
            {
                yield return runningStep.OnStepPlayAnimationForHidding();

                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);
                runningStep = null;
            }

            if (playerIsInGenesisPlaza)
            {
                markTutorialAsCompleted = true;

                if (alreadyOpenedFromDeepLink)
                    yield return ExecuteSteps(stepsOnGenesisPlazaAfterDeepLink, stepIndex);
                else
                    yield return ExecuteSteps(stepsOnGenesisPlaza, stepIndex);
                
            }
            else if (openedFromDeepLink)
            {
                markTutorialAsCompleted = false;
                alreadyOpenedFromDeepLink = true;
                yield return ExecuteSteps(stepsFromDeepLink, stepIndex);
            }
            else
            {
                SetTutorialDisabled();
                yield break;
            }
        }

        /// <summary>
        /// Shows the teacher that will be guiding along the tutorial.
        /// </summary>
        /// <param name="active">True for show the teacher.</param>
        public void ShowTeacher3DModel(bool active)
        {
            teacherCamera.gameObject.SetActive(active);
            teacherRawImage.gameObject.SetActive(active);
        }

        /// <summary>
        /// Move the tutorial teacher to a specific position.
        /// </summary>
        /// <param name="position">Target position.</param>
        /// <param name="animated">True for apply a smooth movement.</param>
        public void SetTeacherPosition(Vector2 position, bool animated = true)
        {
            if (teacherMovementCoroutine != null)
                StopCoroutine(teacherMovementCoroutine);

            if (animated)
                teacherMovementCoroutine = StartCoroutine(MoveTeacher(teacherRawImage.rectTransform.position, position));
            else
                teacherRawImage.rectTransform.position = position;
        }

        /// <summary>
        /// Plays a specific animation on the tutorial teacher.
        /// </summary>
        /// <param name="animation">Animation to apply.</param>
        public void PlayTeacherAnimation(TutorialTeacher.TeacherAnimation animation)
        {
            teacher.PlayAnimation(animation);
        }

        private int GetTutorialStepFromProfile()
        {
            return UserProfile.GetOwnUserProfile().tutorialStep;
        }

        private void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!renderingEnabled)
                return;

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;

            playerIsInGenesisPlaza = IsPlayerInsideGenesisPlaza();

            if (debugRunTutorial)
                currentStepIndex = debugStartingStepIndex >= 0 ? debugStartingStepIndex : 0;
            else
                currentStepIndex = 0;

            PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Reset);
            executeStepsCoroutine = StartCoroutine(StartTutorialFromStep(currentStepIndex));
        }

        private IEnumerator ExecuteSteps(List<TutorialStep> steps, int startingStepIndex)
        {
            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                if (stepPrefab.letInstantiation)
                    runningStep = Instantiate(stepPrefab, this.transform).GetComponent<TutorialStep>();
                else
                    runningStep = steps[i];

                currentStepIndex = i;

                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                if (i < steps.Count - 1)
                    PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.StepCompleted);
                else
                    PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);

                yield return runningStep.OnStepPlayAnimationForHidding();
                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);

                if (i < steps.Count - 1 && timeBetweenSteps > 0)
                    yield return new WaitForSeconds(timeBetweenSteps);
            }

            if (!debugRunTutorial && markTutorialAsCompleted)
                WebInterface.SaveUserTutorialStep(GetTutorialStepFromProfile() | (int)TutorialFinishStep.NewTutorialFinished);

            runningStep = null;

            SetTutorialDisabled();
        }

        private IEnumerator MoveTeacher(Vector2 fromPosition, Vector2 toPosition)
        {
            float t = 0f;

            while (Vector2.Distance(teacherRawImage.rectTransform.position, toPosition) > 0)
            {
                t += teacherMovementSpeed * Time.deltaTime;
                if (t <= 1.0f)
                    teacherRawImage.rectTransform.position = Vector2.Lerp(fromPosition, toPosition, teacherMovementCurve.Evaluate(t));
                else
                    teacherRawImage.rectTransform.position = toPosition;

                yield return null;
            }
        }

        private void GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza()
        {
            SetTutorialDisabled();
        }

        private void GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza()
        {
            SetTutorialEnabled(false.ToString());

            if (hudController != null)
                hudController.taskbarHud.HideGoToGenesisPlazaButton();
        }

        private bool IsPlayerInsideGenesisPlaza()
        {
            if (SceneController.i == null || SceneController.i.currentSceneId == null)
                return false;

            Vector2Int genesisPlazaBaseCoords = new Vector2Int(-9, -9);
            ParcelScene currentScene = SceneController.i.loadedScenes[SceneController.i.currentSceneId];

            if (currentScene != null && currentScene.IsInsideSceneBoundaries(genesisPlazaBaseCoords))
                return true;

            return false;
        }
    }
}
