using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    public class TutorialView : MonoBehaviour
    {
        [Header("General Configuration")]
        [SerializeField]
        internal TutorialSettings configuration;

        [Header("3D Model Teacher references")]
        [SerializeField]
        internal Camera teacherCamera;

        [SerializeField]
        internal RawImage teacherRawImage;

        [SerializeField]
        internal TutorialTeacher teacher;

        [SerializeField]
        internal Canvas teacherCanvas;

        [Header("Eagle Eye Camera references")]
        [SerializeField]
        internal CinemachineVirtualCamera eagleEyeCamera;

        internal TutorialController tutorialController;

        internal void ConfigureView(TutorialController tutorialController)
        {
            this.tutorialController = tutorialController;
            configuration.ConfigureTeacher(teacherCamera, teacherRawImage, teacher, teacherCanvas);
            configuration.ConfigureEagleEyeCamera(eagleEyeCamera);
        }

        public void SetTutorialEnabled(string json) { tutorialController.SetTutorialEnabled(json); }

        public void SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(string json) { tutorialController.SetTutorialEnabledForUsersThatAlreadyDidTheTutorial(json); }
    }
}