using DCL.Tutorial;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using static DCL.Tutorial.TutorialController;

namespace DCL.Tutorial_Tests
{
    public class TutorialControllerShould
    {
        private TutorialConfigurator tutorialConfigurator;
        private TutorialController tutorialController;
        private int currentStepIndex = 0;
        private List<TutorialStep> currentSteps = new List<TutorialStep>();
        private Coroutine stepCoroutine;

        [SetUp]
        public void SetUp() { CreateAndConfigureTutorial(); }

        [TearDown]
        public void TearDown() { DestroyTutorial(); }

        [Test]
        public void SetTutorialEnabledCorrectly()
        {
            // Arrange
            bool fromDeepLink = false;
            bool enableNewTutorialCamera = true;
            TutorialType tutorialType = TutorialType.Initial;
            bool userAlreadyDidTheTutorial = false;

            tutorialController.isRunning = false;
            tutorialController.configuration.eagleCamRotationActived = true;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);
            tutorialController.userAlreadyDidTheTutorial = true;
            CommonScriptableObjects.allUIHidden.Set(true);
            CommonScriptableObjects.tutorialActive.Set(false);
            tutorialController.openedFromDeepLink = true;
            tutorialController.tutorialType = TutorialType.BuilderInWorld;
            NotificationsController.disableWelcomeNotification = false;

            bool onTutorialEnabledInvoked = false;
            tutorialController.OnTutorialEnabled += () => onTutorialEnabledInvoked = true;

            // Act
            tutorialController.SetupTutorial(fromDeepLink.ToString(), enableNewTutorialCamera.ToString(), tutorialType, userAlreadyDidTheTutorial);

            // Assert
            //Assert.IsTrue(tutorialController.isRunning);
            //Assert.IsFalse(tutorialController.configuration.eagleCamRotationActived);
            //Assert.AreEqual(0f, DataStore.i.virtualAudioMixer.sceneSFXVolume.Get());
            //Assert.AreEqual(userAlreadyDidTheTutorial, tutorialController.userAlreadyDidTheTutorial);
            //Assert.IsFalse(CommonScriptableObjects.allUIHidden.Get());
            //Assert.IsTrue(CommonScriptableObjects.tutorialActive.Get());
            //Assert.AreEqual(Convert.ToBoolean(fromDeepLink), tutorialController.openedFromDeepLink);
            //Assert.AreEqual(tutorialType, tutorialController.tutorialType);
            //Assert.IsTrue(NotificationsController.disableWelcomeNotification);
            //Assert.IsTrue(onTutorialEnabledInvoked);
        }

        [Test]
        public void SetTutorialDisabledCorrectly()
        {
            // Arrange
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            tutorialController.tutorialReset = true;
            tutorialController.isRunning = true;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(0f);
            NotificationsController.disableWelcomeNotification = true;
            CommonScriptableObjects.tutorialActive.Set(true);

            bool onTutorialDisabledInvoked = false;
            tutorialController.OnTutorialDisabled += () => onTutorialDisabledInvoked = true;

            // Act
            tutorialController.SetTutorialDisabled();

            // Assert
            Assert.IsFalse(CommonScriptableObjects.featureKeyTriggersBlocked.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(tutorialController.tutorialReset);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.AreEqual(1f, DataStore.i.virtualAudioMixer.sceneSFXVolume.Get());
            Assert.IsFalse(NotificationsController.disableWelcomeNotification);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.IsTrue(onTutorialDisabledInvoked);
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromGenesisPlazaCorrectly()
        {
            // Arrange
            ConfigureTutorialForGenesisPlaza();

            // Act
            yield return tutorialController.StartTutorialFromStep(0);

            // Assert
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromGenesisPlaza, tutorialController.currentPath);
        }

        [Test]
        public void SkipTutorialStepsFromGenesisPlazaCorrectly()
        {
            ConfigureTutorialForGenesisPlaza();

            tutorialController.SkipTutorial();

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromDeepLinkCorrectly()
        {
            ConfigureTutorialForDeepLink();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromDeepLink, tutorialController.currentPath);
        }

        [Test]
        public void SkipTutorialStepsFromDeepLinkCorrectly()
        {
            ConfigureTutorialForDeepLink();

            tutorialController.SkipTutorial();

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsForResetTutorialCorrectly()
        {
            ConfigureTutorialForResetTutorial();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(tutorialController.tutorialReset);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromResetTutorial, tutorialController.currentPath);
        }

        [Test]
        public void SkipTutorialStepsForResetTutorialCorrectly()
        {
            ConfigureTutorialForResetTutorial();

            tutorialController.SkipTutorial();

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(tutorialController.tutorialReset);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromUserThatAlreadyDidTheTutorialCorrectly()
        {
            ConfigureTutorialForUserThatAlreadyDidTheTutorial();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromUserThatAlreadyDidTheTutorial, tutorialController.currentPath);
        }

        [Test]
        public void SkipTutorialStepsFromUserThatAlreadyDidTheTutorialCorrectly()
        {
            ConfigureTutorialForUserThatAlreadyDidTheTutorial();

            tutorialController.SkipTutorial();

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromBuilderInWorldCorrectly()
        {
            // Arrange
            ConfigureTutorialForBuilderInWorld();

            // Act
            yield return tutorialController.StartTutorialFromStep(0);

            // Assert
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromBuilderInWorld, tutorialController.currentPath);
        }

        [Test]
        public void SkipTutorialStepsFromBuilderInWorldCorrectly()
        {
            ConfigureTutorialForBuilderInWorld();

            tutorialController.SkipTutorial();

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowHideTutorialTeacherCorrectly(bool showTeacher)
        {
            tutorialController.ShowTeacher3DModel(showTeacher);

            if (showTeacher)
                Assert.IsTrue(tutorialController.configuration.teacherRawImage.gameObject.activeSelf);
            else
                Assert.IsFalse(tutorialController.configuration.teacherRawImage.gameObject.activeSelf);
        }

        [Test]
        public void SetTutorialTeacherPositionCorrectly()
        {
            Vector3 oldPosition = tutorialController.configuration.teacherRawImage.rectTransform.position;
            tutorialController.SetTeacherPosition(new Vector2(10, 30), false);
            Assert.IsTrue(tutorialController.configuration.teacherRawImage.rectTransform.position != oldPosition);

            oldPosition = tutorialController.configuration.teacherRawImage.rectTransform.position;
            tutorialController.SetTeacherPosition(new Vector2(50, 20), false);
            Assert.IsTrue(tutorialController.configuration.teacherRawImage.rectTransform.position != oldPosition);
        }

        [Test]
        public void SetTeacherCanvasSortingOrderCorrectly()
        {
            // Arrange
            int testSortOrder = 1;
            tutorialController.configuration.teacherCanvas.sortingOrder = 0;

            // Act
            tutorialController.SetTeacherCanvasSortingOrder(testSortOrder);

            // Assert
            Assert.AreEqual(testSortOrder, tutorialController.configuration.teacherCanvas.sortingOrder);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEagleEyeCameraActiveCorrectly(bool isActive)
        {
            // Arrange
            tutorialController.configuration.eagleEyeCamera.gameObject.SetActive(!isActive);
            tutorialController.configuration.eagleEyeCamera.transform.position = Vector3.zero;
            tutorialController.configuration.eagleCamRotationActived = false;

            // Act
            tutorialController.SetEagleEyeCameraActive(isActive);

            // Assert
            Assert.AreEqual(isActive, tutorialController.configuration.eagleEyeCamera.gameObject.activeSelf);
            if (isActive)
                Assert.AreEqual(tutorialController.configuration.eagleCamInitPosition, tutorialController.configuration.eagleEyeCamera.transform.position);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RenderingStateChangedCorrectly(bool debugRunTutorial)
        {
            // Arrange
            tutorialController.configuration.debugRunTutorial = debugRunTutorial;
            tutorialController.currentStepIndex = -1;
            tutorialController.configuration.debugStartingStepIndex = 2;

            // Act
            tutorialController.OnRenderingStateChanged(true, false);

            // Assert
            if (debugRunTutorial)
                Assert.AreEqual(tutorialController.configuration.debugStartingStepIndex, tutorialController.currentStepIndex);
            else
                Assert.AreEqual(0, tutorialController.currentStepIndex);
        }

        [Test]
        public void MoveTeacherCorrectly()
        {
            // Arrange
            Vector3 toPosition = new Vector3(1, 1, 0);
            Vector3 initialPosition = new Vector3(2, 2, 0);
            tutorialController.configuration.teacherRawImage.rectTransform.position = initialPosition;

            // Act
            tutorialController.MoveTeacher(initialPosition, toPosition);

            // Assert
            Assert.AreNotEqual(initialPosition, tutorialController.configuration.teacherRawImage.rectTransform.position);
        }

        [Test]
        public void MoreMenuRestartTutorialCorrectly()
        {
            // Arrange
            tutorialController.tutorialReset = false;

            // Act
            tutorialController.MoreMenu_OnRestartTutorial();

            // Assert
            //Assert.IsTrue(tutorialController.tutorialReset);
        }

        [Test]
        public void RotateEagleEyeCameraCorrectly()
        {
            // Arrange
            Quaternion initialRotation = new Quaternion(0, 0, 0, 0);
            tutorialController.configuration.eagleEyeCamera.transform.rotation = initialRotation;

            // Act
            tutorialController.EagleEyeCameraRotation(3f);

            // Assert
            Assert.AreNotEqual(initialRotation, tutorialController.configuration.eagleEyeCamera.transform.rotation);
        }

        [UnityTest]
        public IEnumerator ExecuteAvatarJumpingStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(0, () =>
            {
                TutorialStep_AvatarJumping step = (TutorialStep_AvatarJumping)tutorialController.runningStep;
                step.jumpingInputAction.RaiseOnStarted();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteAvatarMovementStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(1, () =>
            {
                TutorialStep_AvatarMovement step = (TutorialStep_AvatarMovement)tutorialController.runningStep;
                step.timeRunning = step.minRunningTime;
            });
        }

        [UnityTest]
        public IEnumerator ExecuteBasicControlsStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(2, () =>
            {
                TutorialStep_BasicControls step = (TutorialStep_BasicControls)tutorialController.runningStep;
                step.OnOkButtonClick();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteCameraStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(3, () =>
            {
                TutorialStep_Camera step = (TutorialStep_Camera)tutorialController.runningStep;
                step.CameraMode_OnChange(CameraMode.ModeId.FirstPerson, CameraMode.ModeId.ThirdPerson);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteGenesisGreetingsStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(4, () =>
            {
                TutorialStep_GenesisGreetings step = (TutorialStep_GenesisGreetings)tutorialController.runningStep;
                step.OnOkButtonClick();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteGenesisGreetingsAfterDeepLinkStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(5, () =>
            {
                TutorialStep_GenesisGreetingsAfterDeepLink step = (TutorialStep_GenesisGreetingsAfterDeepLink)tutorialController.runningStep;
                step.OnOkButtonClick();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteMinimapTooltipStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(6, () =>
            {
                TutorialStep_MinimapTooltip step = (TutorialStep_MinimapTooltip)tutorialController.runningStep;
                step.stepIsFinished = true;
            });
        }

        [UnityTest]
        public IEnumerator ExecuteOpenControlsPanelStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(7, () =>
            {
                TutorialStep_OpenControlsPanel step = (TutorialStep_OpenControlsPanel)tutorialController.runningStep;
                step.ControlsHud_OnControlsOpened();
                step.ControlsHud_OnControlsClosed();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipBackpackButtonStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(8, () =>
            {
                TutorialStep_Tooltip_BackpackButton step = (TutorialStep_Tooltip_BackpackButton)tutorialController.runningStep;
                step.ProfileHud_OnOpen();
                step.ProfileHud_OnClose();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipExploreButtonStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(9, () =>
            {
                TutorialStep_Tooltip_ExploreButton step = (TutorialStep_Tooltip_ExploreButton)tutorialController.runningStep;
                step.ExploreHud_OnOpen();
                step.ExploreHud_OnClose();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipTaskbarMoreButtonStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(10, () =>
            {
                TutorialStep_Tooltip_TaskbarMoreButton step = (TutorialStep_Tooltip_TaskbarMoreButton)tutorialController.runningStep;
                step.MoreMenu_OnMoreMenuOpened(true);
                step.MoreMenu_OnMoreMenuOpened(false);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipRestartTutorialButtonStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(11, () =>
            {
                TutorialStep_Tooltip_RestartTutorialButton step = (TutorialStep_Tooltip_RestartTutorialButton)tutorialController.runningStep;
                step.MoreMenu_OnMoreMenuOpened(true);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipSocialFeaturesByOpeningWorldChatStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(12, () =>
            {
                TutorialStep_Tooltip_SocialFeatures step = (TutorialStep_Tooltip_SocialFeatures)tutorialController.runningStep;
                step.WorldChatWindowHud_OnOpen();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipSocialFeaturesByOpeningFriendsStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(12, () =>
            {
                TutorialStep_Tooltip_SocialFeatures step = (TutorialStep_Tooltip_SocialFeatures)tutorialController.runningStep;
                step.FriendsHud_OnFriendsOpened();
                step.FriendsHud_OnFriendsClosed();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipSocialFeaturesByStartingVoiceChatStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(12, () =>
            {
                TutorialStep_Tooltip_SocialFeatures step = (TutorialStep_Tooltip_SocialFeatures)tutorialController.runningStep;
                step.VoiceChatAction_OnStarted(DCLAction_Hold.VoiceChatRecording);
                step.VoiceChatAction_OnFinished(DCLAction_Hold.VoiceChatRecording);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipUsersAroundStepCorrectly()
        {
            CommonScriptableObjects.voiceChatDisabled.Set(false);

            yield return ExecuteAvatarSpecificTutorialStep(13, () =>
            {
                TutorialStep_Tooltip_UsersAround step = (TutorialStep_Tooltip_UsersAround)tutorialController.runningStep;
                step.UsersAroundListHud_OnOpen();
                CommonScriptableObjects.voiceChatDisabled.Set(true);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialCompletedStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(14, () =>
            {
                TutorialStep_TutorialCompleted step = (TutorialStep_TutorialCompleted)tutorialController.runningStep;
                step.OnHideAnimationFinish();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteWelcomeStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(15, () =>
            {
                TutorialStep_Welcome step = (TutorialStep_Welcome)tutorialController.runningStep;
                step.confirmInputAction.RaiseOnStarted();
            });
        }

        private void CreateAndConfigureTutorial()
        {
            tutorialConfigurator = GameObject.Instantiate(Resources.Load<GameObject>("TutorialConfigurator")).GetComponent<TutorialConfigurator>();
            tutorialConfigurator.configuration = ScriptableObject.Instantiate(Resources.Load<TutorialConfiguration>("TutorialConfigurationForTests"));
            tutorialConfigurator.ConfigureTutorial();
            tutorialController = tutorialConfigurator.tutorialController;
            tutorialController.configuration.timeBetweenSteps = 0f;
            tutorialController.configuration.sendStats = false;
            tutorialController.configuration.debugRunTutorial = false;
            tutorialController.tutorialReset = false;
            tutorialController.userAlreadyDidTheTutorial = false;
        }

        private void ClearCurrentSteps()
        {
            tutorialController.configuration.stepsOnGenesisPlaza.Clear();
            tutorialController.configuration.stepsFromDeepLink.Clear();
            tutorialController.configuration.stepsFromReset.Clear();
            tutorialController.configuration.stepsFromUserThatAlreadyDidTheTutorial.Clear();
        }

        private void DestroyTutorial()
        {
            foreach (var step in currentSteps)
            {
                GameObject.Destroy(step);
            }

            GameObject.Destroy(tutorialConfigurator.gameObject);
            GameObject.Destroy(tutorialController.tutorialContainerGO);
            tutorialController.Dispose();
            currentSteps.Clear();
            currentStepIndex = 0;
            tutorialController.configuration.debugRunTutorial = false;
            CoroutineStarter.Stop(stepCoroutine);
            stepCoroutine = null;
        }

        private void ConfigureTutorialForGenesisPlaza()
        {
            ClearCurrentSteps();

            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsOnGenesisPlaza.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsOnGenesisPlaza;

            tutorialController.tutorialType = TutorialType.Initial;
            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.tutorialReset = false;
            tutorialController.isRunning = true;
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForDeepLink()
        {
            ClearCurrentSteps();

            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromDeepLink.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromDeepLink;

            tutorialController.tutorialType = TutorialType.Initial;
            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.tutorialReset = false;
            tutorialController.openedFromDeepLink = true;
            tutorialController.isRunning = true;
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForResetTutorial()
        {
            ClearCurrentSteps();

            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromReset.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromReset;

            tutorialController.tutorialType = TutorialType.Initial;
            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.tutorialReset = true;
            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.isRunning = true;
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForUserThatAlreadyDidTheTutorial()
        {
            ClearCurrentSteps();

            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromUserThatAlreadyDidTheTutorial.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromUserThatAlreadyDidTheTutorial;

            tutorialController.tutorialType = TutorialType.Initial;
            tutorialController.userAlreadyDidTheTutorial = true;
            tutorialController.isRunning = true;
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForBuilderInWorld()
        {
            ClearCurrentSteps();

            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromBuilderInWorld.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromBuilderInWorld;

            tutorialController.tutorialType = TutorialType.BuilderInWorld;
            tutorialController.isRunning = true;
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private TutorialStep_Mock CreateNewFakeStep()
        {
            GameObject newStepObject = new GameObject("FakeStep");
            newStepObject.transform.parent = tutorialController.tutorialContainerGO.transform;
            TutorialStep_Mock newStep = newStepObject.AddComponent<TutorialStep_Mock>();
            newStep.letInstantiation = false;
            newStep.customOnStepStart = WaitForOnStepStart;
            newStep.customOnStepExecute = WaitForOnStepExecute;
            newStep.customOnStepPlayAnimationForHidding = WaitForOnStepPlayAnimationForHidding;
            newStep.customOnStepFinished = WaitForOnStepFinished;
            return newStep;
        }

        private void WaitForOnStepStart() { CheckRunningStep(); }

        private IEnumerator WaitForOnStepExecute()
        {
            CheckRunningStep();
            yield return null;
        }

        private IEnumerator WaitForOnStepPlayAnimationForHidding()
        {
            CheckRunningStep();
            yield return null;
        }

        private void WaitForOnStepFinished()
        {
            CheckRunningStep();
            currentStepIndex++;
        }

        private void CheckRunningStep()
        {
            Assert.IsTrue(tutorialController.isRunning);
            Assert.IsNotNull(tutorialController.runningStep);
            Assert.IsTrue(currentSteps[currentStepIndex] == tutorialController.runningStep);
            Assert.IsTrue(CommonScriptableObjects.tutorialActive.Get());
        }

        private IEnumerator ExecuteAvatarSpecificTutorialStep(int stepIndex, Action actionToFinishStep)
        {
            // Arrange
            TutorialStep stepToTest = tutorialController.configuration.stepsOnGenesisPlaza[stepIndex];
            ClearCurrentSteps();
            tutorialController.configuration.teacherRawImage = null;
            tutorialController.configuration.teacher = null;
            tutorialController.configuration.stepsOnGenesisPlaza.Add(stepToTest);
            tutorialController.tutorialType = TutorialType.Initial;
            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.tutorialReset = false;
            tutorialController.isRunning = true;
            tutorialController.runningStep = null;

            // Act
            stepCoroutine = CoroutineStarter.Start(tutorialController.StartTutorialFromStep(0));

            // Assert
            yield return new WaitUntil(() => tutorialController.runningStep != null, 10f);
            Assert.IsNotNull(tutorialController.runningStep);
            Component.Destroy(tutorialController.runningStep.stepAnimator);

            actionToFinishStep?.Invoke();
            yield return new WaitUntil(() => tutorialController.runningStep == null, 10f);
            Assert.IsNull(tutorialController.runningStep);
        }
    }
}