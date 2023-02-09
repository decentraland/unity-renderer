using DCL.ECS7.InternalComponents;
using DCL.Models;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSOutOfSceneBoundsFeedback_EnabledToggle : IECSOutOfSceneBoundsFeedbackStyle
    {
        public void ApplyFeedback(IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel, bool isVisible, bool isInsideBounds)
        {
            if (sbcComponentModel.renderers != null)
            {
                int count = sbcComponentModel.renderers.Count;

                for (var i = 0; i < count; i++)
                {
                    sbcComponentModel.renderers[i].enabled = isInsideBounds && isVisible;
                }
            }

            if (sbcComponentModel.physicsColliders != null)
            {
                int count = sbcComponentModel.physicsColliders.Count;

                for (var i = 0; i < count; i++)
                {
                    sbcComponentModel.physicsColliders[i].enabled = isInsideBounds;
                }
            }

            if (sbcComponentModel.pointerColliders != null)
            {
                int count = sbcComponentModel.pointerColliders.Count;

                for (var i = 0; i < count; i++)
                {
                    sbcComponentModel.pointerColliders[i].enabled = isInsideBounds;
                }
            }
        }
    }
}
