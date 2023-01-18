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
        public List<TutorialStep> stepsOnGenesisPlaza = new ();

        [Header("Tutorial Steps from Deep Link")]
        public List<TutorialStep> stepsFromDeepLink = new ();

        [Header("Tutorial Steps from Reset Tutorial")]
        public List<TutorialStep> stepsFromReset = new ();

        [Header("Tutorial Steps from User That Already Did The Tutorial")]
        public List<TutorialStep> stepsFromUserThatAlreadyDidTheTutorial = new ();

        [Header("Teacher Configuration")]
        public float teacherMovementSpeed = 4f;
        public AnimationCurve teacherMovementCurve;

        [Header("Eagle Eye Camera Configuration")]
        public Vector3 eagleCamInitPosition = new (30, 30, -50);
        public Vector3 eagleCamInitLookAtPoint = new (0, 0, 0);
        public bool eagleCamRotationActived = true;
        public float eagleCamRotationSpeed = 1f;

        [Header("Debugging Configuration")]
        public bool debugRunTutorial;
        [Min(0)] public int debugStartingStepIndex;
        public bool debugOpenedFromDeepLink;

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

        public void ConfigureEagleEyeCamera(CinemachineVirtualCamera eagleEyeCamera)
        {
            this.eagleEyeCamera = eagleEyeCamera;
        }
    }
}
