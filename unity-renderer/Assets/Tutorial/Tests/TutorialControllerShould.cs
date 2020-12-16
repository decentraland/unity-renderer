using DCL.Tutorial;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Tutorial_Tests
{
    public class TutorialControllerShould
    {
        private TutorialController tutorialController;
        private int currentStepIndex = 0;
        private List<TutorialStep> currentSteps = new List<TutorialStep>();

        [SetUp]
        public void SetUp()
        {
            CreateAndConfigureTutorial();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyTutorial();
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
            Assert.IsTrue(tutorialController.teacherRawImage.gameObject.activeSelf);

            tutorialController.ShowTeacher3DModel(false);
            Assert.IsFalse(tutorialController.teacherRawImage.gameObject.activeSelf);
        }

        [Test]
        public void SetTutorialTeacherPositionCorrectly()
        {
            Vector3 oldPosition = tutorialController.teacherRawImage.rectTransform.position;
            tutorialController.SetTeacherPosition(new Vector2(10, 30), false);
            Assert.IsTrue(tutorialController.teacherRawImage.rectTransform.position != oldPosition);

            oldPosition = tutorialController.teacherRawImage.rectTransform.position;
            tutorialController.SetTeacherPosition(new Vector2(50, 20), false);
            Assert.IsTrue(tutorialController.teacherRawImage.rectTransform.position != oldPosition);
        }

        private void CreateAndConfigureTutorial()
        {
            tutorialController = GameObject.Instantiate(Resources.Load<GameObject>("TutorialController")).GetComponent<TutorialController>();
            tutorialController.stepsOnGenesisPlaza.Clear();
            tutorialController.stepsFromDeepLink.Clear();
            tutorialController.stepsFromReset.Clear();
            tutorialController.stepsFromUserThatAlreadyDidTheTutorial.Clear();
            tutorialController.timeBetweenSteps = 0f;
            tutorialController.sendStats = false;
            tutorialController.debugRunTutorial = false;
            tutorialController.tutorialReset = false;
            tutorialController.userAlreadyDidTheTutorial = false;
        }

        private void DestroyTutorial()
        {
            foreach (var step in currentSteps)
            {
                GameObject.Destroy(step);
            }

            GameObject.Destroy(tutorialController.gameObject);
            currentSteps.Clear();
            currentStepIndex = 0;
        }

        private void ConfigureTutorialForGenesisPlaza()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsOnGenesisPlaza.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.stepsOnGenesisPlaza;

            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForDeepLink()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsFromDeepLink.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.stepsFromDeepLink;

            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.openedFromDeepLink = true;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForResetTutorial()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsFromReset.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.stepsFromReset;

            tutorialController.tutorialReset = true;
            tutorialController.playerIsInGenesisPlaza = false;
            tutorialController.isRunning = true;
            CommonScriptableObjects.tutorialActive.Set(true);
        }

        private void ConfigureTutorialForUserThatAlreadyDidTheTutorial()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsFromUserThatAlreadyDidTheTutorial.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.stepsFromUserThatAlreadyDidTheTutorial;

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

        private void WaitForOnStepStart()
        {
            CheckRunningStep();
        }

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