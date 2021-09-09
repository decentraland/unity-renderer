using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialSettings", menuName = "TutorialSettings")]
    public class TutorialSettings : ScriptableObject
    {
        [Header("General Configuration")]
        public int tutorialVersion = 1;
        public float timeBetweenSteps = 0.5f;
        public bool sendStats = true;

        [Header("Tutorial Steps on Genesis Plaza")]
        public List<TutorialStep> stepsOnGenesisPlaza = new List<TutorialStep>();

        [Header("Tutorial Steps from Deep Link")]
        public List<TutorialStep> stepsFromDeepLink = new List<TutorialStep>();

        [Header("Tutorial Steps from Reset Tutorial")]
        public List<TutorialStep> stepsFromReset = new List<TutorialStep>();

        [Header("Tutorial Steps from Builder In World")]
        public List<TutorialStep> stepsFromBuilderInWorld = new List<TutorialStep>();

        [Header("Tutorial Steps from User That Already Did The Tutorial")]
        public List<TutorialStep> stepsFromUserThatAlreadyDidTheTutorial = new List<TutorialStep>();

        [Header("Teacher Configuration")]
        public float teacherMovementSpeed = 4f;
        public AnimationCurve teacherMovementCurve;

        [Header("Eagle Eye Camera Configuration")]
        public Vector3 eagleCamInitPosition = new Vector3(30, 30, -50);
        public Vector3 eagleCamInitLookAtPoint = new Vector3(0, 0, 0);
        public bool eagleCamRotationActived = true;
        public float eagleCamRotationSpeed = 1f;

        [Header("Debugging Configuration")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;
        public bool debugOpenedFromDeepLink = false;

        internal Camera teacherCamera;
        internal RawImage teacherRawImage;
        internal TutorialTeacher teacher;
        internal Canvas teacherCanvas;
        internal CinemachineVirtualCamera eagleEyeCamera;

        public void ConfigureTeacher(Camera teacherCamera, RawImage teacherRawImage, TutorialTeacher teacher, Canvas teacherCanvas)
        {
            this.teacherCamera = teacherCamera;
            this.teacherRawImage = teacherRawImage;
            this.teacher = teacher;
            this.teacherCanvas = teacherCanvas;
        }

        public void ConfigureEagleEyeCamera(CinemachineVirtualCamera eagleEyeCamera) { this.eagleEyeCamera = eagleEyeCamera; }
    }
}