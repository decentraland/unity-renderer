using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// This class controls the behaviour of the teacher (a 3D model character) that will be guiding to the player along the tutorial.
    /// </summary>
    public class TutorialTeacher : MonoBehaviour
    {
        public enum TeacherAnimation
        {
            StepCompleted,
            QuickGoodbye,
            Reset
        }

        [SerializeField] private Animator teacherAnimator;
        [SerializeField] private AudioEvent audioEventHappy, audioEventNormal;


        public bool IsHiddenByAnimation { get; private set; } = false;


        public void PlayAnimation(TeacherAnimation animation)
        {
            if (!isActiveAndEnabled)
                return;

            switch (animation)
            {
                case TeacherAnimation.StepCompleted:
                    teacherAnimator.SetTrigger("StepCompleted");
                    PlayHappySound(0.3f);
                    break;
                case TeacherAnimation.QuickGoodbye:
                    teacherAnimator.SetTrigger("QuickGoodbye");
                    IsHiddenByAnimation = true;
                    break;
                case TeacherAnimation.Reset:
                    teacherAnimator.SetTrigger("Reset");
                    IsHiddenByAnimation = false;
                    break;
                default:
                    break;
            }
        }

        public void PlaySpeakSound() { audioEventNormal.PlayScheduled(0.4f); }

        public void PlayHappySound(float delay) { audioEventHappy.PlayScheduled(delay); }

        private void OnEnable()
        {
            teacherAnimator.enabled = true;
        }

        private void OnDisable()
        {
            teacherAnimator.StopPlayback();
            teacherAnimator.enabled = false;
        }
    }
}