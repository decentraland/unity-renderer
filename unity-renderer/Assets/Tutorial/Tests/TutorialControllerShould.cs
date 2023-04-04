using DCL.Tutorial;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using DCL.CameraTool;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using static DCL.Tutorial.TutorialController;
using Object = UnityEngine.Object;

namespace DCL.Tutorial_Tests
{
    [Category("Legacy")]
    public class TutorialControllerShould : IntegrationTestSuite
    {
        private TutorialView tutorialView;
        private TutorialController tutorialController;
        private int currentStepIndex = 0;
        private List<TutorialStep> currentSteps = new List<TutorialStep>();
        private Coroutine stepCoroutine;
        private ParcelScene genesisPlazaSimulator;
        private readonly Vector2Int genesisPlazaLocation = new Vector2Int(-9, -9);

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            genesisPlazaSimulator = TestUtils.CreateTestScene(new LoadParcelScenesMessage.UnityParcelScene() {basePosition = genesisPlazaLocation});
            genesisPlazaSimulator.isPersistent = false;
            IWorldState worldState = Environment.i.world.state;
            worldState.TryGetScene(Arg.Any<int>(), out Arg.Any<IParcelScene>()).Returns(param => param[1] = genesisPlazaSimulator);

            CreateAndConfigureTutorial();
        }

        protected override IEnumerator TearDown()
        {
            DestroyTutorial();
            yield return base.TearDown();
        }

        [Test]
        public void SetTutorialEnabledCorrectly()
        {
            // Arrange
            bool fromDeepLink = false;
            bool enableNewTutorialCamera = true;
            bool userAlreadyDidTheTutorial = false;

            DataStore.i.common.isTutorialRunning.Set(false);
            tutorialController.configuration.eagleCamRotationActived = true;
            DataStore.i.virtualAudioMixer.sceneSFXVolume.Set(1f);
            tutorialController.userAlreadyDidTheTutorial = true;
            CommonScriptableObjects.allUIHidden.Set(true);
            CommonScriptableObjects.tutorialActive.Set(false);
            tutorialController.openedFromDeepLink = true;
            NotificationsController.disableWelcomeNotification = false;
            CommonScriptableObjects.rendererState.Set(false);

            bool onTutorialEnabledInvoked = false;
            tutorialController.OnTutorialEnabled += () => onTutorialEnabledInvoked = true;

            // Act
            tutorialController.SetupTutorial(fromDeepLink.ToString(), enableNewTutorialCamera.ToString(), userAlreadyDidTheTutorial);

            // Assert
            Assert.IsTrue(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsFalse(tutorialController.configuration.eagleCamRotationActived);
            Assert.AreEqual(userAlreadyDidTheTutorial, tutorialController.userAlreadyDidTheTutorial);
            Assert.IsFalse(CommonScriptableObjects.allUIHidden.Get());
            Assert.IsTrue(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(Convert.ToBoolean(fromDeepLink), tutorialController.openedFromDeepLink);
            Assert.IsTrue(NotificationsController.disableWelcomeNotification);
            Assert.IsTrue(onTutorialEnabledInvoked);
        }

        [Test]
        public void SetTutorialDisabledCorrectly()
        {
            // Arrange
            CommonScriptableObjects.featureKeyTriggersBlocked.Set(true);
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            tutorialController.tutorialReset = true;
            DataStore.i.common.isTutorialRunning.Set(true);
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
            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
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
            yield return  null;

            // Assert
            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromGenesisPlaza, tutorialController.currentPath);
        }

        [UnityTest]
        public IEnumerator MusicPlayingWhenStartingFromGenesisPlaza()
        {
            // Arrange
            bool fromDeepLink = false;
            bool enableNewTutorialCamera = true;
            bool userAlreadyDidTheTutorial = false;

            // Act
            tutorialController.SetupTutorial(fromDeepLink.ToString(), enableNewTutorialCamera.ToString(), userAlreadyDidTheTutorial);
            yield return null;

            // Assert
            Assert.AreEqual(0f, DataStore.i.virtualAudioMixer.sceneSFXVolume.Get());
        }

        [UnityTest]
        public IEnumerator MusicNotPlayingWhenStartingFromDeepLink()
        {
            // Arrange
            bool fromDeepLink = true;
            bool enableNewTutorialCamera = true;
            bool userAlreadyDidTheTutorial = false;
            if(genesisPlazaSimulator.parcels.Contains(genesisPlazaLocation)) genesisPlazaSimulator.parcels.Remove(genesisPlazaLocation);

            // Act
            tutorialController.SetupTutorial(fromDeepLink.ToString(), enableNewTutorialCamera.ToString(), userAlreadyDidTheTutorial);
            yield return null;


            // Assert
            Assert.AreEqual(1f, DataStore.i.virtualAudioMixer.sceneSFXVolume.Get());
        }

        [UnityTest]
        public IEnumerator SkipTutorialStepsFromGenesisPlazaCorrectly()
        {
            ConfigureTutorialForGenesisPlaza();

            tutorialController.SkipTutorial();
            yield return null;

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromDeepLinkCorrectly()
        {
            ConfigureTutorialForDeepLink();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromDeepLink, tutorialController.currentPath);
        }

        [UnityTest]
        public IEnumerator SkipTutorialStepsFromDeepLinkCorrectly()
        {
            ConfigureTutorialForDeepLink();

            tutorialController.SkipTutorial();
            yield return  null;

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsForResetTutorialCorrectly()
        {
            ConfigureTutorialForResetTutorial();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(tutorialController.tutorialReset);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromResetTutorial, tutorialController.currentPath);
        }

        [UnityTest]
        public IEnumerator SkipTutorialStepsForResetTutorialCorrectly()
        {
            ConfigureTutorialForResetTutorial();

            tutorialController.SkipTutorial();
            yield return null;

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(tutorialController.tutorialReset);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromUserThatAlreadyDidTheTutorialCorrectly()
        {
            ConfigureTutorialForUserThatAlreadyDidTheTutorial();

            yield return tutorialController.StartTutorialFromStep(0);
            yield return null;

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(TutorialPath.FromUserThatAlreadyDidTheTutorial, tutorialController.currentPath);
        }

        [UnityTest]
        public IEnumerator SkipTutorialStepsFromUserThatAlreadyDidTheTutorialCorrectly()
        {
            ConfigureTutorialForUserThatAlreadyDidTheTutorial();

            tutorialController.SkipTutorial();
            yield return null;

            Assert.IsFalse(DataStore.i.common.isTutorialRunning.Get());
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
            Assert.IsFalse(tutorialController.playerIsInGenesisPlaza);
            if (debugRunTutorial)
                Assert.AreEqual(tutorialController.configuration.debugStartingStepIndex, tutorialController.currentStepIndex);
            else
                Assert.AreEqual(0, tutorialController.currentStepIndex);
        }

        [UnityTest]
        public IEnumerator MoveTeacherCorrectly()
        {
            // Arrange
            Vector3 toPosition = new Vector3(1, 1, 0);
            Vector3 initialPosition = new Vector3(2, 2, 0);

            // Act
            stepCoroutine = CoroutineStarter.Start(tutorialController.MoveTeacher(toPosition));
            yield return null;

            // Assert
            Assert.AreNotEqual(initialPosition, tutorialController.configuration.teacherRawImage.rectTransform.position);
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
        public IEnumerator ExecuteOpenControlsPanelStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(6, () =>
            {
                TutorialStep_OpenControlsPanel step = (TutorialStep_OpenControlsPanel)tutorialController.runningStep;
                step.ControlsHud_OnControlsOpened();
                step.ControlsHud_OnControlsClosed();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipBackpackButtonStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(7, () =>
            {
                TutorialStep_Tooltip_BackpackButton step = (TutorialStep_Tooltip_BackpackButton)tutorialController.runningStep;
                step.ProfileHud_OnOpen();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipExploreButtonStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(8, () =>
            {
                TutorialStep_Tooltip_ExploreButton step = (TutorialStep_Tooltip_ExploreButton)tutorialController.runningStep;
                step.ExploreV2IsOpenChanged(true, false);
                step.ExploreV2IsOpenChanged(false, true);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipSocialFeaturesByOpeningWorldChatStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(9, () =>
            {
                TutorialStep_Tooltip_SocialFeatures step = (TutorialStep_Tooltip_SocialFeatures)tutorialController.runningStep;
                step.WorldChatWindowHud_OnOpen();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipSocialFeaturesByOpeningFriendsStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(9, () =>
            {
                TutorialStep_Tooltip_SocialFeatures step = (TutorialStep_Tooltip_SocialFeatures)tutorialController.runningStep;
                step.FriendsHud_OnFriendsOpened();
                step.FriendsHud_OnFriendsClosed();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTooltipSocialFeaturesByStartingVoiceChatStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(9, () =>
            {
                TutorialStep_Tooltip_SocialFeatures step = (TutorialStep_Tooltip_SocialFeatures)tutorialController.runningStep;
                step.VoiceChatAction_OnStarted(DCLAction_Hold.VoiceChatRecording);
            });
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialCompletedStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(10, () =>
            {
                TutorialStep_TutorialCompleted step = (TutorialStep_TutorialCompleted)tutorialController.runningStep;
                step.okPressed = true;
            });
        }

        [UnityTest]
        public IEnumerator ExecuteWelcomeStepCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(11, () =>
            {
                TutorialStep_Welcome step = (TutorialStep_Welcome)tutorialController.runningStep;
                step.confirmInputAction.RaiseOnStarted();
            });
        }

        [UnityTest]
        public IEnumerator ExecuteLockTheCursorCorrectly()
        {
            yield return ExecuteAvatarSpecificTutorialStep(12, () =>
            {
                TutorialStep_LockTheCursor step = (TutorialStep_LockTheCursor)tutorialController.runningStep;
                step.mouseCatcher = null;
                step.OnShowAnimationFinish();
                step.OnHideAnimationFinish();
            });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TutorialStepNoSkipInputActionOnFinishedCorrectly(bool mainSectionActivated)
        {
            // Arrange
            TutorialStep testStep = new GameObject().AddComponent<TutorialStep>();
            testStep.mainSection = new GameObject();
            testStep.skipTutorialSection = new GameObject();
            testStep.blockSkipActions = false;
            testStep.mainSection.SetActive(mainSectionActivated);
            testStep.skipTutorialSection.SetActive(!mainSectionActivated);

            // Act
            testStep.NoSkipInputAction_OnFinished(DCLAction_Hold.DefaultConfirmAction);

            // Assert
            if (mainSectionActivated)
            {
                Assert.IsFalse(testStep.mainSection.activeSelf);
                Assert.IsTrue(testStep.skipTutorialSection.activeSelf);
            }
            else
            {
                Assert.IsTrue(testStep.mainSection.activeSelf);
                Assert.IsFalse(testStep.skipTutorialSection.activeSelf);
            }
        }

        private void CreateAndConfigureTutorial()
        {
            tutorialController = new TutorialController(DataStore.i.common, DataStore.i.settings, DataStore.i.exploreV2);
            tutorialView = tutorialController.tutorialView;

            // NOTE(Brian): Avoid AudioListener warning
            tutorialView.gameObject.AddComponent<AudioListener>();

            tutorialView.configuration = Object.Instantiate(AssetDatabase.LoadAssetAtPath<TutorialSettings>("Assets/Tutorial/Tests/TutorialConfigurationForTests.asset"));
            tutorialView.ConfigureView(tutorialController);
            tutorialController.SetConfiguration(tutorialView.configuration);
            tutorialController = tutorialView.tutorialController;
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
                Object.Destroy(step);
            }

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

            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.playerIsInGenesisPlaza = true;
            if(!genesisPlazaSimulator.parcels.Contains(genesisPlazaLocation)) genesisPlazaSimulator.parcels.Add(genesisPlazaLocation);
            tutorialController.tutorialReset = false;
            DataStore.i.common.isTutorialRunning.Set(true);
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

            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.playerIsInGenesisPlaza = false;
            if(genesisPlazaSimulator.parcels.Contains(genesisPlazaLocation)) genesisPlazaSimulator.parcels.Remove(genesisPlazaLocation);
            tutorialController.tutorialReset = false;
            tutorialController.openedFromDeepLink = true;
            DataStore.i.common.isTutorialRunning.Set(true);
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

            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.tutorialReset = true;
            tutorialController.playerIsInGenesisPlaza = false;
            if(genesisPlazaSimulator.parcels.Contains(genesisPlazaLocation)) genesisPlazaSimulator.parcels.Remove(genesisPlazaLocation);
            DataStore.i.common.isTutorialRunning.Set(true);
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

            tutorialController.userAlreadyDidTheTutorial = true;
            DataStore.i.common.isTutorialRunning.Set(true);
            tutorialController.runningStep = new GameObject().AddComponent<TutorialStep>();
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private TutorialStep_Mock CreateNewFakeStep()
        {
            GameObject newStepObject = new GameObject("FakeStep");
            newStepObject.transform.parent = tutorialController.tutorialView.transform;
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
            Assert.IsTrue(DataStore.i.common.isTutorialRunning.Get());
            Assert.IsNotNull(tutorialController.runningStep);
            Assert.IsTrue(currentSteps[currentStepIndex] == tutorialController.runningStep);
            Assert.IsTrue(CommonScriptableObjects.tutorialActive.Get());
        }

        private IEnumerator ExecuteAvatarSpecificTutorialStep(int stepIndex, Action actionToFinishStep)
        {
            // Arrange
            DataStore.i.exploreV2.isInitialized.Set(true);
            TutorialStep stepToTest = tutorialController.configuration.stepsOnGenesisPlaza[stepIndex];
            ClearCurrentSteps();
            tutorialController.configuration.teacherRawImage = null;
            tutorialController.configuration.teacher = null;
            tutorialController.configuration.stepsOnGenesisPlaza.Add(stepToTest);
            tutorialController.userAlreadyDidTheTutorial = false;
            tutorialController.playerIsInGenesisPlaza = true;
            if(!genesisPlazaSimulator.parcels.Contains(genesisPlazaLocation)) genesisPlazaSimulator.parcels.Add(genesisPlazaLocation);
            tutorialController.tutorialReset = false;
            DataStore.i.common.isTutorialRunning.Set(true);
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
