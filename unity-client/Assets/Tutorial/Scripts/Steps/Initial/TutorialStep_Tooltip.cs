using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents one of the tooltip steps included in the onboarding tutorial.
    /// </summary>
    public class TutorialStep_Tooltip : TutorialStep, IPointerDownHandler
    {
        [SerializeField] protected RectTransform tooltipTransform;
        [SerializeField] bool setMaxTimeToHide = false;
        [SerializeField] float maxTimeToHide = 5f;

        private bool tooltipStarted = false;
        private float timeSinceWasOpened = 0f;
        protected bool stepIsFinished = false;
        protected bool isRelatedFeatureActived = false;

        protected virtual void Update()
        {
            if (!setMaxTimeToHide || !tooltipStarted)
                return;

            timeSinceWasOpened += Time.deltaTime;

            if (timeSinceWasOpened >= maxTimeToHide)
                stepIsFinished = true;
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            stepIsFinished = true;
        }

        public override void OnStepStart()
        {
            base.OnStepStart();

            SetTooltipPosition();

            if (tutorialController != null)
                tutorialController.SetTeacherPosition(teacherPositionRef.position);

            tooltipStarted = true;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        public override IEnumerator OnStepPlayHideAnimation()
        {
            yield return base.OnStepPlayHideAnimation();
            yield return new WaitUntil(() => !isRelatedFeatureActived);
        }

        protected virtual void SetTooltipPosition()
        {
        }
    }
}