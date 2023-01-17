
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSOutOfSceneBoundsFeedback_EnabledToggle : IECSOutOfSceneBoundsFeedbackStyle
    {
        public void ApplyFeedback(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData, IECSReadOnlyComponentData<InternalVisibility> visibilityComponentData, bool isInsideBounds)
        {
            if (sbcComponentData.model.renderers != null)
            {
                int count = sbcComponentData.model.renderers.Count;
                for (var i = 0; i < count; i++)
                {
                    sbcComponentData.model.renderers[i].enabled = isInsideBounds
                                                                  && (visibilityComponentData == null
                                                                  || visibilityComponentData.model.visible);
                }
            }

            if (sbcComponentData.model.physicsColliders != null)
            {
                int count = sbcComponentData.model.physicsColliders.Count;
                for (var i = 0; i < count; i++)
                {
                    sbcComponentData.model.physicsColliders[i].enabled = isInsideBounds;
                }
            }

            if (sbcComponentData.model.pointerColliders != null)
            {
                int count = sbcComponentData.model.pointerColliders.Count;
                for (var i = 0; i < count; i++)
                {
                    sbcComponentData.model.pointerColliders[i].enabled = isInsideBounds;
                }
            }
        }
    }
}
