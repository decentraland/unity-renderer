using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    public class TutorialConfigurator : MonoBehaviour
    {
        [Header("General Configuration")]
        [SerializeField]
        internal TutorialConfiguration configuration;

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

        private void Awake() { ConfigureTutorial(); }

        private void OnDestroy() { tutorialController.Dispose(); }

        internal void ConfigureTutorial()
        {
            if (tutorialController != null)
                tutorialController.Dispose();

            configuration.ConfigureTeacher(teacherCamera, teacherRawImage, teacher, teacherCanvas);
            configuration.ConfigureEagleEyeCamera(eagleEyeCamera);

            tutorialController = new TutorialController();
            tutorialController.Initialize(configuration, gameObject);
        }
    }
}