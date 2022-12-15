using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;

namespace DCL.ECSComponents
{
    public static class PointerHoverFeedbackExtensions
    {
        public static IReadOnlyList<PBPointerHoverFeedback.Types.Entry> GetPointerEventsForEntity(this ECSComponent<PBPointerHoverFeedback> component,
            IParcelScene scene, IDCLEntity entity)
        {
            var componentData = component.Get(scene, entity);
            return componentData?.model.PointerEvents;
        }
    }
}
