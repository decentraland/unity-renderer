using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

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
            EmailRequested = 128, // NOTE: old email prompt set tutorialStep to 128 when finished
            NewTutorialFinished = 256
        }

        public enum TutorialType
        {
            Initial,
            BuilderInWorld
        }

        internal enum TutorialPath
        {
            FromGenesisPlaza,
            FromDeepLink,
            FromResetTutorial,
            FromBuilderInWorld,
            FromUserThatAlreadyDidTheTutorial
        }

        public static TutorialController i { get; private set; }

        public HUDController hudController
        {
            get => HUDController.i;
        }

        public int currentStepIndex { get; private set; }

        private const string PLAYER_PREFS_VOICE_CHAT_FEATURE_SHOWED = "VoiceChatFeatureShowed";

        [Header("General Configuration")]
        [SerializeField]
        internal int tutorialVersion = 1;

        [SerializeField]
        internal float timeBetweenSteps = 0.5f;

        [SerializeField]
        internal bool sendStats = true;

        [Header("Tutorial Steps on Genesis Plaza")]
        [SerializeField]
        internal List<TutorialStep> stepsOnGenesisPlaza = new List<TutorialStep>();

        [Header("Tutorial Steps from Deep Link")]
        [SerializeField]
        internal List<TutorialStep> stepsFromDeepLink = new List<TutorialStep>();

        [Header("Tutorial Steps from Reset Tutorial")]
        [SerializeField]
        internal List<TutorialStep> stepsFromReset = new List<TutorialStep>();

        [Header("Tutorial Steps from Builder In World")]
        [SerializeField]
        internal List<TutorialStep> stepsFromBuilderInWorld = new List<TutorialStep>();

        [Header("Tutorial Steps from User That Already Did The Tutorial")]
        [SerializeField]
        internal List<TutorialStep> stepsFromUserThatAlreadyDidTheTutorial = new List<TutorialStep>();

        [Header("3D Model Teacher")]
        [SerializeField]
        internal Camera teacherCamera;

        [SerializeField]
        internal RawImage teacherRawImage;

        [SerializeField]
        internal TutorialTeacher teacher;

        [SerializeField]
        internal float teacherMovementSpeed = 4f;

        [SerializeField]
        internal AnimationCurve teacherMovementCurve;

        [SerializeField]
        internal Canvas teacherCanvas;

        [Header("Eagle Eye Camera")]
        [SerializeField]
        internal CinemachineVirtualCamera eagleEyeCamera;

        [SerializeField]
        internal Vector3 eagleCamInitPosition = new Vector3(30, 30, -50);

        [SerializeField]
        internal Vector3 eagleCamInitLookAtPoint = new Vector3(0, 0, 0);

        [SerializeField]
        internal bool eagleCamRotationActived = true;

        [SerializeField]
        internal float eagleCamRotationSpeed = 1f;

        [Header("Debugging")]
        [SerializeField]
        internal bool debugRunTutorial = false;

        [SerializeField]
        internal int debugStartingStepIndex;

        [SerializeField]
        internal bool debugOpenedFromDeepLink = false;

        internal bool isRunning = false;
        internal bool openedFromDeepLink = false;
        internal bool playerIsInGenesisPlaza = false;
        internal TutorialStep runningStep = null;
        internal bool tutorialReset = false;
        internal float elapsedTimeInCurrentStep = 0f;
        internal TutorialPath currentPath;
        internal int currentStepNumber;
        internal TutorialType tutorialType = TutorialType.Initial;

        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;
        private Coroutine eagleEyeRotationCoroutine;

        private int tutorialLayerMask;

        internal bool userAlreadyDidTheTutorial { get; set; }

        private void Awake()
        {
            i = this;
            tutorialLayerMask = LayerMask.GetMask("Tutorial");
            ShowTeacher3DModel(false);
        }

        private void Start()
        {
            if (CommonScriptableObjects.isTaskbarHUDInitialized.Get())
                IsTaskbarHUDInitialized_OnChange(true, false);
            else
                CommonScriptableObjects.isTaskbarHUDInitialized.OnChange += IsTaskbarHUDInitialized_OnChange;

            if (debugRunTutorial)
                SetTutorialEnabled(debugOpenedFromDeepLink.ToString());
        }

        private void OnDestroy()
        {
            SetTutorialDisabled();

            CommonScriptableObjects.isTaskbarHUDInitialized.OnChange -= IsTaskbarHUDInitialized_OnChange;

            if (hudController != null &&
                hudController.taskbarHud != null)
            {
                hudController.taskbarHud.moreMenu.OnRestartTutorial -= MoreMenu_OnRestartTutorial;
            }

            NotificationsController.disableWelcomeNotification = false;
        }

        public void SetTutorialEnabled(string fromDeepLink)
        {
            SetupTutorial(fromDeepLink, TutorialType.Initial);
        }

        public void SetTutorialEnabledForUsersThatAlreadyDidTheTutorial()
        {
            // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
            if (PlayerPrefs.GetInt(PLAYER_PREFS_VOICE_CHAT_FEATURE_SHOWED) == 1)
                return;

            SetupTutorial(false.ToString(), TutorialType.Initial, true);
        }

        public void SetBuilderInWorldTutorialEnabled()
        {
            SetupTutorial(false.ToString(), TutorialType.BuilderInWorld);
        }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        void SetupTutorial(string fromDeepLink, TutorialType tutorialType, bool userAlreadyDidTheTutorial = false)
        {
            if (isRunning)
                return;

            isRunning = true;
            this.userAlreadyDidTheTutorial = userAlreadyDidTheTutorial;
            CommonScriptableObjects.allUIHidden.Set(false);
            CommonScriptableObjects.tutorialActive.Set(true);
            openedFromDeepLink = Convert.ToBoolean(fromDeepLink);
            this.tutorialType = tutorialType;

            hudController?.taskbarHud?.ShowTutorialOption(false);
            hudController?.profileHud?.HideProfileMenu();

            NotificationsController.disableWelcomeNotification = true;

            WebInterface.SetDelightedSurveyEnabled(false);

            ModifyCullingSettings();

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
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(false);

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

            tutorialReset = false;
            isRunning = false;
            ShowTeacher3DModel(false);
            WebInterface.SetDelightedSurveyEnabled(true);

            if (Environment.i != null)
            {
                WebInterface.SendSceneExternalActionEvent(Environment.i.worldState.currentSceneId, "tutorial", "end");
            }

            NotificationsController.disableWelcomeNotification = false;

            hudController?.taskbarHud?.ShowTutorialOption(true);

            CommonScriptableObjects.tutorialActive.Set(false);

            RestoreCullingSettings();

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
                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);
                runningStep = null;
            }

            switch (tutorialType)
            {
                case TutorialType.Initial:
                    if (userAlreadyDidTheTutorial)
                    {
                        yield return ExecuteSteps(TutorialPath.FromUserThatAlreadyDidTheTutorial, stepIndex);
                    }
                    else if (playerIsInGenesisPlaza || tutorialReset)
                    {
                        if (tutorialReset)
                        {
                            yield return ExecuteSteps(TutorialPath.FromResetTutorial, stepIndex);
                        }
                        else
                        {
                            yield return ExecuteSteps(TutorialPath.FromGenesisPlaza, stepIndex);
                        }
                    }
                    else if (openedFromDeepLink)
                    {
                        yield return ExecuteSteps(TutorialPath.FromDeepLink, stepIndex);
                    }
                    else
                    {
                        SetTutorialDisabled();
                        yield break;
                    }

                    break;
                case TutorialType.BuilderInWorld:
                    yield return ExecuteSteps(TutorialPath.FromBuilderInWorld, stepIndex);
                    break;
            }
        }

        /// <summary>
        /// Shows the teacher that will be guiding along the tutorial.
        /// </summary>
        /// <param name="active">True for show the teacher.</param>
        public void ShowTeacher3DModel(bool active)
        {
            teacherCamera.enabled = active;
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

        /// <summary>
        /// Set sort order for canvas containing teacher RawImage
        /// </summary>
        /// <param name="sortOrder"></param>
        public void SetTeacherCanvasSortingOrder(int sortOrder)
        {
            teacherCanvas.sortingOrder = sortOrder;
        }

        /// <summary>
        /// Finishes the current running step, skips all the next ones and completes the tutorial.
        /// </summary>
        public void SkipTutorial()
        {
            if (!debugRunTutorial && sendStats)
            {
                SendSkipTutorialSegmentStats(
                    tutorialVersion,
                    runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""));
            }

            int skipIndex = stepsOnGenesisPlaza.Count +
                            stepsFromDeepLink.Count +
                            stepsFromReset.Count +
                            stepsFromBuilderInWorld.Count +
                            stepsFromUserThatAlreadyDidTheTutorial.Count;

            StartCoroutine(StartTutorialFromStep(skipIndex));

            hudController?.taskbarHud?.SetVisibility(true);
            hudController?.profileHud?.SetBackpackButtonVisibility(true);
        }

        /// <summary>
        /// Activate/deactivate the eagle eye camera.
        /// </summary>
        /// <param name="isActive">True for activate the eagle eye camera.</param>
        public void SetEagleEyeCameraActive(bool isActive)
        {
            eagleEyeCamera.gameObject.SetActive(isActive);
            StartCoroutine(BlockPlayerCameraUntilBlendingIsFinished(isActive));

            if (isActive)
            {
                eagleEyeCamera.transform.position = eagleCamInitPosition;
                eagleEyeCamera.transform.LookAt(CommonScriptableObjects.playerUnityPosition.Get());

                if (eagleCamRotationActived)
                    eagleEyeRotationCoroutine = StartCoroutine(EagleEyeCameraRotation(eagleCamRotationSpeed));
            }
            else if (eagleEyeRotationCoroutine != null)
            {
                StopCoroutine(eagleEyeRotationCoroutine);
            }
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

        private IEnumerator ExecuteSteps(TutorialPath tutorialPath, int startingStepIndex)
        {
            List<TutorialStep> steps = new List<TutorialStep>();

            switch (tutorialPath)
            {
                case TutorialPath.FromGenesisPlaza:
                    steps = stepsOnGenesisPlaza;
                    break;
                case TutorialPath.FromDeepLink:
                    steps = stepsFromDeepLink;
                    break;
                case TutorialPath.FromResetTutorial:
                    steps = stepsFromReset;
                    break;
                case TutorialPath.FromBuilderInWorld:
                    steps = stepsFromBuilderInWorld;
                    break;
                case TutorialPath.FromUserThatAlreadyDidTheTutorial:
                    steps = stepsFromUserThatAlreadyDidTheTutorial;
                    break;
            }

            currentPath = tutorialPath;

            elapsedTimeInCurrentStep = 0f;
            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
                if (stepPrefab is TutorialStep_Tooltip_UsersAround &&
                    CommonScriptableObjects.voiceChatDisabled.Get())
                    continue;

                if (stepPrefab.letInstantiation)
                    runningStep = Instantiate(stepPrefab, this.transform).GetComponent<TutorialStep>();
                else
                    runningStep = steps[i];

                currentStepIndex = i;

                elapsedTimeInCurrentStep = Time.realtimeSinceStartup;
                currentStepNumber = i + 1;

                if (!debugRunTutorial && sendStats)
                {
                    SendStepStartedSegmentStats(
                        tutorialVersion,
                        tutorialPath,
                        i + 1,
                        runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""));
                }

                if (tutorialPath == TutorialPath.FromUserThatAlreadyDidTheTutorial &&
                    runningStep is TutorialStep_Tooltip)
                {
                    ((TutorialStep_Tooltip)runningStep).OverrideSetMaxTimeToHide(true);
                }

                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                if (i < steps.Count - 1)
                    PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.StepCompleted);
                else
                    PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);

                yield return runningStep.OnStepPlayHideAnimation();
                runningStep.OnStepFinished();
                elapsedTimeInCurrentStep = Time.realtimeSinceStartup - elapsedTimeInCurrentStep;

                if (!debugRunTutorial &&
                    sendStats &&
                    tutorialPath != TutorialPath.FromUserThatAlreadyDidTheTutorial)
                {
                    SendStepCompletedSegmentStats(
                        tutorialVersion,
                        tutorialPath,
                        i + 1,
                        runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""),
                        elapsedTimeInCurrentStep);
                }

                Destroy(runningStep.gameObject);

                if (i < steps.Count - 1 && timeBetweenSteps > 0)
                    yield return new WaitForSeconds(timeBetweenSteps);
            }

            if (!debugRunTutorial &&
                tutorialPath != TutorialPath.FromBuilderInWorld &&
                tutorialPath != TutorialPath.FromUserThatAlreadyDidTheTutorial)
            {
                SetUserTutorialStepAsCompleted(TutorialFinishStep.NewTutorialFinished);
            }

            runningStep = null;

            SetTutorialDisabled();
        }

        private void SetUserTutorialStepAsCompleted(TutorialFinishStep finishStepType)
        {
            WebInterface.SaveUserTutorialStep(UserProfile.GetOwnUserProfile().tutorialStep | (int) finishStepType);
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

        private void IsTaskbarHUDInitialized_OnChange(bool current, bool previous)
        {
            if (current &&
                hudController != null &&
                hudController.taskbarHud != null)
            {
                hudController.taskbarHud.moreMenu.OnRestartTutorial -= MoreMenu_OnRestartTutorial;
                hudController.taskbarHud.moreMenu.OnRestartTutorial += MoreMenu_OnRestartTutorial;
            }
        }

        private void MoreMenu_OnRestartTutorial()
        {
            SetTutorialDisabled();
            tutorialReset = true;
            SetTutorialEnabled(false.ToString());
        }

        private bool IsPlayerInsideGenesisPlaza()
        {
            WorldState worldState = Environment.i.worldState;
            if (worldState == null || worldState.currentSceneId == null)
                return false;

            Vector2Int genesisPlazaBaseCoords = new Vector2Int(-9, -9);
            ParcelScene currentScene = worldState.loadedScenes[worldState.currentSceneId];

            if (currentScene != null && currentScene.IsInsideSceneBoundaries(genesisPlazaBaseCoords))
                return true;

            return false;
        }

        private void SendStepStartedSegmentStats(int version, TutorialPath tutorialPath, int stepNumber, string stepName)
        {
            WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]
            {
                new WebInterface.AnalyticsPayload.Property("version", version.ToString()),
                new WebInterface.AnalyticsPayload.Property("path", tutorialPath.ToString()),
                new WebInterface.AnalyticsPayload.Property("step number", stepNumber.ToString()),
                new WebInterface.AnalyticsPayload.Property("step name", stepName)
            };
            WebInterface.ReportAnalyticsEvent("tutorial step started", properties);
        }

        private void SendStepCompletedSegmentStats(int version, TutorialPath tutorialPath, int stepNumber, string stepName, float elapsedTime)
        {
            WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]
            {
                new WebInterface.AnalyticsPayload.Property("version", version.ToString()),
                new WebInterface.AnalyticsPayload.Property("path", tutorialPath.ToString()),
                new WebInterface.AnalyticsPayload.Property("step number", stepNumber.ToString()),
                new WebInterface.AnalyticsPayload.Property("step name", stepName),
                new WebInterface.AnalyticsPayload.Property("elapsed time", elapsedTime.ToString("0.00"))
            };
            WebInterface.ReportAnalyticsEvent("tutorial step completed", properties);
        }

        private void SendSkipTutorialSegmentStats(int version, string stepName)
        {
            WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]
            {
                new WebInterface.AnalyticsPayload.Property("version", version.ToString()),
                new WebInterface.AnalyticsPayload.Property("path", currentPath.ToString()),
                new WebInterface.AnalyticsPayload.Property("step number", currentStepNumber.ToString()),
                new WebInterface.AnalyticsPayload.Property("step name", stepName),
                new WebInterface.AnalyticsPayload.Property("elapsed time", (Time.realtimeSinceStartup - elapsedTimeInCurrentStep).ToString("0.00"))
            };
            WebInterface.ReportAnalyticsEvent("tutorial skipped", properties);
        }

        private IEnumerator EagleEyeCameraRotation(float rotationSpeed)
        {
            while (true)
            {
                eagleEyeCamera.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed, Space.World);
                yield return null;
            }
        }

        private IEnumerator BlockPlayerCameraUntilBlendingIsFinished(bool hideUIs)
        {
            if (hideUIs)
            {
                hudController?.minimapHud?.SetVisibility(false);
                hudController?.profileHud?.SetVisibility(false);
            }

            CommonScriptableObjects.cameraBlocked.Set(true);

            yield return null;
            yield return new WaitUntil(() => !CommonScriptableObjects.cameraIsBlending.Get());

            CommonScriptableObjects.cameraBlocked.Set(false);

            if (!hideUIs)
            {
                hudController?.minimapHud?.SetVisibility(true);
                hudController?.profileHud?.SetVisibility(true);
            }
        }

        private void ModifyCullingSettings()
        {
            var cullingSettings = Environment.i.cullingController.GetSettingsCopy();
            cullingSettings.ignoredLayersMask |= tutorialLayerMask;
            Environment.i.cullingController.SetSettings(cullingSettings);
        }

        private void RestoreCullingSettings()
        {
            var cullingSettings = Environment.i.cullingController.GetSettingsCopy();
            cullingSettings.ignoredLayersMask &= ~tutorialLayerMask;
            Environment.i.cullingController.SetSettings(cullingSettings);
        }
    }
}