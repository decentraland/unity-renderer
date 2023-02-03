using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Tutorial
{
    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : IPlugin
    {
        [Serializable]
        public class TutorialInitializationMessage
        {
            public string fromDeepLink;
            public string enableNewTutorialCamera;
        }

        [Flags]
        public enum TutorialFinishStep
        {
            None = 0,
            OldTutorialValue = 99, // NOTE: old tutorial set tutorialStep to 99 when finished
            EmailRequested = 128, // NOTE: old email prompt set tutorialStep to 128 when finished
            NewTutorialFinished = 256,
        }

        internal enum TutorialPath
        {
            FromGenesisPlaza,
            FromDeepLink,
            FromResetTutorial,
            FromUserThatAlreadyDidTheTutorial,
        }

        public static TutorialController i { get; private set; }

        public HUDController hudController => HUDController.i;

        public int currentStepIndex { get; internal set; }
        public event Action OnTutorialEnabled;
        public event Action OnTutorialDisabled;

        private const string PLAYER_PREFS_START_MENU_SHOWED = "StartMenuFeatureShowed";

        private readonly DataStore_Common commonDataStore;
        private readonly DataStore_Settings settingsDataStore;
        private readonly DataStore_ExploreV2 exploreV2DataStore;

        internal readonly TutorialView tutorialView;

        internal TutorialSettings configuration;

        internal bool openedFromDeepLink;
        internal bool playerIsInGenesisPlaza;
        internal TutorialStep runningStep;
        internal bool tutorialReset;
        private float elapsedTimeInCurrentStep;
        internal TutorialPath currentPath;
        private int currentStepNumber;
        private int nextStepsToSkip;

        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;
        private Coroutine eagleEyeRotationCoroutine;

        internal bool userAlreadyDidTheTutorial { get; set; }

        public TutorialController(DataStore_Common commonDataStore, DataStore_Settings settingsDataStore, DataStore_ExploreV2 exploreV2DataStore)
        {
            this.commonDataStore = commonDataStore;
            this.settingsDataStore = settingsDataStore;
            this.exploreV2DataStore = exploreV2DataStore;

            i = this;

            tutorialView = CreateTutorialView();
            SetConfiguration(tutorialView.configuration);
        }

        private TutorialView CreateTutorialView()
        {
            GameObject tutorialObject = Object.Instantiate(Resources.Load<GameObject>("TutorialView"));
            tutorialObject.name = "TutorialController";

            TutorialView view = tutorialObject.GetComponent<TutorialView>();
            view.ConfigureView(this);

            return view;
        }

        public void SetConfiguration(TutorialSettings config)
        {
            configuration = config;

            ShowTeacher3DModel(false);

            if (settingsDataStore.isInitialized.Get())
                IsSettingsHUDInitialized_OnChange(true, false);
            else
                settingsDataStore.isInitialized.OnChange += IsSettingsHUDInitialized_OnChange;

            if (config.debugRunTutorial)
                SetTutorialEnabled(JsonUtility.ToJson(new TutorialInitializationMessage
                {
                    fromDeepLink = config.debugOpenedFromDeepLink.ToString(),
                    enableNewTutorialCamera = false.ToString(),
                }));
        }

        public void Dispose()
        {
            SetTutorialDisabled();

            settingsDataStore.isInitialized.OnChange -= IsSettingsHUDInitialized_OnChange;

            if (hudController is { settingsPanelHud: { } })
                hudController.settingsPanelHud.OnRestartTutorial -= OnRestartTutorial;

            NotificationsController.disableWelcomeNotification = false;

            if (tutorialView != null)
                Object.Destroy(tutorialView.gameObject);
        }

        public void SetTutorialEnabled(string json)
        {
            TutorialInitializationMessage msg = JsonUtility.FromJson<TutorialInitializationMessage>(json);
            SetupTutorial(msg.fromDeepLink, msg.enableNewTutorialCamera);
        }

        public void SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(string json)
        {
            TutorialInitializationMessage msg = JsonUtility.FromJson<TutorialInitializationMessage>(json);

            // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactor the tutorial system in order to make it compatible with incremental features.
            if (PlayerPrefsBridge.GetInt(PLAYER_PREFS_START_MENU_SHOWED) == 1)
                return;

            SetupTutorial(false.ToString(), msg.enableNewTutorialCamera, true);
        }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        internal void SetupTutorial(string fromDeepLink, string enableNewTutorialCamera, bool userAlreadyDidTheTutorial = false)
        {
            if (commonDataStore.isWorld.Get() || commonDataStore.isTutorialRunning.Get())
                return;

            if (Convert.ToBoolean(enableNewTutorialCamera))
            {
                configuration.eagleCamInitPosition = new Vector3(15, 115, -30);
                configuration.eagleCamInitLookAtPoint = new Vector3(16, 105, 6);
                configuration.eagleCamRotationActived = false;
            }

            commonDataStore.isTutorialRunning.Set(true);
            this.userAlreadyDidTheTutorial = userAlreadyDidTheTutorial;
            CommonScriptableObjects.allUIHidden.Set(false);
            CommonScriptableObjects.tutorialActive.Set(true);
            openedFromDeepLink = Convert.ToBoolean(fromDeepLink);

            hudController?.settingsPanelHud?.SetTutorialButtonEnabled(false);

            NotificationsController.disableWelcomeNotification = true;

            WebInterface.SetDelightedSurveyEnabled(false);

            if (!CommonScriptableObjects.rendererState.Get())
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            else
                OnRenderingStateChanged(true, false);

            OnTutorialEnabled?.Invoke();
        }

        /// <summary>
        /// Stop and disables the tutorial controller.
        /// </summary>
        public void SetTutorialDisabled()
        {
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(false);

            if (executeStepsCoroutine != null)
            {
                CoroutineStarter.Stop(executeStepsCoroutine);
                executeStepsCoroutine = null;
            }

            if (runningStep != null)
            {
                Object.Destroy(runningStep.gameObject);
                runningStep = null;
            }

            if (teacherMovementCoroutine != null)
            {
                CoroutineStarter.Stop(teacherMovementCoroutine);
                teacherMovementCoroutine = null;
            }

            tutorialReset = false;
            commonDataStore.isTutorialRunning.Set(false);
            tutorialView.tutorialMusicHandler.StopTutorialMusic();
            ShowTeacher3DModel(false);
            WebInterface.SetDelightedSurveyEnabled(true);

            if (Environment.i is { world: { } })
                WebInterface.SendSceneExternalActionEvent(Environment.i.world.state.GetCurrentSceneNumber(), "tutorial", "end");

            NotificationsController.disableWelcomeNotification = false;

            hudController?.settingsPanelHud?.SetTutorialButtonEnabled(true);

            CommonScriptableObjects.tutorialActive.Set(false);

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;

            OnTutorialDisabled?.Invoke();
        }

        /// <summary>
        /// Starts to execute the tutorial from a specific step (It is needed to call SetTutorialEnabled() before).
        /// </summary>
        /// <param name="stepIndex">First step to be executed.</param>
        internal IEnumerator StartTutorialFromStep(int stepIndex)
        {
            if (!commonDataStore.isTutorialRunning.Get())
                yield break;

            if (runningStep != null)
            {
                runningStep.OnStepFinished();
                Object.Destroy(runningStep.gameObject);
                runningStep = null;
            }

            yield return new WaitUntil(IsPlayerInScene);

            playerIsInGenesisPlaza = IsPlayerInsideGenesisPlaza();

            if (playerIsInGenesisPlaza)
                tutorialView.tutorialMusicHandler.TryPlayingMusic();

            yield return ExecuteRespectiveTutorialStep(stepIndex);
        }

        private IEnumerator ExecuteRespectiveTutorialStep(int stepIndex)
        {
            if (userAlreadyDidTheTutorial)
                yield return ExecuteSteps(TutorialPath.FromUserThatAlreadyDidTheTutorial, stepIndex);
            else if (tutorialReset)
                yield return ExecuteSteps(TutorialPath.FromResetTutorial, stepIndex);
            else if (playerIsInGenesisPlaza)
                yield return ExecuteSteps(TutorialPath.FromGenesisPlaza, stepIndex);
            else if (openedFromDeepLink)
                yield return ExecuteSteps(TutorialPath.FromDeepLink, stepIndex);
            else
                SetTutorialDisabled();
        }

        /// <summary>
        /// Shows the teacher that will be guiding along the tutorial.
        /// </summary>
        /// <param name="active">True for show the teacher.</param>
        public void ShowTeacher3DModel(bool active)
        {
            if (configuration.teacherCamera != null)
                configuration.teacherCamera.enabled = active;

            if (configuration.teacherRawImage != null)
                configuration.teacherRawImage.gameObject.SetActive(active);
        }

        /// <summary>
        /// Move the tutorial teacher to a specific position.
        /// </summary>
        /// <param name="destinationPosition">Target position.</param>
        /// <param name="animated">True for apply a smooth movement.</param>
        public void SetTeacherPosition(Vector3 destinationPosition, bool animated = true)
        {
            if (teacherMovementCoroutine != null)
                CoroutineStarter.Stop(teacherMovementCoroutine);

            if (configuration.teacherRawImage != null)
            {
                if (animated)
                    teacherMovementCoroutine = CoroutineStarter.Start(MoveTeacher(destinationPosition));
                else
                    configuration.teacherRawImage.rectTransform.position = new Vector3(destinationPosition.x, destinationPosition.y, configuration.teacherRawImage.rectTransform.position.z);
            }
        }

        /// <summary>
        /// Plays a specific animation on the tutorial teacher.
        /// </summary>
        /// <param name="animation">Animation to apply.</param>
        public void PlayTeacherAnimation(TutorialTeacher.TeacherAnimation animation)
        {
            if (configuration.teacher == null)
                return;

            configuration.teacher.PlayAnimation(animation);
        }

        /// <summary>
        /// Set sort order for canvas containing teacher RawImage
        /// </summary>
        /// <param name="sortOrder"></param>
        public void SetTeacherCanvasSortingOrder(int sortOrder)
        {
            if (configuration.teacherCanvas == null)
                return;

            configuration.teacherCanvas.sortingOrder = sortOrder;
        }

        /// <summary>
        /// Finishes the current running step, skips all the next ones and completes the tutorial.
        /// </summary>
        public void SkipTutorial(bool ignoreStatsSending = false)
        {
            if (!ignoreStatsSending && NeedToSendStats())
                SendSkipTutorialSegmentStats(configuration.tutorialVersion, runningStep.name);

            int skipIndex = configuration.stepsOnGenesisPlaza.Count +
                            configuration.stepsFromDeepLink.Count +
                            configuration.stepsFromReset.Count +
                            configuration.stepsFromUserThatAlreadyDidTheTutorial.Count;

            CoroutineStarter.Start(StartTutorialFromStep(skipIndex));

            hudController?.taskbarHud?.SetVisibility(true);
        }

        /// <summary>
        /// Jump to a specific step.
        /// </summary>
        /// <param name="stepName">Step to jump.</param>
        public void GoToSpecificStep(string stepName)
        {
            int stepIndex;

            if (userAlreadyDidTheTutorial)
                stepIndex = configuration.stepsFromUserThatAlreadyDidTheTutorial.FindIndex(x => x.name == stepName);
            else if (tutorialReset)
                stepIndex = configuration.stepsFromReset.FindIndex(x => x.name == stepName);
            else if (playerIsInGenesisPlaza)
                stepIndex = configuration.stepsOnGenesisPlaza.FindIndex(x => x.name == stepName);
            else if (openedFromDeepLink)
                stepIndex = configuration.stepsFromDeepLink.FindIndex(x => x.name == stepName);
            else
                stepIndex = 0;

            nextStepsToSkip = 0;

            if (stepIndex >= 0)
                CoroutineStarter.Start(StartTutorialFromStep(stepIndex));
            else
                SkipTutorial(true);
        }

        /// <summary>
        /// Set the number of steps that will be skipped in the next iteration.
        /// </summary>
        /// <param name="skippedSteps">Number of steps to skip.</param>
        public void SetNextSkippedSteps(int skippedSteps) =>
            nextStepsToSkip = skippedSteps;

        /// <summary>
        /// Activate/deactivate the eagle eye camera.
        /// </summary>
        /// <param name="isActive">True for activate the eagle eye camera.</param>
        public void SetEagleEyeCameraActive(bool isActive)
        {
            configuration.eagleEyeCamera.gameObject.SetActive(isActive);
            CoroutineStarter.Start(BlockPlayerCameraUntilBlendingIsFinished(isActive));

            if (isActive)
            {
                configuration.eagleEyeCamera.transform.position = configuration.eagleCamInitPosition;
                configuration.eagleEyeCamera.transform.LookAt(configuration.eagleCamInitLookAtPoint);

                if (configuration.eagleCamRotationActived)
                    eagleEyeRotationCoroutine = CoroutineStarter.Start(EagleEyeCameraRotation(configuration.eagleCamRotationSpeed));
            }
            else if (eagleEyeRotationCoroutine != null)
                CoroutineStarter.Stop(eagleEyeRotationCoroutine);
        }

        internal void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!renderingEnabled)
                return;

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;

            currentStepIndex = configuration.debugRunTutorial ?  configuration.debugStartingStepIndex : 0;

            PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Reset);
            executeStepsCoroutine = CoroutineStarter.Start(StartTutorialFromStep(currentStepIndex));
        }

        private IEnumerator ExecuteSteps(TutorialPath tutorialPath, int startingStepIndex)
        {
            List<TutorialStep> steps = tutorialPath switch
                                       {
                                           TutorialPath.FromGenesisPlaza => configuration.stepsOnGenesisPlaza,
                                           TutorialPath.FromDeepLink => configuration.stepsFromDeepLink,
                                           TutorialPath.FromResetTutorial => configuration.stepsFromReset,
                                           TutorialPath.FromUserThatAlreadyDidTheTutorial => configuration.stepsFromUserThatAlreadyDidTheTutorial,
                                           _ => new List<TutorialStep>(),
                                       };

            currentPath = tutorialPath;

            elapsedTimeInCurrentStep = 0f;

            yield return IterateSteps(tutorialPath, startingStepIndex, steps);

            if (!configuration.debugRunTutorial && tutorialPath != TutorialPath.FromUserThatAlreadyDidTheTutorial)
                SetUserTutorialStepAsCompleted(TutorialFinishStep.NewTutorialFinished);

            runningStep = null;

            SetTutorialDisabled();
        }

        private IEnumerator IterateSteps(TutorialPath tutorialPath, int startingStepIndex, List<TutorialStep> steps)
        {
            for (int stepId = startingStepIndex; stepId < steps.Count; stepId++)
            {
                if (nextStepsToSkip > 0)
                {
                    nextStepsToSkip--;
                    continue;
                }

                TutorialStep stepPrefab = steps[stepId];

                // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
                if (stepPrefab is TutorialStep_Tooltip_ExploreButton && !exploreV2DataStore.isInitialized.Get())
                    continue;

                yield return RunStep(tutorialPath, stepPrefab, stepId, steps);

                if (stepId < steps.Count - 1 && configuration.timeBetweenSteps > 0)
                    yield return new WaitForSeconds(configuration.timeBetweenSteps);
            }
        }

        private IEnumerator RunStep(TutorialPath tutorialPath, TutorialStep stepPrefab, int stepId, List<TutorialStep> steps)
        {
            runningStep = stepPrefab.letInstantiation ? Object.Instantiate(stepPrefab, tutorialView.transform).GetComponent<TutorialStep>() : steps[stepId];

            runningStep.gameObject.name = runningStep.gameObject.name.Replace("(Clone)", "");
            currentStepIndex = stepId;

            elapsedTimeInCurrentStep = Time.realtimeSinceStartup;
            currentStepNumber = stepId + 1;

            if (NeedToSendStats())
                SendStepStartedSegmentStats(configuration.tutorialVersion, tutorialPath, stepId + 1, runningStep.name);

            runningStep.OnStepStart();
            yield return runningStep.OnStepExecute();

            PlayTeacherAnimation(animation: stepId < steps.Count - 1
                ? TutorialTeacher.TeacherAnimation.StepCompleted
                : TutorialTeacher.TeacherAnimation.QuickGoodbye);

            yield return runningStep.OnStepPlayHideAnimation();
            runningStep.OnStepFinished();
            elapsedTimeInCurrentStep = Time.realtimeSinceStartup - elapsedTimeInCurrentStep;

            if (NeedToSendStats() && tutorialPath != TutorialPath.FromUserThatAlreadyDidTheTutorial)
                SendStepCompletedSegmentStats(configuration.tutorialVersion, tutorialPath, stepId + 1, runningStep.name, elapsedTimeInCurrentStep);

            Object.Destroy(runningStep.gameObject);
        }

        private bool NeedToSendStats() =>
            !configuration.debugRunTutorial && configuration.sendStats;

        private static void SetUserTutorialStepAsCompleted(TutorialFinishStep finishStepType) =>
            WebInterface.SaveUserTutorialStep(UserProfile.GetOwnUserProfile().tutorialStep | (int)finishStepType);

        internal IEnumerator MoveTeacher(Vector3 toPosition)
        {
            if (configuration.teacherRawImage == null)
                yield break;

            var t = 0f;

            Vector3 fromPosition = configuration.teacherRawImage.rectTransform.position;

            while (Vector3.Distance(configuration.teacherRawImage.rectTransform.position, toPosition) > 0)
            {
                t += configuration.teacherMovementSpeed * Time.deltaTime;

                configuration.teacherRawImage.rectTransform.position = t <= 1.0f
                    ? Vector3.Lerp(fromPosition, toPosition, configuration.teacherMovementCurve.Evaluate(t))
                    : toPosition;

                yield return null;
            }
        }

        private void IsSettingsHUDInitialized_OnChange(bool isInitialized, bool _)
        {
            if (isInitialized && hudController is { settingsPanelHud: { } })
            {
                hudController.settingsPanelHud.OnRestartTutorial -= OnRestartTutorial;
                hudController.settingsPanelHud.OnRestartTutorial += OnRestartTutorial;
            }
        }

        private void OnRestartTutorial()
        {
            SetTutorialDisabled();
            tutorialReset = true;

            SetTutorialEnabled(JsonUtility.ToJson(new TutorialInitializationMessage
            {
                fromDeepLink = false.ToString(),
                enableNewTutorialCamera = false.ToString(),
            }));
        }

        private static bool IsPlayerInScene()
        {
            IWorldState worldState = Environment.i.world.state;

            if (worldState == null || worldState.GetCurrentSceneNumber() == null)
                return false;

            return true;
        }

        private static bool IsPlayerInsideGenesisPlaza()
        {
            if (Environment.i.world == null)
                return false;

            IWorldState worldState = Environment.i.world.state;

            if (worldState == null || worldState.GetCurrentSceneNumber() == null)
                return false;

            var genesisPlazaBaseCoords = new Vector2Int(-9, -9);

            IParcelScene currentScene = worldState.GetScene(worldState.GetCurrentSceneNumber());

            return currentScene != null && currentScene.IsInsideSceneBoundaries(genesisPlazaBaseCoords);
        }

        private static void SendStepStartedSegmentStats(int version, TutorialPath tutorialPath, int stepNumber, string stepName)
        {
            WebInterface.AnalyticsPayload.Property[] properties =
            {
                new ("version", version.ToString()),
                new ("path", tutorialPath.ToString()),
                new ("step number", stepNumber.ToString()),
                new ("step name", StepNameForStatsMessage(stepName)),
            };

            WebInterface.ReportAnalyticsEvent("tutorial step started", properties);
        }

        private static void SendStepCompletedSegmentStats(int version, TutorialPath tutorialPath, int stepNumber, string stepName, float elapsedTime)
        {
            WebInterface.AnalyticsPayload.Property[] properties =
            {
                new ("version", version.ToString()),
                new ("path", tutorialPath.ToString()),
                new ("step number", stepNumber.ToString()),
                new ("step name", StepNameForStatsMessage(stepName)),
                new ("elapsed time", elapsedTime.ToString("0.00")),
            };

            WebInterface.ReportAnalyticsEvent("tutorial step completed", properties);
        }

        private void SendSkipTutorialSegmentStats(int version, string stepName)
        {
            WebInterface.AnalyticsPayload.Property[] properties =
            {
                new ("version", version.ToString()),
                new ("path", currentPath.ToString()),
                new ("step number", currentStepNumber.ToString()),
                new ("step name", StepNameForStatsMessage(stepName)),
                new ("elapsed time", (Time.realtimeSinceStartup - elapsedTimeInCurrentStep).ToString("0.00")),
            };

            WebInterface.ReportAnalyticsEvent("tutorial skipped", properties);
        }

        private static string StepNameForStatsMessage(string stepName) =>
            stepName.Replace("(Clone)", "").Replace("TutorialStep_", "");

        internal IEnumerator EagleEyeCameraRotation(float rotationSpeed)
        {
            while (true)
            {
                configuration.eagleEyeCamera.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed, Space.World);
                yield return null;
            }
        }

        private IEnumerator BlockPlayerCameraUntilBlendingIsFinished(bool hideUIs)
        {
            if (hideUIs)
                SetHudVisibility(false);

            CommonScriptableObjects.cameraBlocked.Set(true);

            yield return null;
            yield return new WaitUntil(() => !CommonScriptableObjects.cameraIsBlending.Get());

            CommonScriptableObjects.cameraBlocked.Set(false);

            if (!hideUIs)
                SetHudVisibility(true);

            void SetHudVisibility(bool isVisible)
            {
                hudController?.minimapHud?.SetVisibility(isVisible);
                hudController?.profileHud?.SetVisibility(isVisible);
            }
        }
    }
}
