using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public HUDController hudController { get => HUDController.i; }

        public int currentStepIndex { get; internal set; }
        public event Action OnTutorialEnabled;
        public event Action OnTutorialDisabled;

        private const string PLAYER_PREFS_START_MENU_SHOWED = "StartMenuFeatureShowed";

        internal TutorialSettings configuration;
        internal TutorialView tutorialView;

        internal bool openedFromDeepLink = false;
        internal bool playerIsInGenesisPlaza = false;
        internal TutorialStep runningStep = null;
        internal bool tutorialReset = false;
        internal float elapsedTimeInCurrentStep = 0f;
        internal TutorialPath currentPath;
        internal int currentStepNumber;
        internal TutorialType tutorialType = TutorialType.Initial;
        internal int nextStepsToSkip = 0;

        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;
        private Coroutine eagleEyeRotationCoroutine;

        internal bool userAlreadyDidTheTutorial { get; set; }

        public TutorialController ()
        {
            tutorialView = CreateTutorialView();
            SetConfiguration(tutorialView.configuration);
        }

        internal TutorialView CreateTutorialView()
        {
            GameObject tutorialGO = GameObject.Instantiate(Resources.Load<GameObject>("TutorialView"));
            tutorialGO.name = "TutorialController";
            TutorialView tutorialView = tutorialGO.GetComponent<TutorialView>();
            tutorialView.ConfigureView(this);

            return tutorialView;
        }

        public void SetConfiguration(TutorialSettings configuration)
        {
            this.configuration = configuration;

            i = this;
            ShowTeacher3DModel(false);

            if (DataStore.i.settings.isInitialized.Get())
                IsSettingsHUDInitialized_OnChange(true, false);
            else
                DataStore.i.settings.isInitialized.OnChange += IsSettingsHUDInitialized_OnChange;

            if (configuration.debugRunTutorial)
            {
                SetTutorialEnabled(JsonUtility.ToJson(new TutorialInitializationMessage
                {
                    fromDeepLink = configuration.debugOpenedFromDeepLink.ToString(),
                    enableNewTutorialCamera = false.ToString()
                }));
            }
        }

        public void Dispose()
        {
            SetTutorialDisabled();

            DataStore.i.settings.isInitialized.OnChange -= IsSettingsHUDInitialized_OnChange;

            if (hudController != null &&
                hudController.settingsPanelHud != null)
            {
                hudController.settingsPanelHud.OnRestartTutorial -= OnRestartTutorial;
            }

            NotificationsController.disableWelcomeNotification = false;

            if (tutorialView != null)
                GameObject.Destroy(tutorialView.gameObject);
        }

        public void SetTutorialEnabled(string json)
        {
            TutorialInitializationMessage msg = JsonUtility.FromJson<TutorialInitializationMessage>(json);
            SetupTutorial(msg.fromDeepLink, msg.enableNewTutorialCamera, TutorialType.Initial);
        }

        public void SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(string json)
        {
            TutorialInitializationMessage msg = JsonUtility.FromJson<TutorialInitializationMessage>(json);

            // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
            if (PlayerPrefsUtils.GetInt(PLAYER_PREFS_START_MENU_SHOWED) == 1)
                return;

            SetupTutorial(false.ToString(), msg.enableNewTutorialCamera, TutorialType.Initial, true);
        }

        public void SetBuilderInWorldTutorialEnabled() { SetupTutorial(false.ToString(), false.ToString(), TutorialType.BuilderInWorld); }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        internal void SetupTutorial(string fromDeepLink, string enableNewTutorialCamera, TutorialType tutorialType, bool userAlreadyDidTheTutorial = false)
        {
            if (DataStore.i.common.isTutorialRunning.Get())
                return;

            if (Convert.ToBoolean(enableNewTutorialCamera))
            {
                configuration.eagleCamInitPosition = new Vector3(15, 115, -30);
                configuration.eagleCamInitLookAtPoint = new Vector3(16, 105, 6);
                configuration.eagleCamRotationActived = false;
            }

            DataStore.i.common.isTutorialRunning.Set(true);
            this.userAlreadyDidTheTutorial = userAlreadyDidTheTutorial;
            CommonScriptableObjects.allUIHidden.Set(false);
            CommonScriptableObjects.tutorialActive.Set(true);
            openedFromDeepLink = Convert.ToBoolean(fromDeepLink);
            this.tutorialType = tutorialType;

            hudController?.settingsPanelHud?.SetTutorialButtonEnabled(false);
            hudController?.profileHud?.HideProfileMenu();

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
                UnityEngine.Object.Destroy(runningStep.gameObject);
                runningStep = null;
            }

            if (teacherMovementCoroutine != null)
            {
                CoroutineStarter.Stop(teacherMovementCoroutine);
                teacherMovementCoroutine = null;
            }

            tutorialReset = false;
            DataStore.i.common.isTutorialRunning.Set(false);
            tutorialView.tutorialMusicHandler.StopTutorialMusic();
            ShowTeacher3DModel(false);
            WebInterface.SetDelightedSurveyEnabled(true);

            if (Environment.i != null && Environment.i.world != null)
            {
                WebInterface.SendSceneExternalActionEvent(Environment.i.world.state.GetCurrentSceneId(), "tutorial", "end");
            }

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
        public IEnumerator StartTutorialFromStep(int stepIndex)
        {
            if (!DataStore.i.common.isTutorialRunning.Get())
                yield break;

            if (runningStep != null)
            {
                runningStep.OnStepFinished();
                GameObject.Destroy(runningStep.gameObject);
                runningStep = null;
            }

            yield return new WaitUntil(IsPlayerInScene);

            playerIsInGenesisPlaza = IsPlayerInsideGenesisPlaza();
            
            switch (tutorialType)
            {
                case TutorialType.Initial:
                    if(playerIsInGenesisPlaza) tutorialView.tutorialMusicHandler.TryPlayingMusic();
                    if (userAlreadyDidTheTutorial)
                    {
                        yield return ExecuteSteps(TutorialPath.FromUserThatAlreadyDidTheTutorial, stepIndex);
                    }
                    else if (tutorialReset)
                    {
                        yield return ExecuteSteps(TutorialPath.FromResetTutorial, stepIndex);
                    }
                    else if (playerIsInGenesisPlaza)
                    {
                        yield return ExecuteSteps(TutorialPath.FromGenesisPlaza, stepIndex);
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
            if (!ignoreStatsSending && !configuration.debugRunTutorial && configuration.sendStats)
            {
                SendSkipTutorialSegmentStats(
                    configuration.tutorialVersion,
                    runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""));
            }

            int skipIndex = configuration.stepsOnGenesisPlaza.Count +
                            configuration.stepsFromDeepLink.Count +
                            configuration.stepsFromReset.Count +
                            configuration.stepsFromBuilderInWorld.Count +
                            configuration.stepsFromUserThatAlreadyDidTheTutorial.Count;

            CoroutineStarter.Start(StartTutorialFromStep(skipIndex));

            hudController?.taskbarHud?.SetVisibility(true);
        }

        /// <summary>
        /// Jump to a specific step.
        /// </summary>
        /// <param name="stepIndex">Step to jump.</param>
        public void GoToSpecificStep(string stepName)
        {
            int stepIndex = 0;
            switch (tutorialType)
            {
                case TutorialType.Initial:
                    if (userAlreadyDidTheTutorial)
                    {
                        stepIndex = configuration.stepsFromUserThatAlreadyDidTheTutorial.FindIndex(x => x.name == stepName);
                    }
                    else if (playerIsInGenesisPlaza || tutorialReset)
                    {
                        if (tutorialReset)
                        {
                            stepIndex = configuration.stepsFromReset.FindIndex(x => x.name == stepName);
                        }
                        else
                        {
                            stepIndex = configuration.stepsOnGenesisPlaza.FindIndex(x => x.name == stepName);
                        }
                    }
                    else if (openedFromDeepLink)
                    {
                        stepIndex = configuration.stepsFromDeepLink.FindIndex(x => x.name == stepName);
                    }
                    break;
                case TutorialType.BuilderInWorld:
                    stepIndex = configuration.stepsFromBuilderInWorld.FindIndex(x => x.name == stepName);
                    break;
            }

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
        public void SetNextSkippedSteps(int skippedSteps) { nextStepsToSkip = skippedSteps; }

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
            {
                CoroutineStarter.Stop(eagleEyeRotationCoroutine);
            }
        }

        internal void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!renderingEnabled)
                return;

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;

            if (configuration.debugRunTutorial)
                currentStepIndex = configuration.debugStartingStepIndex >= 0 ? configuration.debugStartingStepIndex : 0;
            else
                currentStepIndex = 0;

            PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Reset);
            executeStepsCoroutine = CoroutineStarter.Start(StartTutorialFromStep(currentStepIndex));
        }

        private IEnumerator ExecuteSteps(TutorialPath tutorialPath, int startingStepIndex)
        {
            List<TutorialStep> steps = new List<TutorialStep>();

            switch (tutorialPath)
            {
                case TutorialPath.FromGenesisPlaza:
                    steps = configuration.stepsOnGenesisPlaza;
                    break;
                case TutorialPath.FromDeepLink:
                    steps = configuration.stepsFromDeepLink;
                    break;
                case TutorialPath.FromResetTutorial:
                    steps = configuration.stepsFromReset;
                    break;
                case TutorialPath.FromBuilderInWorld:
                    steps = configuration.stepsFromBuilderInWorld;
                    break;
                case TutorialPath.FromUserThatAlreadyDidTheTutorial:
                    steps = configuration.stepsFromUserThatAlreadyDidTheTutorial;
                    break;
            }

            currentPath = tutorialPath;

            elapsedTimeInCurrentStep = 0f;
            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                if (nextStepsToSkip > 0)
                {
                    nextStepsToSkip--;
                    continue;
                }

                var stepPrefab = steps[i];

                // TODO (Santi): This a TEMPORAL fix. It will be removed when we refactorize the tutorial system in order to make it compatible with incremental features.
                if (stepPrefab is TutorialStep_Tooltip_ExploreButton &&
                    !DataStore.i.exploreV2.isInitialized.Get())
                    continue;

                if (stepPrefab.letInstantiation)
                    runningStep = GameObject.Instantiate(stepPrefab, tutorialView.transform).GetComponent<TutorialStep>();
                else
                    runningStep = steps[i];

                runningStep.gameObject.name = runningStep.gameObject.name.Replace("(Clone)", "");
                currentStepIndex = i;

                elapsedTimeInCurrentStep = Time.realtimeSinceStartup;
                currentStepNumber = i + 1;

                if (!configuration.debugRunTutorial && configuration.sendStats)
                {
                    SendStepStartedSegmentStats(
                        configuration.tutorialVersion,
                        tutorialPath,
                        i + 1,
                        runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""));
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

                if (!configuration.debugRunTutorial &&
                    configuration.sendStats &&
                    tutorialPath != TutorialPath.FromUserThatAlreadyDidTheTutorial)
                {
                    SendStepCompletedSegmentStats(
                        configuration.tutorialVersion,
                        tutorialPath,
                        i + 1,
                        runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""),
                        elapsedTimeInCurrentStep);
                }

                GameObject.Destroy(runningStep.gameObject);

                if (i < steps.Count - 1 && configuration.timeBetweenSteps > 0)
                    yield return new WaitForSeconds(configuration.timeBetweenSteps);
            }

            if (!configuration.debugRunTutorial &&
                tutorialPath != TutorialPath.FromBuilderInWorld &&
                tutorialPath != TutorialPath.FromUserThatAlreadyDidTheTutorial)
            {
                SetUserTutorialStepAsCompleted(TutorialFinishStep.NewTutorialFinished);
            }

            runningStep = null;

            SetTutorialDisabled();
        }

        private void SetUserTutorialStepAsCompleted(TutorialFinishStep finishStepType) { WebInterface.SaveUserTutorialStep(UserProfile.GetOwnUserProfile().tutorialStep | (int) finishStepType); }

        internal IEnumerator MoveTeacher(Vector3 toPosition)
        {
            if (configuration.teacherRawImage == null)
                yield break;

            float t = 0f;
          
            Vector3 fromPosition = configuration.teacherRawImage.rectTransform.position;
            
            while (Vector3.Distance(configuration.teacherRawImage.rectTransform.position, toPosition) > 0)
            {
                t += configuration.teacherMovementSpeed * Time.deltaTime;
                if (t <= 1.0f)
                    configuration.teacherRawImage.rectTransform.position = Vector3.Lerp(fromPosition, toPosition, configuration.teacherMovementCurve.Evaluate(t));
                else
                    configuration.teacherRawImage.rectTransform.position = toPosition;
                yield return null;
            }
        }

        private void IsSettingsHUDInitialized_OnChange(bool current, bool previous)
        {
            if (current &&
                hudController != null &&
                hudController.settingsPanelHud != null)
            {
                hudController.settingsPanelHud.OnRestartTutorial -= OnRestartTutorial;
                hudController.settingsPanelHud.OnRestartTutorial += OnRestartTutorial;
            }
        }

        internal void OnRestartTutorial()
        {
            SetTutorialDisabled();
            tutorialReset = true;
            SetTutorialEnabled(JsonUtility.ToJson(new TutorialInitializationMessage
            {
                fromDeepLink = false.ToString(),
                enableNewTutorialCamera = false.ToString()
            }));
        }

        internal bool IsPlayerInScene()
        {
            IWorldState worldState = Environment.i.world.state;

            if (worldState == null || worldState.GetCurrentSceneId() == null)
                return false;

            return true;
        }

        internal static bool IsPlayerInsideGenesisPlaza()
        {
            if (Environment.i.world == null)
                return false;

            IWorldState worldState = Environment.i.world.state;

            if (worldState == null || worldState.GetCurrentSceneId() == null)
                return false;

            Vector2Int genesisPlazaBaseCoords = new Vector2Int(-9, -9);

            var currentScene = worldState.GetScene(worldState.GetCurrentSceneId());

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
    }
}