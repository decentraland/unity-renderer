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

        [SerializeField] internal Animator teacherAnimator;

        /// <summary>
        /// Play an animation.
        /// </summary>
        /// <param name="animation">Animation to play.</param>
        public void PlayAnimation(TeacherAnimation animation)
        {
            if (!isActiveAndEnabled)
                return;

            switch (animation)
            {
                case TeacherAnimation.StepCompleted:
                    teacherAnimator.SetTrigger("StepCompleted");
                    break;
                case TeacherAnimation.QuickGoodbye:
                    teacherAnimator.SetTrigger("QuickGoodbye");
                    break;
                case TeacherAnimation.Reset:
                    teacherAnimator.SetTrigger("Reset");
                    break;
                default:
                    break;
            }
        }
    }
}