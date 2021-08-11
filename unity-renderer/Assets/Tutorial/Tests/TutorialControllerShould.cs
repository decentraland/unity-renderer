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

        [SetUp]
        public void SetUp() { CreateAndConfigureTutorial(); }

        [TearDown]
        public void TearDown() { DestroyTutorial(); }

        [Test]
        public void SetTutorialEnabledCorrectly()
        {
            // Arrange
            bool fromDeepLink = false;
            bool enableNewTutorialCamera = false;
            TutorialType tutorialType = TutorialType.Initial;
            bool userAlreadyDidTheTutorial = false;

            tutorialController.isRunning = false;
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
            Assert.IsTrue(tutorialController.isRunning);
            Assert.AreEqual(0f, DataStore.i.virtualAudioMixer.sceneSFXVolume.Get());
            Assert.AreEqual(userAlreadyDidTheTutorial, tutorialController.userAlreadyDidTheTutorial);
            Assert.IsFalse(CommonScriptableObjects.allUIHidden.Get());
            Assert.IsTrue(CommonScriptableObjects.tutorialActive.Get());
            Assert.AreEqual(Convert.ToBoolean(fromDeepLink), tutorialController.openedFromDeepLink);
            Assert.AreEqual(tutorialType, tutorialController.tutorialType);
            Assert.IsTrue(NotificationsController.disableWelcomeNotification);
            Assert.IsTrue(onTutorialEnabledInvoked);
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromGenesisPlazaCorrectly()
        {
            ConfigureTutorialForGenesisPlaza();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
            Assert.IsFalse(CommonScriptableObjects.tutorialActive.Get());
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

        [Test]
        public void ShowHideTutorialTeacherCorrectly()
        {
            tutorialController.ShowTeacher3DModel(true);
            Assert.IsTrue(tutorialController.configuration.teacherRawImage.gameObject.activeSelf);

            tutorialController.ShowTeacher3DModel(false);
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

        private void CreateAndConfigureTutorial()
        {
            tutorialConfigurator = GameObject.Instantiate(Resources.Load<GameObject>("TutorialConfigurator")).GetComponent<TutorialConfigurator>();
            tutorialConfigurator.configuration = Resources.Load<TutorialConfiguration>("TutorialConfigurationForTests");
            tutorialConfigurator.ConfigureTutorial();
            tutorialController = tutorialConfigurator.tutorialController;
            tutorialController.configuration.stepsOnGenesisPlaza.Clear();
            tutorialController.configuration.stepsFromDeepLink.Clear();
            tutorialController.configuration.stepsFromReset.Clear();
            tutorialController.configuration.stepsFromUserThatAlreadyDidTheTutorial.Clear();
            tutorialController.configuration.timeBetweenSteps = 0f;
            tutorialController.configuration.sendStats = false;
            tutorialController.configuration.debugRunTutorial = false;
            tutorialController.tutorialReset = false;
            tutorialController.userAlreadyDidTheTutorial = false;
        }

        private void DestroyTutorial()
        {
            foreach (var step in currentSteps)
            {
                GameObject.Destroy(step);
            }

            GameObject.Destroy(tutorialController.tutorialContainerGO);
            tutorialController.Dispose();
            currentSteps.Clear();
            currentStepIndex = 0;
        }

        private void ConfigureTutorialForGenesisPlaza()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsOnGenesisPlaza.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsOnGenesisPlaza;

            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForDeepLink()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromDeepLink.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromDeepLink;

            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.openedFromDeepLink = true;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForResetTutorial()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromReset.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromReset;

            tutorialController.tutorialReset = true;
            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForUserThatAlreadyDidTheTutorial()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.configuration.stepsFromUserThatAlreadyDidTheTutorial.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.configuration.stepsFromUserThatAlreadyDidTheTutorial;

            tutorialController.userAlreadyDidTheTutorial = true;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private TutorialStep_Mock CreateNewFakeStep()
        {
            GameObject newStepObject = new GameObject("FakeStep");
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
    }
}