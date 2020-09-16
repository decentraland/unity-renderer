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

            Assert.IsTrue(tutorialController.markTutorialAsCompleted);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromDeepLinkCorrectly()
        {
            ConfigureTutorialForDeepLink();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsFalse(tutorialController.markTutorialAsCompleted);
            Assert.IsTrue(tutorialController.alreadyOpenedFromDeepLink);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
        }

        [UnityTest]
        public IEnumerator ExecuteTutorialStepsFromGenesisPlazaAfterDeepLinkCorrectly()
        {
            ConfigureTutorialForGenesisPlazaAfterDeepLink();

            yield return tutorialController.StartTutorialFromStep(0);

            Assert.IsTrue(tutorialController.markTutorialAsCompleted);
            Assert.IsFalse(tutorialController.isRunning);
            Assert.IsNull(tutorialController.runningStep);
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
            tutorialController.stepsOnGenesisPlazaAfterDeepLink.Clear();
            tutorialController.timeBetweenSteps = 0f;
            tutorialController.debugRunTutorial = false;
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
            tutorialController.alreadyOpenedFromDeepLink = false;
            tutorialController.isRunning = true;
            tutorialController.markTutorialAsCompleted = false;
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
            tutorialController.markTutorialAsCompleted = true;
        }

        private void ConfigureTutorialForGenesisPlazaAfterDeepLink()
        {
            for (int i = 0; i < 5; i++)
            {
                tutorialController.stepsOnGenesisPlazaAfterDeepLink.Add(CreateNewFakeStep());
            }

            currentStepIndex = 0;
            currentSteps = tutorialController.stepsOnGenesisPlazaAfterDeepLink;

            tutorialController.playerIsInGenesisPlaza = true;
            tutorialController.alreadyOpenedFromDeepLink = true;
            tutorialController.isRunning = true;
            tutorialController.markTutorialAsCompleted = false;
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
        }
    }
}