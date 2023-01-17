
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public interface IECSOutOfSceneBoundsFeedbackStyle
    {
        void ApplyFeedback(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData,
            IECSReadOnlyComponentData<InternalVisibility> visibilityComponentData, bool isInsideBounds);
    }
}
