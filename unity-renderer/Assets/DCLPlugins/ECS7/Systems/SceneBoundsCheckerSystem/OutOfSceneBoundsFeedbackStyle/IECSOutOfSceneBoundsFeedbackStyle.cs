using DCL.ECS7.InternalComponents;
using DCL.Models;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public interface IECSOutOfSceneBoundsFeedbackStyle
    {
        void ApplyFeedback(IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel, bool isVisible, bool isInsideBounds);
    }
}
